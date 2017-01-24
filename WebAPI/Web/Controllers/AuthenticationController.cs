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
using System.Net.Mail;
using System.Threading;

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
            var existingUser = await db.Users.FirstOrDefaultAsync(u => String.Compare(u.Email, loginInfo.email.Trim(), true) == 0 && u.AccountState >= (int)Models.Enums.AccountState.Available && String.Compare(u.UserType, loginInfo.role.Trim(), true) == 0);
            if (existingUser == null)
            {
                var unconfirmed = await db.Users.FirstOrDefaultAsync(u => String.Compare(u.Email, loginInfo.email.Trim(), true) == 0 && u.AccountState == (int)Models.Enums.AccountState.UnconfirmedEmail);
                if (string.IsNullOrEmpty(unconfirmed.MobileNumber))
                    return Request.CreateResponse(HttpStatusCode.Found);
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.Unauthorized, ErrorDescription = "User is not authorized" };
            }

            if (string.IsNullOrEmpty(existingUser.MobileNumber) && existingUser.UserType.Equals("User"))
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable);
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
                    if (string.IsNullOrEmpty(existingUser.EmailVerificationCode) && existingUser.UserType.Equals("User"))
                    {
                        return Request.CreateResponse(HttpStatusCode.NotImplemented);
                    }
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
            string linkUrl = string.Empty;
            string bodyMessage = string.Empty;
            string messageSubject = string.Empty;
            User emailAddress = await db.Users.FirstOrDefaultAsync(x => x.Email == model.email && String.Compare(x.UserType, "User", true) == 0);
            if (!string.IsNullOrEmpty(model.phone))
                emailAddress.MobileNumber = model.phone;
            if (string.IsNullOrEmpty(emailAddress.MobileNumber) && emailAddress.UserType.Equals("User"))
            {
                return Unauthorized();
            }
            if (emailAddress != null)
            {
                //emailAddress.Password = Helper.PasswordHash.HashPassword(model.newPassword);
                
                
                string vEmailCode = GenerateCode.CreateRandomCode(4);
                MailMessage message = new MailMessage("test@test.com", model.email);

                if (!string.IsNullOrEmpty(model.callbackURL) && model.type.Equals("forgot"))
                {
                    messageSubject = "Everywhere password reset";
                    linkUrl = " Please click on the given URL to change your password: " + model.callbackURL + "<br />";
                    bodyMessage= "Hi " + emailAddress.FirstName + ", <br>" +
                                "You requested to reset your Everywhere account password. We sent a phone number verification code to your mobile phone as a text message. <br />" +
                                linkUrl +
                                "If you didn't request the password reset, you can safely ignore this email. Someone else might have typed your email address by mistake. <br />" +
                                        "Best regards <br>" +
                                        "Team Everywhere <br>" +
                                        "www.Everywhere.live <br> ";
                }
                if (!string.IsNullOrEmpty(model.callbackURL) && !model.type.Equals("forgot"))
                {
                    messageSubject = "Everywhere signup confirmation";
                    linkUrl = " Please click the link below to enter your verification codes: " + model.callbackURL + "<br />";
                    bodyMessage = "Hi " + emailAddress.FirstName + ",<br />" +
                                        "<br />" +
                                        "To finish setting up your Everywhere account, we just need to make sure this email address is yours. Here is your email address verification code: <b>" + vEmailCode + "</b> Likewise we sent a phone number verification code to your mobile phone as a text message. <br />" +
                                        linkUrl + " <br />" +
                                        "If you didn't request this code, you can safely ignore this email. Someone else might have typed your email address by mistake. <br />" +
                                        "Best regards<br />" +
                                        "Team Everywhere<br />" +
                                        "www.Everywhere.live<br /> ";
                }

                message.Subject = messageSubject;
               

                message.Body = bodyMessage;


                await SendEmail.sendMail(message);
                string vMobileCode = GenerateCode.CreateRandomCode(4);
                if (!emailAddress.UserType.Equals("Admin"))
                {
                    await SMS.SendSMS(emailAddress.MobileNumber, vMobileCode);
                    emailAddress.MobileConfirmationCode = vMobileCode;
                }
                emailAddress.ConfirmationDueDate = DateTime.Now;
                emailAddress.ModifiedDate = DateTime.Now;
                
                emailAddress.EmailVerificationCode = vEmailCode;
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
        [Route("api/v1/AddPhone")]
        [HttpPost]
        public async Task<IHttpActionResult> AddPhone([FromBody] JForgetPassword model)
        {
            string linkUrl = string.Empty;
            User emailAddress = await db.Users.FirstOrDefaultAsync(x => x.Email == model.email && String.Compare(x.UserType, "User", true) == 0);
            if (emailAddress != null)
            {
                //emailAddress.Password = Helper.PasswordHash.HashPassword(model.newPassword);

                string vEmailCode = GenerateCode.CreateRandomCode(4);
                MailMessage message = new MailMessage("test@test.com", model.email);

                if (!string.IsNullOrEmpty(model.callbackURL))
                    linkUrl = " Please click the link below to enter your verification codes: " + model.callbackURL + "<br />";

                message.Subject = "Everywhere phone number verification";


                message.Body = "Hi " + emailAddress.FirstName + ",<br />" +
                                        "<br />" +
                                        "To finish setting up your Everywhere account, we just need to make sure this email address is yours. Here is your email address verification code: <b>" + vEmailCode + "</b> Likewise we sent a phone number verification code to your mobile phone as a text message. <br />" +
                                        linkUrl + " <br />" +
                                        "If you didn't request this code, you can safely ignore this email. Someone else might have typed your email address by mistake. <br />" +
                                        "Best regards<br />" +
                                        "Team Everywhere<br />" +
                                        "www.Everywhere.live<br /> ";


                await SendEmail.sendMail(message);
                emailAddress.MobileNumber = model.phone;
                string vMobileCode = GenerateCode.CreateRandomCode(4);
                await SMS.SendSMS(emailAddress.MobileNumber, vMobileCode);
                emailAddress.MobileNumber = model.phone;
                emailAddress.AccountState = (byte)Models.Enums.AccountState.UnconfirmedEmail;
                emailAddress.ConfirmationDueDate = DateTime.Now;
                emailAddress.ModifiedDate = DateTime.Now;
                emailAddress.MobileConfirmationCode = vMobileCode;
                emailAddress.EmailVerificationCode = vEmailCode;
                await db.SaveChangesAsync();
                return Conflict();
            }
            else
            {
                return NotFound();
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorDescription = "Bad Request..." };
            }
        }

        [AllowAnonymous]
        [Route("api/v1/ForgotAdminPassword")]
        [HttpPost]
        public async Task<IHttpActionResult> ForgotAdminPassword([FromBody] JForgetPassword model)
        {
            string linkUrl = string.Empty;
            User emailAddress = await db.Users.FirstOrDefaultAsync(x => x.Email == model.email);
            
            if (emailAddress != null)
            {
                //emailAddress.Password = Helper.PasswordHash.HashPassword(model.newPassword);
                if (!string.IsNullOrEmpty(model.callbackURL))
                    linkUrl = "Please click on the given URL to change your password: " + model.callbackURL + "<br />";
                string vEmailCode = GenerateCode.CreateRandomCode(4);
                MailMessage message = new MailMessage("test@test.com", model.email);
                
                message.Subject = "Everywhere password reset";


                message.Body = "Hi " + emailAddress.FirstName + ", <br>" +
                                "On your request to reset the password a verification code has been sent to your mobile phone by text. Please enter the verification code in the portal/app to reset your password. You can then continue with live streaming of videos through Everywhere platform. <br>" +
                                linkUrl +
                                        "Best regards <br>" +
                                        "Team Everywhere <br>" +
                                        "www.Everywhere.live <br> ";

                await SendEmail.sendMail(message);
                
                emailAddress.ConfirmationDueDate = DateTime.Now;
                emailAddress.ModifiedDate = DateTime.Now;
                emailAddress.EmailVerificationCode = vEmailCode;
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
            User emailAddress = await db.Users.FirstOrDefaultAsync(x => x.MobileConfirmationCode == code.mobilecode);
            if (emailAddress != null)
            {
                TimeSpan ts = DateTime.Now.Subtract(Convert.ToDateTime(emailAddress.ConfirmationDueDate));

                if (Convert.ToInt32(ts.TotalDays) > 7)
                {
                    return NotFound();
                    throw new ApiDataException(1002, "Verification code expired...", HttpStatusCode.NotAcceptable);
                }
                emailAddress.AccountState = (byte)Models.Enums.AccountState.Active;
                if (code.codeType.Equals("mobile"))
                    emailAddress.MobileConfirmationCode = string.Empty;
                if(code.codeType.Equals("email"))
                    emailAddress.EmailVerificationCode = string.Empty;
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
            User emailAddress = null;
            if (model.usertype.Equals("User"))
                emailAddress = await db.Users.FirstOrDefaultAsync(x => x.MobileConfirmationCode == code && x.UserType.Equals(model.usertype));
            else
                emailAddress = await db.Users.FirstOrDefaultAsync(x => x.EmailVerificationCode == code && x.UserType.Equals(model.usertype));
            if (emailAddress != null)
            {
                TimeSpan ts = DateTime.Now.Subtract(Convert.ToDateTime(emailAddress.ConfirmationDueDate));

                if (Convert.ToInt32(ts.TotalDays) > 7)
                {
                    return NotFound();
                    throw new ApiDataException(1002, "Verification code expired...", HttpStatusCode.NotAcceptable);
                }
                emailAddress.Password = Helper.PasswordHash.HashPassword(model.newPassword);
                emailAddress.MobileConfirmationCode = string.Empty;
                emailAddress.EmailVerificationCode = string.Empty;
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
        [Route("api/v1/ResetAdminPassword/{code}")]
        [HttpPut]
        public async Task<IHttpActionResult> ResetAdminPassword(string code, [FromBody] ResetPasswordModel model)
        {
            User emailAddress = await db.Users.FirstOrDefaultAsync(x => x.MobileConfirmationCode == code && x.Email == model.email);
            if (emailAddress != null)
            {
                TimeSpan ts = DateTime.Now.Subtract(Convert.ToDateTime(emailAddress.ConfirmationDueDate));

                if (Convert.ToInt32(ts.TotalDays) > 7)
                {
                    return NotFound();
                    throw new ApiDataException(1002, "Verification code expired...", HttpStatusCode.NotAcceptable);
                }
                emailAddress.Password = Helper.PasswordHash.HashPassword(model.newPassword);
                emailAddress.AccountState = (byte)Models.Enums.AccountState.Active;
                emailAddress.EmailVerificationCode = string.Empty;
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
