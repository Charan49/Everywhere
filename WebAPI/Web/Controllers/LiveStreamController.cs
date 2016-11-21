using System;
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

namespace Web.Controllers
{
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LiveStreamController : ApiController
    {
        private InterceptDB db = new InterceptDB();

        [Route("api/v1/livestream/start")]
        [HttpGet]
        public async Task<List<JLiveStream>> CreateLiveStreamUrls()
        {
            var ruser = this.ApiUser().RUser;
            var services = await db.UserServices.Where(x => x.UserGUID == ruser.SubjectID && x.IsDeleted == false).ToListAsync();
            var list = new List<JLiveStream>();

            foreach (var entry in services)
            {
                if (entry.Service.Name == "Facebook")
                {
                    //Create Facebook Live Stream
                    Facebook.FacebookClient client = new FacebookClient();
                    client.AccessToken = entry.AccessToken;
                    dynamic ret = await client.PostTaskAsync("/me/live_videos", new { });

                    list.Add(new JLiveStream
                    {
                        serviceName = entry.Service.Name,
                        streamId = ret.id,
                        streamUrl = ret.stream_url
                    });
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

            if (service.Service.Name == "Facebook")
            {
                //Create Facebook Live Stream
                Facebook.FacebookClient client = new FacebookClient();
                client.AccessToken = service.AccessToken;
                dynamic ret = await client.PostTaskAsync("/me/live_videos", new { });

                result.serviceName = service.Service.Name;
                result.streamId = ret.id;
                result.streamUrl = ret.stream_url;
                
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
