using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Web.Models;
using System.Data.Entity;
using Web.Helper;
using Web.Models.Json;
using System.Threading.Tasks;
using System.Web.Http.Cors;
using System.Configuration;
using SendGrid;
using System.Net.Mail;
using System.Web.Services.Description;
using Exceptions;
using System.Text;
using WebApi.ErrorHelper;
using System.Text.RegularExpressions;
using NLog;
using System.Runtime.Remoting.Contexts;
using System.Web;

namespace Web.Controllers
{
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UserController : ApiController
    {
        private InterceptDB db = new InterceptDB();
        

        [AllowAnonymous]
        [Route("api/v1/user/register")]
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<HttpResponseMessage> Register([FromBody] JRegister model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.BadRequest, ErrorDescription = "Bad Request..." };
            }
            string linkUrl = string.Empty;
            
            string vEmailCode = GenerateCode.CreateRandomCode(4);

            //Create user and save to database
            //////////////////////////////////
            User user = null;

            //Create First & Last Names from provided name            
            int index = model.name.LastIndexOf(' ');

            string FirstName = "";
            string LastName = "";

            if (index == -1)
                FirstName = model.name;
            else
            {
                FirstName = model.name.Substring(0, index).Trim();
                LastName = model.name.Substring(index).Trim();
            }

            user = new User
            {
                FirstName = FirstName,
                LastName = LastName,
                UserType = "User",

                AccountState = (byte)Models.Enums.AccountState.UnconfirmedEmail,

                Email = model.email.Trim(),
                Password = Helper.PasswordHash.HashPassword(model.password),
                EmailVerificationCode = vEmailCode,
                ConfirmationDueDate = DateTime.Now,
                CreatedDate = DateTime.UtcNow,
                UserGUID = Guid.NewGuid(),
                MobileNumber=model.mobilenumber
            };


            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    //Check if a User with Same E-mail Already Exists
                    int count = await db.Users.CountAsync(u => String.Compare(u.Email, model.email.Trim(), true) == 0 && u.AccountState != (byte)Models.Enums.AccountState.Deleted && String.Compare(u.UserType, "User", true) == 0);
                    if (count > 0)
                    {
                        int unConfirmed = await db.Users.CountAsync(u => String.Compare(u.Email, model.email.Trim(), true) == 0 && u.AccountState == (byte)Models.Enums.AccountState.UnconfirmedEmail && String.Compare(u.UserType, "User", true) == 0);
                        if(unConfirmed>0)
                            return Request.CreateResponse(HttpStatusCode.Found);
                        return Request.CreateErrorResponse(HttpStatusCode.Conflict, "User already exists.");
                        throw new ApiDataException(1002, "User is already exist in system.", HttpStatusCode.Conflict);

                    }

                    if (!string.IsNullOrEmpty(model.url))
                        linkUrl = " Please click the link below to enter your verification codes: " + model.url + "<br />";


                    MailMessage message = new MailMessage("test@test.com", model.email);
                    message.Subject= "Everywhere signup confirmation";
                    message.Body = "Hi " + user.FirstName + ",<br />" +
                                        "<br />" +
                                        "Thanks for signing up to Everywhere. We sent a phone number verification code to your mobile phone as a text message.  Please complete the sign up by entering the code in app/web to complete the signup. <br />" +
                                        linkUrl +" <br />"+
                                        "You are then ready to live stream videos through Everywhere platform. <br />" +
                                        "Best regards<br />" +
                                        "Team Everywhere<br />" +
                                        "www.Everywhere.live<br /> ";
                    await SendEmail.sendMail(message);
                    string vMobileCode = GenerateCode.CreateRandomCode(4);
                    await SMS.SendSMS(model.mobilenumber,vMobileCode);
                    user.MobileConfirmationCode = vMobileCode;
                    db.Users.Add(user);
                    db.SaveChanges();

                    //Complete Transaction
                    transaction.Commit();

                    return Request.CreateResponse(HttpStatusCode.Created);
                }
                catch
                {
                    //Rollback
                    transaction.Rollback();

                    throw new ApiException() { ErrorCode = (int)HttpStatusCode.InternalServerError, ErrorDescription = "Please try again...  " };
                }
            }
        }

        [AllowAnonymous]
        [Route("api/v1/user/ResendVerificationEmail")]
        [HttpPost]
        public async Task<IHttpActionResult> ResendVerificationEmail(JForgetPassword model)
        {
            string linkUrl = string.Empty;
            User emailAddress = await db.Users.FirstOrDefaultAsync(x => x.Email == model.email);
            if (emailAddress != null)
            {
                string vEmailCode = GenerateCode.CreateRandomCode(4);

                if (!string.IsNullOrEmpty(model.callbackURL))
                    linkUrl = " Please click the link below to enter your verification codes: " + model.callbackURL + "<br />";

                MailMessage message = new MailMessage("test@test.com", model.email);
                message.Subject = "Everywhere signup confirmation";
                message.Body = "Hi " + emailAddress.FirstName + ",<br />" +
                                        "<br />" +
                                        "To finish setting up your Everywhere account, we just need to make sure this email address is yours. Here is your email address verification code: <b>" + vEmailCode + "</b> Likewise we sent a phone number verification code to your mobile phone as a text message. < br />" +
                                        linkUrl + " <br />" +
                                        "If you didn't request this code, you can safely ignore this email. Someone else might have typed your email address by mistake. <br />" +
                                        "Best regards<br />" +
                                        "Team Everywhere<br />" +
                                        "www.Everywhere.live<br /> ";
                await SendEmail.sendMail(message);
                string vMobileCode = GenerateCode.CreateRandomCode(4);

                await SMS.SendSMS(emailAddress.MobileNumber, vMobileCode);
                emailAddress.ConfirmationDueDate = DateTime.Now;
                emailAddress.ModifiedDate = DateTime.Now;
                emailAddress.MobileConfirmationCode = vMobileCode;
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
        [Route("api/v1/user/check")]
        [HttpPost]
        public async Task<IHttpActionResult> CheckUser([FromBody] JUserCheck model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.BadRequest, ErrorDescription = "Bad Request..." };
            }

            int count = await db.Users.CountAsync(x => String.Compare(x.Email, model.email, true) == 0 && x.AccountState != (byte)Models.Enums.AccountState.Deleted);

            if (count == 0)
            {
                return NotFound();
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorDescription = "Bad Request..." };
            }
            else
            {
                return Conflict();
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.Conflict, ErrorDescription = "Bad Request..." };
            }
        }


        [Route("api/v1/user")]
        [HttpGet]
        public async Task<IEnumerable<JUser>> GetUserInfo()
        {
            var ruser = this.ApiUser().RUser;
            var users = await db.Users.Where(x => x.UserGUID == ruser.SubjectID).Select(x => new JUser { id = x.UserID, email = x.Email, firstName = x.FirstName, lastName = x.LastName }).ToListAsync();
            if (users.Count > 0)
                return users;
            else
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorDescription = "Bad Request..." };
        }



        // PUT
        [Route("api/v1/user")]
        [HttpPut]
        public async Task<IHttpActionResult> PutUser([FromBody] JUser juser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.BadRequest, ErrorDescription = "Bad Request..." };
            }

            juser.email = juser.email.Trim();

            var ruser = this.ApiUser().RUser;

            User user = await db.Users.FirstOrDefaultAsync(x => x.UserGUID == ruser.SubjectID);
            if (user == null)
            {
                return NotFound();
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorDescription = "Bad Request..." };
            }

            //Update Settings
            if (String.Compare(user.Email, juser.email, true) != 0)
            {
                //Check if the Email Address is Available
                int count = await db.Users.CountAsync(x => String.Compare(x.Email, juser.email, true) == 0 && x.AccountState != (byte)Models.Enums.AccountState.Deleted);

                if (count != 0)
                {
                    return Conflict();  //An Account with this Email Already Exists
                    throw new ApiDataException(1002, "User is already exist in system.", HttpStatusCode.Conflict);
                }
                else
                    user.Email = juser.email;
            }

            user.FirstName = juser.firstName;
            user.ModifiedBy = ruser.Email;
            user.ModifiedDate = DateTime.UtcNow;

            if (!String.IsNullOrEmpty(juser.lastName))
                user.LastName = juser.lastName;

            db.Entry(user).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

        private string GetHeaderValue(string header)
        {
            return Request.Headers.GetValues(header).FirstOrDefault();
        }



    }
}
