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
using Facebook;
using System.Configuration;
using WebApi.ErrorHelper;

namespace Web.Controllers
{
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ServiceMembershipController : ApiController
    {
        private InterceptDB db = new InterceptDB();

        [Route("api/v1/service_membership/{id}")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddServiceMembership([FromBody] JServiceUpdate model)    //Service GUID
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.BadRequest, ErrorDescription = "Bad Request..." };
            }

            Guid ServiceID;
            Guid.TryParse(model.id.ToString().ToUpper(), out ServiceID);
            
            //Check if Service is Present
            var service = await db.Services.FirstOrDefaultAsync(x => x.ServiceGUID == ServiceID && x.IsDeleted == false);
            if (service == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorDescription = "Bad Request..." };
            }

            var rUser = this.ApiUser().RUser;
            
            //Check if the Service Entry Already Exists
            var entry = await db.UserServices.FirstOrDefaultAsync(x => x.ServiceGUID == ServiceID && x.UserGUID == rUser.SubjectID);


            //Get Long-Term Token  
            string tokenLongTerm = "";
            string tokenExpiration = "";
                     
            if (service.Name == "Facebook")
            {
                //Create a Long Term Facebook Token
                FacebookClient client = new FacebookClient();
                client.AccessToken = model.accessToken;
                client.AppId = ConfigurationManager.AppSettings.Get("FacebookAppId");
                client.AppSecret = ConfigurationManager.AppSettings.Get("FacebookAppSecret");
                
                dynamic ret = await client.GetTaskAsync(string.Format("oauth/access_token?grant_type=fb_exchange_token&fb_exchange_token={0}&client_id={1}&client_secret={2}", model.accessToken, client.AppId, client.AppSecret), new { });

                tokenLongTerm = ret.access_token;
                tokenExpiration = DateTime.UtcNow.AddDays(60).ToString();
            }


            //Create New or Use Existing
            if (entry == null)
            {
                var newUserService = new UserService
                {
                    AccessID = Guid.NewGuid(),
                    ServiceGUID = model.id,
                    AccessToken = tokenLongTerm,
                    TokenExpiration = tokenExpiration,
                    UserGUID = this.ApiUser().RUser.SubjectID,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    CreatedBy = this.ApiUser().RUser.SubjectID.ToString(),
                    PictureURL = model.pictureURL
                };

                db.UserServices.Add(newUserService);
            }
            else
            {
                entry.AccessToken = tokenLongTerm;
                entry.TokenExpiration = tokenExpiration;
                entry.PictureURL = model.pictureURL;
                entry.IsDeleted = false;                
                db.Entry(entry).State = EntityState.Modified;
            }            

            //Save
            await db.SaveChangesAsync();

            var testUsers = db.TestUsers.FirstOrDefault(x => x.UserGUID == rUser.SubjectID && x.IsDeleted == false && x.IsLinked==false);
            testUsers.IsLinked = true;
            db.Entry(testUsers).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.Created);
        }
                

        [Route("api/v1/service_membership/{id}")]
        [HttpDelete]
        public async Task<IHttpActionResult> RemoveServiceMembership(Guid id)
        {
            var ruser = this.ApiUser().RUser;

            var entry = await db.UserServices.FirstOrDefaultAsync(x => x.UserGUID == ruser.SubjectID && x.ServiceGUID == id && x.IsDeleted == false);

            if (entry == null)
            {
                return NotFound();
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorDescription = "Bad Request..." };
            }

            //Mark as Deleted and Remove any User Info
            entry.IsDeleted = true;
            entry.PictureURL = "";
            entry.StreamDate = null;
            entry.StreamID = "";
            entry.StreamKey = "";
            entry.StreamURL = "";
            entry.AccessToken = "";
            entry.TokenExpiration = "";

            db.Entry(entry).State = EntityState.Modified;
            await db.SaveChangesAsync();

            var testUsers = await db.TestUsers.FirstOrDefaultAsync(x => x.UserGUID == ruser.SubjectID && x.IsDeleted==false);


            FacebookClient client = new FacebookClient();
            client.AppId = ConfigurationManager.AppSettings.Get("FacebookAppId");
            client.AppSecret = ConfigurationManager.AppSettings.Get("FacebookAppSecret");
            dynamic result = await client.GetTaskAsync("oauth/access_token", new
            {
                client_id = client.AppId,
                client_secret = client.AppSecret,
                grant_type = "client_credentials",
            });


            client.AccessToken = result[0];
            dynamic result1 = client.Delete(testUsers.FaceBookID, new
            {
                access_token = result[0],

            });

          
            testUsers.IsDeleted = true;
            testUsers.IsLinked = false;
            db.Entry(testUsers).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return Ok();
        }

        [Route("api/v1/service_membership/{id}")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateServiceMembership(Guid id, JServiceUpdate model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.BadRequest, ErrorDescription = "Bad Request..." };
            }

            var ruser = this.ApiUser().RUser;

            var entry = await db.UserServices.FirstOrDefaultAsync(x => x.UserGUID == ruser.SubjectID && x.ServiceGUID == id && x.IsDeleted == false);

            if (entry == null)
            {
                return NotFound();
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorDescription = "Bad Request..." };
            }

            entry.AccessToken = model.accessToken;
            entry.TokenExpiration = model.tokenExpiresAt;

            db.Entry(entry).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return Ok();
        }


        [Route("api/v1/service_membership")]
        [HttpGet]
        public async Task<IEnumerable<JServiceMembership>> GetServiceMembership()
        {
            var ruser = this.ApiUser().RUser;
            var ret= await db.UserServices.Where(x => x.UserGUID == ruser.SubjectID && x.IsDeleted == false).Select(x => new JServiceMembership { name = x.Service.Name, id = x.ServiceGUID, authenticationMethod = x.Service.AuthMethod, serviceProviderInfo = x.Service.ServiceProviderInfo, streamId = x.StreamID, streamUrl = x.StreamURL, streamKey = x.StreamKey, streamDate = (x.StreamDate == null? "" : x.StreamDate.ToString()), pictureUrl = x.PictureURL }).ToListAsync();
            return ret;
        }

        private string GetHeaderValue(string header)
        {
            return Request.Headers.GetValues(header).FirstOrDefault();
        }
    }
}
