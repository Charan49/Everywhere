using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Web.Models;
using System.Data.Entity;
using Web.Helper;
using System.Threading.Tasks;
using System.Web.Http.Cors;
using System.Net.Mail;
using WebApi.ErrorHelper;
using Web.Models.Json;

namespace Web.Controllers
{
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class VideoSwitchContoller : ApiController
    {
        private InterceptDB db = new InterceptDB();


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
        
            VideoSwitchURL jvs = new VideoSwitchURL()
            {
                StreamURL = model.url,
                VideoGUID = Guid.NewGuid()
            };


            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    //Check if a Video with Same E-mail Already Exists
                    int count = await db.VideoSwitchURLs.CountAsync(u => String.Compare(u.StreamURL, model.url.Trim(), true) == 0);
                    if (count > 0)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.Conflict, "URL already exists.");
                        throw new ApiDataException(1002, "URL is already exist in system.", HttpStatusCode.Conflict);

                    }                   

                    db.VideoSwitchURLs.Add(jvs);
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

        [Route("api/v1/video/Getvideo")]
        [HttpGet]
        public async Task<IEnumerable<VideoSwitchURL>> GetUrlInfo()
        {         
            var videos = await db.VideoSwitchURLs.Select(x => new VideoSwitchURL { Id = x.Id, StreamURL = x.StreamURL }).ToListAsync();
            if (videos.Count > 0)
                return videos;
            else
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorDescription = "Bad Request..." };
        }

    }
}
