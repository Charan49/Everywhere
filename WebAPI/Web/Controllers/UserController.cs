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

namespace Web.Controllers
{    
    [Authorize]
    public class UserController : ApiController
    {
        private InterceptDB db = new InterceptDB();


        [AllowAnonymous]
        [Route("api/v1/user/register")]
        [HttpPost]
        public async Task<HttpResponseMessage> Register([FromBody] JRegister model)
        {
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);



            //Create user and save to database
            //////////////////////////////////
            User user = null;

            //Create First & Last Names from provided name            
            int index = model.Name.LastIndexOf(' ');

            string FirstName = "";
            string LastName = "";

            if (index == -1)
                FirstName = model.Name;
            else
            {
                FirstName = model.Name.Substring(0, index).Trim();
                LastName = model.Name.Substring(index).Trim();
            }

            user = new User
            {
                FirstName = FirstName,
                LastName = LastName,
                UserType = "User",

                AccountState = (byte)Models.Enums.AccountState.UnconfirmedEmail,

                Email = model.Email,
                Password = Helper.PasswordHash.HashPassword(model.Password),

                CreatedDate = DateTime.UtcNow
            };


            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    //Check if a User with Same E-mail Already Exists
                    int count = await db.Users.CountAsync(u => String.Compare(u.Email, model.Email, true) == 0 && u.AccountState != (byte)Models.Enums.AccountState.Deleted);
                    if (count > 0)
                        return Request.CreateResponse(HttpStatusCode.Conflict, "User already exists.");

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

                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Please try again");
                }
            }
        }

        [AllowAnonymous]
        [Route("api/v1/user/check")]
        [HttpPost]
        public async Task<IHttpActionResult> CheckUser()
        {
            IEnumerable<string> headerValues = Request.Headers.GetValues("Email");
            var Email = headerValues.FirstOrDefault();

            int count = await db.Users.CountAsync(x => String.Compare(x.Email, Email, true) == 0 && x.AccountState != (byte)Models.Enums.AccountState.Deleted);

            if (count == 0)
                return NotFound();
            else
                return Conflict();
        }


        [Route("api/v1/user")]
        [HttpGet]        
        public async Task<IEnumerable<JUser>> GetUserInfo()
        {
            var ruser = this.ApiUser().RUser;
            return await db.Users.Where(x => x.UserGUID == ruser.SubjectID).Select(x => new JUser { id = x.UserID, email = x.Email, firstName = x.FirstName, lastName = x.LastName }).ToListAsync();
        }

        

        // PUT
        [Route("api/v1/user")]
        [HttpPut]
        public async Task<IHttpActionResult> PutUser([FromBody] JUser juser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ruser = this.ApiUser().RUser;

            User user = await db.Users.FirstOrDefaultAsync(x => x.UserGUID == ruser.SubjectID);
            if (user == null)
                return NotFound();

            //Update Settings
            if (String.Compare(user.Email, juser.email, true) != 0)
            {
                //Check if the Email Address is Available
                int count = await db.Users.CountAsync(x => String.Compare(x.Email, juser.email, true) == 0 && x.AccountState != (byte)Models.Enums.AccountState.Deleted);

                if (count != 0)
                    return Conflict();  //An Account with this Email Already Exists
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
