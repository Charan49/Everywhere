
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

namespace Web.Controllers
{
   
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class TestUsersController : ApiController
    {
        private InterceptDB db = new InterceptDB();
           
        [Route("api/v1/TestUsers")]
        [HttpGet]
        public async Task<List<JTestUser>> CreateTestUser()    //Service GUID
        {
            var list = new List<JTestUser>();
            var rUser = this.ApiUser().RUser;
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
            dynamic result1 = await client.PostTaskAsync(client.AppId+ "/accounts/test-users?installed=true&permissions=read_stream&name="+ rUser.Name+rUser.LastName, new
            {
                access_token = result[0],
 
            });

            //dynamic result2 = await client.PostTaskAsync(result1[0], new
            //{
            //    name = rUser.Name

            //});

            list.Add(new JTestUser { id = result1[0], access_token = result1[1], login_url = result1[2], email = result1[3], password = result1[4] });
            TestUser testUser = null;

            Guid fb =new Guid("1687D96E-BFDC-472D-AF9F-2AE31CCFAC35");

            testUser = new TestUser
            {
                EmailID = result1[3],
                Password = result1[4],
                UserGUID = rUser.SubjectID,
                Name = rUser.Name,
                ModifiedDate = DateTime.UtcNow
                
            };

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {

                    System.Diagnostics.EventLog.WriteEntry("Desktop Window Manager", "error");
                    db.TestUsers.Add(testUser);
                    db.SaveChanges();

                    //Complete Transaction
                    transaction.Commit();

                   
                }
                catch
                {
                    //Rollback
                    transaction.Rollback();
                    System.Diagnostics.EventLog.WriteEntry("Desktop Window Manager", "error");
                   
                }
            }

            
            return list;

        }

        private string GetHeaderValue(string header)
        {
            return Request.Headers.GetValues(header).FirstOrDefault();
        }
    }
}
