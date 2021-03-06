﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Web.Models.Json;
using System.Data.Entity;
using Web.Models;
using Web;
using System.Web.Http.Cors;
using Facebook;
using WebApi.ErrorHelper;
using Google.Apis.YouTube.v3;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Auth.OAuth2;
using System.Configuration;
using Newtonsoft.Json.Serialization;
using Web.Helper;

namespace Web.Controllers
{
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LiveStreamController : ApiController
    {
        private const string serviceFacebook = "Facebook";
        private const string serviceYoutube = "YouTube";
        private const string serviceYounow = "Younow";

        private InterceptDB db = new InterceptDB();

        [Route("api/v1/livestream/start")]
        [HttpPost]
        public async Task<List<JLiveStream>> CreateLiveStreamUrls([FromBody] JServiceName serviceNames)
        {
            var ruser = this.ApiUser().RUser;
            List<UserService> uServices = new List<UserService>();
            string[] values = serviceNames.serviceNames.Split(',');
            for (int i = 0; i < values.Length; i++)
            {
                string sName = values[i].Trim();
                if (!string.IsNullOrEmpty(sName))
                    uServices.Add(await db.UserServices.FirstOrDefaultAsync(x => x.UserGUID == ruser.SubjectID && x.Service.Name == sName && x.IsDeleted == false));
            }


            //var services = await db.UserServices.Where(x => x.UserGUID == ruser.SubjectID && x.IsDeleted == false).ToListAsync();
            var list = new List<JLiveStream>();

            foreach (var entry in uServices)
            {
                if (entry.Service.Name == serviceFacebook)
                {
                    //Create Facebook Live Stream
                    Facebook.FacebookClient client = new FacebookClient();
                    client.AccessToken = entry.LongToken;

                    //dynamic ret = await client.GetTaskAsync(string.Format("oauth/access_token?grant_type=fb_exchange_token&fb_exchange_token={0}&client_id={1}&client_secret={2}", model.accessToken, client.AppId, client.AppSecret), new { });



                    dynamic ret = await client.PostTaskAsync("/me/live_videos", new { });
                    string subUrl = ret.stream_url;
                    list.Add(new JLiveStream
                    {
                        serviceName = entry.Service.Name,
                        streamId = ret.id + subUrl.Substring(subUrl.IndexOf("?"), subUrl.Length - subUrl.IndexOf("?")),
                        streamUrl = ret.stream_url,
                        streamDate = DateTime.UtcNow
                    });

                    //entry.StreamID = ret.id;
                    entry.StreamID = ret.id + subUrl.Substring(subUrl.IndexOf("?"), subUrl.Length - subUrl.IndexOf("?"));
                    entry.StreamURL = ret.stream_url;
                    entry.StreamDate = DateTime.UtcNow;

                    db.Entry(entry).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
                else if (entry.Service.Name == serviceYoutube)
                {


                    ClientSecrets secrets = new ClientSecrets()
                    {
                        ClientId = ConfigurationManager.AppSettings.Get("YouTubeAppId"),
                        ClientSecret = ConfigurationManager.AppSettings.Get("YouTubeAppSecret")

                    };

                    var token = new TokenResponse { RefreshToken = entry.LongToken };
                    var credentials1 = new UserCredential(new GoogleAuthorizationCodeFlow(
                        new GoogleAuthorizationCodeFlow.Initializer
                        {
                            ClientSecrets = secrets
                        }), ConfigurationManager.AppSettings.Get("YouTubeAppId"), token);


                    var youTubeService = new YouTubeService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credentials1,

                        ApplicationName = "Everywhere"
                    });

                    var liveStream = new LiveStream
                    {

                        Cdn = new CdnSettings
                        {
                            Format = "360p",
                            IngestionType = "rtmp"
                        },
                        Snippet = new LiveStreamSnippet
                        {
                            Title = "test"

                        }

                    };
                    var request = youTubeService.LiveStreams.Insert(liveStream, "cdn,snippet");
                    var youtuberesponse = request.Execute();



                    ////Validate that the Token is Valid
                    //try
                    //{
                    //    //Create Facebook Live Stream
                    //    Facebook.FacebookClient tempClient = new FacebookClient();
                    //    tempClient.AccessToken = service.AccessToken;
                    //    dynamic ret2 = await tempClient.PostTaskAsync("me", new { });
                    //}
                    //catch
                    //{
                    //    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Access Token Expired");
                    //    throw new ApiDataException(1002, "Access Token Expired.", HttpStatusCode.Unauthorized);
                    //}

                    ////Create Facebook Live Stream
                    //Facebook.FacebookClient client = new FacebookClient();
                    //client.AccessToken = service.AccessToken;
                    //dynamic ret = await client.PostTaskAsync("/me/live_videos", new { });

                    list.Add(new JLiveStream
                    {
                        serviceName = entry.Service.Name,
                        streamId = youtuberesponse.Cdn.IngestionInfo.StreamName,
                        streamUrl = youtuberesponse.Cdn.IngestionInfo.IngestionAddress,
                        streamDate = DateTime.UtcNow
                    });



                    entry.StreamID = youtuberesponse.Cdn.IngestionInfo.StreamName;
                    entry.StreamURL = youtuberesponse.Cdn.IngestionInfo.IngestionAddress;
                    entry.StreamDate = DateTime.UtcNow;

                    db.Entry(entry).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }

            }

            return list;
        }

        [Route("api/v1/livestream/start/{serviceName}")]
        [HttpGet]
        public async Task<HttpResponseMessage> CreateLiveStreamUrl(string serviceName)
        {
            var ruser = this.ApiUser().RUser;
            var service = await db.UserServices.FirstOrDefaultAsync(x => x.UserGUID == ruser.SubjectID && x.Service.Name == serviceName && x.IsDeleted == false);

            if (service == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            JLiveStream result = new JLiveStream();

            if (service.Service.Name == serviceFacebook)
            {
                //Check if Token is Valid
                if (service.AccessToken == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access Token Missing");
                    throw new ApiDataException(1002, "Access Token Missing.", HttpStatusCode.Forbidden);
                }

                //Validate that the Token is Valid
                try
                {
                    //Create Facebook Live Stream
                    Facebook.FacebookClient tempClient = new FacebookClient();
                    tempClient.AccessToken = service.LongToken;
                    dynamic ret2 = await tempClient.PostTaskAsync("me", new { });
                }
                catch
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Access Token Expired");
                    throw new ApiDataException(1002, "Access Token Expired.", HttpStatusCode.Unauthorized);
                }


                //Create Facebook Live Stream
                Facebook.FacebookClient client = new FacebookClient();
                client.AccessToken = service.LongToken;
                dynamic ret = await client.PostTaskAsync("/me/live_videos", new { });

                result.serviceName = service.Service.Name;
                result.streamId = ret.id;
                result.streamUrl = ret.stream_url;
                result.streamDate = DateTime.UtcNow;

                service.StreamID = ret.id;
                service.StreamURL = ret.stream_url;
                service.StreamDate = DateTime.UtcNow;

                db.Entry(service).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            else if (service.Service.Name == serviceYoutube)
            {
                //Check if Token is Valid
                if (service.AccessToken == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access Token Missing");
                    throw new ApiDataException(1002, "Access Token Missing.", HttpStatusCode.Forbidden);
                }


                ClientSecrets secrets = new ClientSecrets()
                {
                    ClientId = ConfigurationManager.AppSettings.Get("YouTubeAppId"),
                    ClientSecret = ConfigurationManager.AppSettings.Get("YouTubeAppSecret")

                };

                var token = new TokenResponse { RefreshToken = service.LongToken };
                var credentials1 = new UserCredential(new GoogleAuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = secrets
                    }), ConfigurationManager.AppSettings.Get("YouTubeAppId"), token);


                var youTubeService = new YouTubeService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credentials1,

                    ApplicationName = "Everywhere"
                });

                var liveStream = new LiveStream
                {

                    Cdn = new CdnSettings
                    {
                        Format = "360p",
                        IngestionType = "rtmp"
                    },
                    Snippet = new LiveStreamSnippet
                    {
                        Title = "test"

                    }

                };
                var request = youTubeService.LiveStreams.Insert(liveStream, "cdn,snippet");
                var youtuberesponse = request.Execute();



                ////Validate that the Token is Valid
                //try
                //{
                //    //Create Facebook Live Stream
                //    Facebook.FacebookClient tempClient = new FacebookClient();
                //    tempClient.AccessToken = service.AccessToken;
                //    dynamic ret2 = await tempClient.PostTaskAsync("me", new { });
                //}
                //catch
                //{
                //    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Access Token Expired");
                //    throw new ApiDataException(1002, "Access Token Expired.", HttpStatusCode.Unauthorized);
                //}

                ////Create Facebook Live Stream
                //Facebook.FacebookClient client = new FacebookClient();
                //client.AccessToken = service.AccessToken;
                //dynamic ret = await client.PostTaskAsync("/me/live_videos", new { });

                result.serviceName = service.Service.Name;
                result.streamId = youtuberesponse.Cdn.IngestionInfo.StreamName;
                result.streamUrl = youtuberesponse.Cdn.IngestionInfo.IngestionAddress;
                result.streamDate = DateTime.UtcNow;

                service.StreamID = youtuberesponse.Cdn.IngestionInfo.StreamName;
                service.StreamURL = youtuberesponse.Cdn.IngestionInfo.IngestionAddress;
                service.StreamDate = DateTime.UtcNow;

                db.Entry(service).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [Route("api/v1/livestream/start/{serviceName}")]
        [HttpPost]
        public async Task<HttpResponseMessage> CreateLiveStreamUrl([FromBody] JLiveStream model)
        {
            var ruser = this.ApiUser().RUser;
            var service = await db.UserServices.FirstOrDefaultAsync(x => x.UserGUID == ruser.SubjectID && x.Service.Name == model.serviceName && x.IsDeleted == false);

            if (service == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            JLiveStream result = new JLiveStream();

            if (service.Service.Name == serviceYounow)
            {
                
              
                service.StreamID = model.streamId;
                service.StreamURL = model.streamUrl;
                service.StreamDate = DateTime.UtcNow;
                service.PictureURL = "../images/stream_thumb_younow.png";
                db.Entry(service).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            

            return Request.CreateResponse(HttpStatusCode.Created);
        }
    }
}
