using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace StreamingPOC
{
    public partial class Callback : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var sr = new StreamReader(Request.InputStream);
            string content = sr.ReadToEnd();

            content = Request.QueryString["Code"].ToString();
            //Code

            ClientSecrets secrets = new ClientSecrets()
            {
                ClientId = "523666141221-8jckerkfihfs2iskmoagc5hpgmrpk9f9.apps.googleusercontent.com",
                ClientSecret = "LTW0m9rgkjSW17bhiXSVCiEr"
            };

            // Use the code exchange flow to get an access and refresh token.
            IAuthorizationCodeFlow flow =
                new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = secrets,
                    Scopes = new string[] { YouTubeService.Scope.Youtube }
                });
            TokenResponse response = flow.ExchangeCodeForTokenAsync("", content, "postmessage", CancellationToken.None).Result;

            var credentials = new UserCredential(new GoogleAuthorizationCodeFlow(
            new GoogleAuthorizationCodeFlow.Initializer { ClientSecrets = secrets }),
           "TEST", response);


            var service = new YouTubeService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
                ApplicationName = "Testing"
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
            //WriteOutput("5");
            var request = service.LiveStreams.Insert(liveStream, "cdn,snippet");
            var response1 = request.Execute();

            Label1.Text = "Streamimg address=" + response1.Cdn.IngestionInfo.IngestionAddress;
            Label2.Text = "Streamimg Name=" + response1.Cdn.IngestionInfo.StreamName;

        }
    }
}