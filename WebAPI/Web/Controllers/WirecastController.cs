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
    public class WirecastController : ApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("api/v1/Wirecast/DestinationDescription")]
  
        public async Task<HttpResponseMessage> DestinationDescription()
        {

            //string sSyncData = "<?xml version=\"1.0\"?> " + "Test";

            string sSyncData = @"<?xml   version=""1.0""  encoding=""utf - 8""?>
<destination>
  <branding>
    <logo location = ""destination"" image = ""http://www.everywhere.live/wp-content/uploads/2016/12/asset2.png""  click = ""http://www.everywhere.live/wp-content/uploads/2016/12/asset2.png"">
      <title text = ""Everywhere""  language = ""EN"" />
</logo>
  </branding >
  <channel_discovery_service  url = ""https://web.everywhere.live:8080/api/v1/Wirecast/Discovery"" />
  <format allow_user_defined = ""true"" >
    <title text = ""720p 16:9""  language = ""EN"" />
    <video type = ""flash"" encoder = ""x264"" profile = ""main""    width = ""1280""  height = ""720""  fps = ""25""  bitrate = ""400""  keyframe = ""60"" X264_preset = ""7"" x264_commandline = ""--nal-hrd cbr"" key_frame_aligned = ""true"" absolute_time_stamps = ""true"" strict_cbr = ""true"" />
<audio  encoder = ""aac""  bitrate = ""96""    samplerate = ""44100""    channels = ""2"" />
  </format >
</destination>";
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StringContent(sSyncData);
            return response;
            //return Request.CreateResponse(HttpStatusCode.Created);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("api/v1/Wirecast/Discovery")]

        public async Task<HttpResponseMessage> Discovery(string username, string password)
        {

            //string sSyncData = "<?xml version=\"1.0\"?> " + "Test";

            string sSyncData = @"<?xml   version=""1.0""  encoding=""utf-‐8""?>
<response>
<error  code=""0""    />
<channel  rtmp=""rtmp://vs.everywhere.live""   stream=""Test Stream"" >
<title text=""Test Channel"" language=""EN""/>
</channel>
</response>
";
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StringContent(sSyncData);
            return response;
            //return Request.CreateResponse(HttpStatusCode.Created);
        }


    }
}