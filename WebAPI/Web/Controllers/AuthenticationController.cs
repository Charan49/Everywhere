

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
using Web.Models.Json;
using System.Web.Http.Tracing;

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
            }


            //Check if User Exists and Account is Available
            var existingUser = await db.Users.FirstOrDefaultAsync(u => String.Compare(u.Email, loginInfo.email.Trim(), true) == 0 && u.AccountState >= (int)Models.Enums.AccountState.Available);
            if (existingUser == null)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }


            //Validate Password
            bool bLoginSuccess = Helper.PasswordHash.ValidatePassword(loginInfo.password, existingUser.Password);

            if (bLoginSuccess)
                bLoginSuccess = ValidateKey(loginInfo, existingUser);


            //Generate Final Response
            if (bLoginSuccess == false)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
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
                string vCode = GenerateCode.CreateRandomCode(8);
                var callbackUrl = Url.Link("Default", new { Controller = "Account", Action = "ResetPassword", email = model.email });
                var myMessage = new SendGridMessage();
                myMessage.AddTo(model.email);
                myMessage.From = new System.Net.Mail.MailAddress(
                                    "Everywherewebvideo@gmail.com");
                myMessage.Subject = "Reset Password";
                myMessage.Text = "Please reset your password by clicking on this link. " + callbackUrl;
                myMessage.Html = "";
                await SendConfirmationEmail.sendMail(myMessage);
                
                emailAddress.VerificationCode = vCode;
                await db.SaveChangesAsync();
                return Ok();
            }
            else
                return NotFound();
        }

        [AllowAnonymous]
        [Route("api/v1/ConfirmEmail")]
        [HttpPost]
        public async Task<IHttpActionResult> ConfirmEmail([FromBody] JConfirmEmail code)
        {
            User emailAddress = await db.Users.FirstOrDefaultAsync(x => x.VerificationCode == code.code);
            if (emailAddress != null)
            {
                emailAddress.AccountState = (byte)Models.Enums.AccountState.Active;
                emailAddress.VerificationCode = string.Empty;
                await db.SaveChangesAsync();
                return Ok();
            }
            else
                return NotFound();
        }

        [AllowAnonymous]
        [Route("api/v1/ResetPassword/{email}")]
        [HttpPut]
        public async Task<IHttpActionResult> ResetPassword(string email, [FromBody] ResetPasswordModel model)
        {
            User emailAddress = await db.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (emailAddress != null)
            {
                emailAddress.Password = Helper.PasswordHash.HashPassword(model.newPassword);

                await db.SaveChangesAsync();
                return Ok();
            }
            else
                return NotFound();

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
