

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

namespace Web.Controllers
{
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
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                                    

            //Check if User Exists and Account is Available
            var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == loginInfo.email && u.AccountState >= (int)Models.Enums.AccountState.Available );
            if (existingUser == null)            
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
                                   

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
                var token = new Web.Models.Json.JToken()
                {
                    token = TokenController.CreateToken(loginInfo, existingUser),
                    id = existingUser.UserID.ToString()
                };
                
                return Request.CreateResponse(token);
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
        


        private bool ValidateKey(JLogin login, User user)
        {
            switch (user.UserType)
            {
                case "User":
                    {
                        Guid uuid;
                        if (String.IsNullOrEmpty(login.uuid) || Guid.TryParse(login.uuid, out uuid) == false)
                            return false;

                        var app = db.Apps.FirstOrDefault(x => x.DeviceID == uuid && x.IsDeleted == false);
                        if (app == null)
                            return false;

                        if (login.key == app.RegistrationToken)
                            return true;
                        else
                            return false;
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
