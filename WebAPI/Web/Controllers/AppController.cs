

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

namespace Web.Controllers
{
    public class AppController : ApiController
    {
        private InterceptDB db = new InterceptDB();
        
        [AllowAnonymous]
        [Route("api/v1/app/register")]
        [HttpPost]
        public async Task<HttpResponseMessage> Register([FromBody] JAppRegister model)
        {
            Guid uuid;

            if (!ModelState.IsValid || Guid.TryParse(model.UUID, out uuid) == false)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

            //Validate Registration Key
            if (model.DefaultKey != ConfigurationManager.AppSettings.Get("DefaultKeyAppRegister"))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Registration Key");


            //Create Application and save to database
            //////////////////////////////////
            App app = null;

            app = new App
            {
                DeviceID = uuid,
                AppName = model.AppName,
                OS = model.OS,
                Version = model.Version,

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
                        return Request.CreateResponse(HttpStatusCode.Conflict, "App already exists.");

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
