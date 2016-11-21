

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
using Web.Models.Json;
using System.Web.Http.Cors;

namespace Web.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AppController : ApiController
    {
        private InterceptDB db = new InterceptDB();
        
        [AllowAnonymous]
        [Route("api/v1/app/register")]
        [HttpPost]
        public async Task<HttpResponseMessage> Register([FromBody] JAppRegister model)
        {
            Guid uuid;

            if (!ModelState.IsValid || Guid.TryParse(model.uuid, out uuid) == false)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

            //Validate Registration Key
            if (model.defaultKey != ConfigurationManager.AppSettings.Get("DefaultKeyAppRegister"))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Registration Key");


            //Create Application and save to database
            //////////////////////////////////
            App app = null;

            app = new App
            {
                AppGUID = Guid.NewGuid(),
                DeviceID = uuid,
                AppName = model.appName,
                OS = model.os,
                Version = model.version,

                RegistrationToken = Common.GetUniqueKey(50),

                CreatedDate = DateTime.Now,
                IsDeleted = false
            };


            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    //Check if Application Already Exists
                    int count = await db.Apps.CountAsync(u => u.DeviceID == uuid && u.IsDeleted != true);
                    if (count > 0)
                        return Request.CreateErrorResponse(HttpStatusCode.Conflict, "App already exists.");

                    db.Apps.Add(app);
                    db.SaveChanges();
                                        
                    //Complete Transaction
                    transaction.Commit();

                    return Request.CreateResponse(HttpStatusCode.Created, new JRegistrationToken { registration_token = app.RegistrationToken });
                }
                catch
                {
                    //Rollback
                    transaction.Rollback();

                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Please try again");
                }
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
