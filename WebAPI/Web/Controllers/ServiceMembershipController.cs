﻿using System;
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
using Google.Apis.YouTube.v3;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Auth.OAuth2;
using System.Threading;
using Newtonsoft.Json.Linq;

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
            string shortToken= "";
            string tokenExpiration = "";
            string longToken= "";

            if (service.Name == "Facebook")
            {
                //Create a Long Term Facebook Token
                FacebookClient client = new FacebookClient();
                client.AccessToken = model.accessToken;
                client.AppId = ConfigurationManager.AppSettings.Get("FacebookAppId");
                client.AppSecret = ConfigurationManager.AppSettings.Get("FacebookAppSecret");

                dynamic ret = await client.GetTaskAsync(string.Format("oauth/access_token?grant_type=fb_exchange_token&fb_exchange_token={0}&client_id={1}&client_secret={2}", model.accessToken, client.AppId, client.AppSecret), new { });

                shortToken = model.accessToken;
                longToken= ret.access_token;
                tokenExpiration = DateTime.UtcNow.AddDays(60).ToString();

                var testUsers = db.TestUsers.FirstOrDefault(x => x.UserGUID == rUser.SubjectID && x.IsDeleted == false && x.IsLinked == false);
                testUsers.IsLinked = true;
                db.Entry(testUsers).State = EntityState.Modified;
                await db.SaveChangesAsync();

            }

            if (service.Name == "YouTube")
            {
                string Code = model.accessToken;

                ClientSecrets secrets = new ClientSecrets()
                {
                    ClientId = ConfigurationManager.AppSettings.Get("YouTubeAppId"),
                    ClientSecret = ConfigurationManager.AppSettings.Get("YouTubeAppSecret")

                };

                IAuthorizationCodeFlow flow =
                    new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = secrets,
                        Scopes = new string[] { YouTubeService.Scope.Youtube }
                    });
                TokenResponse response = flow.ExchangeCodeForTokenAsync("", Code, "postmessage", CancellationToken.None).Result;

                var credentials = new UserCredential(new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer { ClientSecrets = secrets }),
               "TEST", response);

                shortToken = response.AccessToken;
                longToken = response.RefreshToken;
                tokenExpiration = response.Issued.AddSeconds(Convert.ToDouble(response.ExpiresInSeconds)).ToString();

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + response.AccessToken;

                    var response1 = client.DownloadString("https://www.googleapis.com/plus/v1/people/me");
                    JObject o = JObject.Parse(response1);
                    string pictureURL = (string)o.SelectToken("image.url");
                    model.pictureURL = pictureURL.Replace("sz=50", "sz=150");
                    model.fbUserID = (string)o.SelectToken("id");
                }
            }


                //Create New or Use Existing
                if (entry == null)
                {
                    var newUserService = new UserService
                    {
                        AccessID = Guid.NewGuid(),
                        ServiceGUID = model.id,
                        LongToken=longToken,
                        AccessToken = shortToken,
                        TokenExpiration = tokenExpiration,
                        UserGUID = this.ApiUser().RUser.SubjectID,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        CreatedBy = this.ApiUser().RUser.SubjectID.ToString(),
                        PictureURL = model.pictureURL,
                        fbUserID = model.fbUserID
                    };

                    db.UserServices.Add(newUserService);
                }
                else
                {
                if(!string.IsNullOrEmpty(longToken))
                    entry.LongToken = longToken;
                entry.AccessToken =shortToken;
                    entry.TokenExpiration = tokenExpiration;
                    entry.PictureURL = model.pictureURL;
                    entry.IsDeleted = false;
                    entry.fbUserID = model.fbUserID;
                    entry.PictureURL = model.pictureURL;
                    db.Entry(entry).State = EntityState.Modified;
                }

                //Save
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
            entry.fbUserID = "";

            db.Entry(entry).State = EntityState.Modified;
            await db.SaveChangesAsync();

            if (entry.Service.Name == "Facebook")
            {

                var testUsers = await db.TestUsers.FirstOrDefaultAsync(x => x.UserGUID == ruser.SubjectID && x.IsDeleted == false);


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
            }

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
            var ret = await db.UserServices.Where(x => x.UserGUID == ruser.SubjectID && x.IsDeleted == false).Select(x => new JServiceMembership { name = x.Service.Name, id = x.ServiceGUID, authenticationMethod = x.Service.AuthMethod, serviceProviderInfo = x.Service.ServiceProviderInfo, streamId = x.StreamID, streamUrl = x.StreamURL, streamKey = x.StreamKey, streamDate = (x.StreamDate == null ? "" : x.StreamDate.ToString()), pictureUrl = x.PictureURL, fbUserID = x.fbUserID }).ToListAsync();
            return ret;
        }

        [Route("api/v1/service_membership/{id}")]
        [HttpGet]
        public async Task<IEnumerable<JServiceMembership>> GetServiceMembershipWithGuid(Guid id)
        {
            var ret = await db.UserServices.Where(x => x.UserGUID == id && x.IsDeleted == false).Select(x => new JServiceMembership { name = x.Service.Name, id = x.ServiceGUID, authenticationMethod = x.Service.AuthMethod, serviceProviderInfo = x.Service.ServiceProviderInfo, streamId = x.StreamID, streamUrl = x.StreamURL, streamKey = x.StreamKey, streamDate = (x.StreamDate == null ? "" : x.StreamDate.ToString()), pictureUrl = x.PictureURL, fbUserID = x.fbUserID }).OrderBy(x => x.name).ToListAsync();
            //var ret = await db.UserServices.Where(x => x.UserGUID == id && x.IsDeleted == false).Select(x => new JServiceMembership { name = x.Service.Name, id = x.ServiceGUID, authenticationMethod = x.Service.AuthMethod, serviceProviderInfo = x.Service.ServiceProviderInfo, streamId = x.StreamID, streamUrl = x.StreamURL, streamKey = x.StreamKey, streamDate = (x.StreamDate == null ? "" : x.StreamDate.ToString()), pictureUrl = x.PictureURL, fbUserID = x.fbUserID }).ToListAsync();
            return ret;
        }

        
        [Route("api/v1/video/addvideoswitch")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddVideoSwitch([FromBody] JVideoSwitch model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.BadRequest, ErrorDescription = "Bad Request..." };
            }

            //////////////////////////////////
            //Create First & Last Names from provided name            

            VideoSwitchURL jvs = new VideoSwitchURL();


            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    //Check if a Video with Same E-mail Already Exists
                    int count = await db.VideoSwitchURLs.CountAsync(u => u.Id == model.id);
                    if (count > 0)
                    {
                        jvs.Id = model.id;
                        jvs.StreamURL = model.url;
                        jvs.ModifiedBy = this.ApiUser().RUser.SubjectID.ToString();
                        jvs.ModifiedDate = DateTime.UtcNow;
                        db.Entry(jvs).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        jvs.StreamURL = model.url;
                        jvs.VideoGUID = Guid.NewGuid();
                        jvs.IsDeleted = false;
                        jvs.CreatedDate = DateTime.UtcNow;
                        jvs.CreatedBy = this.ApiUser().RUser.SubjectID.ToString();
                        db.VideoSwitchURLs.Add(jvs);
                        db.SaveChanges();
                    }
                    

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

        
        [Route("api/v1/video/Getvideo")]
        [HttpGet]
        public async Task<IEnumerable<JVideoSwitch>> GetUrlInfo()
        {
            var videos = await db.VideoSwitchURLs.Select(x => new JVideoSwitch { id=x.Id, url=x.StreamURL}).ToListAsync();
            if (videos.Count > 0)
                return videos;
            else
                return null;
           //     throw new ApiException() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorDescription = "Bad Request..." };
        }

        private string GetHeaderValue(string header)
        {
            return Request.Headers.GetValues(header).FirstOrDefault();
        }
    }
}
