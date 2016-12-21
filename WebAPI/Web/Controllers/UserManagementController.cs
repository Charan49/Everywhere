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
using SendGrid;
using System.Configuration;
using Exceptions;
using System.Text;
using SendGrid.SmtpApi;
using System.Web.Http.Tracing;
using WebApi.ErrorHelper;
using System.Net.Mail;

namespace Web.Controllers
{
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UserManagementController : ApiController
    {
        private InterceptDB db = new InterceptDB();

        [Route("api/v1/users")]
        [HttpGet]
        public async Task<IEnumerable<JUser>> GetUsers()
        {
            var users = await db.Users.Select(x => new JUser { id = x.UserID, email = x.Email, firstName = x.FirstName, lastName = x.LastName, accountState = x.AccountState, guid = x.UserGUID }).ToListAsync();
            if (users.Count > 0)
                return users;
            throw new ApiDataException(1000, "Users not found", HttpStatusCode.NotFound);
        }

        [Route("api/v1/users/getId/{email}")]
        [HttpGet]
        public async Task<IEnumerable<JUser>> GetUserWithEmail(string email)
        {
            var users = await db.Users.Where(x => x.Email == email).Select(x => new JUser { id = x.UserID, email = x.Email, firstName = x.FirstName, lastName = x.LastName, guid = x.UserGUID, accountState = x.AccountState }).ToListAsync();
            if (users.Count > 0)
                return users;
            throw new ApiDataException(1001, "No user found for this id.", HttpStatusCode.NotFound);
        }

        [Route("api/v1/users/type/{userType}")]
        [HttpGet]
        public async Task<IEnumerable<JUser>> GetAdminUsers(string userType)
        {
            var users = await db.Users.Where(x => x.UserType == userType).Select(x => new JUser { id = x.UserID, email = x.Email, firstName = x.FirstName, lastName = x.LastName, accountState = x.AccountState, guid = x.UserGUID }).ToListAsync();
            if(userType.Equals("Admin"))
                users.Remove(users.Find(s=>s.email.Equals(this.ApiUser().Email)));
            if (users.Count > 0)
                return users;
            throw new ApiDataException(1000, "Users not found", HttpStatusCode.NotFound);
        }


        [Route("api/v1/users/{id}")]
        [HttpGet]
        public async Task<IEnumerable<JUser>> GetUserWithID(int id)
        {
            var users = await db.Users.Where(x => x.UserID == id).Select(x => new JUser { id = x.UserID, email = x.Email, firstName = x.FirstName, lastName = x.LastName, guid = x.UserGUID, accountState = x.AccountState}).ToListAsync();
            if (users.Count > 0)
                return users;
            throw new ApiDataException(1001, "No user found for this id.", HttpStatusCode.NotFound);
        }

        // POST: api/users
        [Route("api/v1/users")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostUser(JUserAdd newUser)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.BadRequest, ErrorDescription = "Bad Request..." };
            }

            if (Enum.IsDefined(typeof(Models.Enums.UserType), newUser.role))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Role Not Supported");
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.BadRequest, ErrorDescription = "Role Not Supported" };
            }


            //Check if Already Exists
            int count = await db.Users.CountAsync(x => String.Compare(x.Email, newUser.email, true) == 0 && String.Compare(x.UserType, newUser.role, true)==0);

            if (count != 0)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Conflict, "Already Exists");
                throw new ApiDataException(1002, "User is already exist in system.", HttpStatusCode.Conflict);
            }
            string vCode = GenerateCode.CreateRandomCode(4);

            //Create New User
            User user = new User()
            {
                UserGUID = Guid.NewGuid(),
                Email = newUser.email,
                Password = Helper.PasswordHash.HashPassword(newUser.Password),

                FirstName = newUser.firstName,
                LastName = newUser.lastName,
                ConfirmationCode = vCode,
                ConfirmationDueDate = DateTime.Now,

                AccountState = (byte)Models.Enums.AccountState.UnconfirmedEmail,
                UserType = newUser.role,

                CreatedDate = DateTime.UtcNow,
                CreatedBy = this.ApiUser().Email
            };


            MailMessage message = new MailMessage("test@test.com", newUser.email);




            message.Subject = "Everywhere Admin account";


            message.Body = "Hi " + user.FirstName + ",<br />" +
                                "<br />" +
                                "Welcome to Everywhere. Admin account is created for you. Please complete the registration by entering the following verification code: <b>" + vCode + "</b> in the portal/app to complete registration. You are then ready to live stream videos through Everywhere platform.<br />" +
                                "Please click on the given URL to complete the registration " + newUser.callbackURL + "<br />" +
                                "<br />" +
                                "Best regards<br />" +
                                "Team Everywhere<br />" +
                                "www.Everywhere.live<br /> ";
            await SendEmail.sendMail(message);
            user.ConfirmationDueDate = DateTime.Now;
            user.ModifiedDate = DateTime.Now;
            user.ConfirmationCode = vCode;

            db.Users.Add(user);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.ExpectationFailed, ErrorDescription = "Cannot add the user" };
            }
            return Request.CreateResponse(HttpStatusCode.Created, user.UserID);
        }


        // PUT
        [Route("api/v1/users/{id}")]
        [HttpPut]
        public async Task<IHttpActionResult> PutUser(int id, [FromBody] JUser juser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.BadRequest, ErrorDescription = "Bad Request..." };
            }

            string vCode = GenerateCode.CreateRandomCode(4);

            User user = await db.Users.FirstOrDefaultAsync(x => x.UserID == id);
            if (user == null)
            {
                return NotFound();
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorDescription = "Bad Request..." };
            }

            //Check if the Email Address is Available
                int count = await db.Users.CountAsync(x => String.Compare(x.Email, juser.email, true) == 0 && x.AccountState != (byte)Models.Enums.AccountState.Deleted);

                if (count != 0)
                {
                    user.LastName = juser.lastName;
                    user.FirstName = juser.firstName;
                    user.ModifiedBy = this.ApiUser().Email;
                    user.ModifiedDate = DateTime.UtcNow;

                    //return Conflict();  //An Account with this Email Already Exists
                    //throw new ApiDataException(1002, "An Account with this Email Already Exists.", HttpStatusCode.Conflict);
                }
                else
                {
                   
                    user.Email = juser.email;
                    
                    if (!String.IsNullOrEmpty(juser.lastName))
                        user.LastName = juser.lastName;

                    var callbackURL = Url.Link("Default", new { controller = "VerifyCode", action = "Account" });
                    UriBuilder builder = new UriBuilder(callbackURL);
                    string newUri = builder.Uri.ToString();


                    MailMessage message = new MailMessage("test@test.com", juser.email);

                    message.Subject = "Everywhere Admin account";


                    message.Body = "Hi " + user.FirstName + ",<br />" +
                                        "<br />" +
                                        "Welcome to Everywhere. Admin account is created for you. Please complete the registration by entering the following verification code: <b>" + vCode + "</b> in the portal/app to complete registration. You are then ready to live stream videos through Everywhere platform.<br />" +
                                        "Please click on the given URL to complete the registration " + juser.callbackURL + "<br />" +
                                        "<br />" +
                                        "Best regards<br />" +
                                        "Team Everywhere<br />" +
                                        "www.Everywhere.live<br /> ";
                    await SendEmail.sendMail(message);
                    user.ConfirmationDueDate = DateTime.Now;
                    user.ModifiedDate = DateTime.Now;
                    user.ConfirmationCode = vCode;
                }
                

            

            db.Entry(user).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }


        // DELETE
        [Route("api/v1/users/{id}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteUser(int id)
        {
            User user = await db.Users.FirstOrDefaultAsync(x => x.UserID == id);
            if (user == null)
            {
                return NotFound();
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorDescription = "Bad Request..." };
            }

            //Mark as Deleted
            user.AccountState = (byte)Models.Enums.AccountState.Deleted;
            await db.SaveChangesAsync();

            return Ok();
        }


        // Disable
        [Route("api/v1/users/disable")]
        [HttpPost]
        public async Task<IHttpActionResult> DeleteUser([FromBody] ParamDisable param)
        {
            User user = await db.Users.FirstOrDefaultAsync(x => x.UserID == param.id);
            if (user == null)
            {
                return NotFound();
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorDescription = "Bad Request..." };
            }

            //Update State
            if (Convert.ToBoolean(param.disable))
                user.AccountState = (byte)Models.Enums.AccountState.Disabled;
            else
                user.AccountState = (byte)Models.Enums.AccountState.Active;

            user.ModifiedBy = this.ApiUser().Email;
            user.ModifiedDate = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Ok();
        }

    }
}
