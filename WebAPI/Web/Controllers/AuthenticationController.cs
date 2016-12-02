using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using Web.Models;
using JWT;
using Web.Helper;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Configuration;
using System.Web.Http.Cors;
using SendGrid;
using Exceptions;
using Web.Models.Json;
using System.Web.Http.Tracing;
using WebApi.ErrorHelper;
using System.Text.RegularExpressions;

namespace Web.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AuthenticationController : ApiController
    {
        private InterceptDB db = new InterceptDB();

        [AllowAnonymous]
        [Route("api/v1/authenticate")]
        [HttpPost]
        public async Task<HttpResponseMessage> Login(JLogin loginInfo)
        {
            //Validate Model
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.BadRequest, ErrorDescription = "Bad Request..." };
            }


            //Check if User Exists and Account is Available
            var existingUser = await db.Users.FirstOrDefaultAsync(u => String.Compare(u.Email, loginInfo.email.Trim(), true) == 0 && u.AccountState >= (int)Models.Enums.AccountState.Available);
            if (existingUser == null)
            {
                var unconfirmed = await db.Users.FirstOrDefaultAsync(u => String.Compare(u.Email, loginInfo.email.Trim(), true) == 0 && u.AccountState == (int)Models.Enums.AccountState.UnconfirmedEmail);
                if (unconfirmed != null)
                    return Request.CreateResponse(HttpStatusCode.Found);
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.Unauthorized, ErrorDescription = "User is not authorized" };
            }


            //Validate Password
            bool bLoginSuccess = Helper.PasswordHash.ValidatePassword(loginInfo.password, existingUser.Password);

            if (bLoginSuccess)
                bLoginSuccess = ValidateKey(loginInfo, existingUser);


            //Generate Final Response
            if (bLoginSuccess == false)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
                throw new ApiDataException(1002, "User is not authorized.", HttpStatusCode.Unauthorized);
            }
            else
            {
                try
                {
                    var token = new Web.Models.Json.JToken()
                    {
                        token = TokenController.CreateToken(loginInfo, existingUser),
                        id = existingUser.UserID.ToString(),
                        name = existingUser.FirstName + " " + existingUser.LastName
                    };

                    return Request.CreateResponse(token);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Conflict, "Cannot login user.");
                    throw new ApiDataException(1002, "Cannot login user.", HttpStatusCode.InternalServerError);
                }
            }
        }




        [Route("api/v1/signout")]
        [HttpGet]
        public async Task<IHttpActionResult> SignOut()
        {
            //Remove Token from Redis
            var user = this.ApiUser().RUser;
            await RedisDb.RedisCache.RemoveAsync("ApiUser:" + user.SubjectID + ":Session:" + user.SessionID);

            return Ok();
        }

        [AllowAnonymous]
        [Route("api/v1/ForgotPassword")]
        [HttpPost]
        public async Task<IHttpActionResult> ForgotPassword([FromBody] JForgetPassword model)
        {
            User emailAddress = await db.Users.FirstOrDefaultAsync(x => x.Email == model.email);
            if (emailAddress != null)
            {
                //emailAddress.Password = Helper.PasswordHash.HashPassword(model.newPassword);
                
                string vCode = GenerateCode.CreateRandomCode(4);
                var myMessage = new SendGridMessage();
                myMessage.AddTo(model.email);
                myMessage.From = new System.Net.Mail.MailAddress(
                                    "Everywherewebvideo@gmail.com");
                myMessage.Subject = "Everywhere password reset";
                myMessage.Text = "";
                myMessage.Html = "Hi " + emailAddress.FirstName + ", " +
                                "<br>" +
                                "Don’t fret. Please enter the following verification code: <b>" + vCode + "</b> in the portal/app to reset your password. You can then continue with live streaming of videos through Everywhere platform.<br>" +
                                        "<br>" +
                                        "Best regards <br>" +
                                        "Team Everywhere <br>" +
                                        "www.Everywhere.live <br> ";

                await SendConfirmationEmail.sendMail(myMessage);
                emailAddress.ConfirmationDueDate = DateTime.Now;
                emailAddress.ModifiedDate = DateTime.Now;
                emailAddress.ConfirmationCode = vCode;
                await db.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return NotFound();
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorDescription = "Bad Request..." };
            }
        }

        [AllowAnonymous]
        [Route("api/v1/ConfirmEmail")]
        [HttpPost]
        public async Task<IHttpActionResult> ConfirmEmail([FromBody] JConfirmEmail code)
        {
            User emailAddress = await db.Users.FirstOrDefaultAsync(x => x.ConfirmationCode == code.code);
            if (emailAddress != null)
            {
                TimeSpan ts = DateTime.Now.Subtract(Convert.ToDateTime(emailAddress.ConfirmationDueDate));

                if (Convert.ToInt32(ts.TotalDays) > 7)
                {
                    return NotFound();
                    throw new ApiDataException(1002, "Verification code expired...", HttpStatusCode.NotAcceptable);
                }
                emailAddress.AccountState = (byte)Models.Enums.AccountState.Active;
                emailAddress.ConfirmationCode = string.Empty;
                await db.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return NotFound();
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorDescription = "Bad Request..." };
            }
        }

        [AllowAnonymous]
        [Route("api/v1/ResetPassword/{code}")]
        [HttpPut]
        public async Task<IHttpActionResult> ResetPassword(string code, [FromBody] ResetPasswordModel model)
        {
            User emailAddress = await db.Users.FirstOrDefaultAsync(x => x.ConfirmationCode == code);
            if (emailAddress != null)
            {
                TimeSpan ts = DateTime.Now.Subtract(Convert.ToDateTime(emailAddress.ConfirmationDueDate));

                if (Convert.ToInt32(ts.TotalDays) > 7)
                {
                    return NotFound();
                    throw new ApiDataException(1002, "Verification code expired...", HttpStatusCode.NotAcceptable);
                }
                emailAddress.Password = Helper.PasswordHash.HashPassword(model.newPassword);
                emailAddress.ConfirmationCode = string.Empty;
                await db.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return NotFound();
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorDescription = "Bad Request..." };
            }

        }

        private bool ValidateKey(JLogin login, User user)
        {
            switch (user.UserType)
            {
                case "User":
                    {
                        return true;
                        //Guid uuid;
                        //if (String.IsNullOrEmpty(login.uuid) || Guid.TryParse(login.uuid, out uuid) == false)
                        //    return false;

                        //var app = db.Apps.FirstOrDefault(x => x.DeviceID == uuid && x.IsDeleted == false);
                        //if (app == null)
                        //    return false;

                        //if (login.key == app.RegistrationToken)
                        //    return true;
                        //else
                        //    return false;
                    }

                case "VideoSwitch":
                    {
                        if (login.key == ConfigurationManager.AppSettings.Get("DefaultKeyVideoSwitchLogin"))
                            return true;
                        else
                            return false;
                    }

                case "Admin":
                    {
                        if (login.key == ConfigurationManager.AppSettings.Get("DefaultKeyAdminLogin"))
                            return true;
                        else
                            return false;
                    }

                default:
                    return false;
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
