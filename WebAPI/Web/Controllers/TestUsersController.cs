
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
using SendGrid;
using System.Net.Mail;

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
            try
            {
                dynamic result1 = await client.PostTaskAsync(client.AppId + "/accounts/test-users?installed=true&permissions=read_stream&name=" + rUser.Name + " " + rUser.LastName, new
                {
                    access_token = result[0],

                });
                list.Add(new JTestUser { id = result1[0], access_token = result1[1], login_url = result1[2], email = result1[3], password = result1[4] });
                TestUser testUser = null;


                testUser = new TestUser
                {
                    EmailID = result1[3],
                    Password = result1[4],
                    FaceBookID = result1[0],
                    UserGUID = rUser.SubjectID,
                    Name = rUser.Name,
                    IsDeleted = false,
                    IsLinked = false,
                    ModifiedDate = DateTime.UtcNow

                };

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {

                        db.TestUsers.Add(testUser);
                        db.SaveChanges();

                        //Complete Transaction
                        transaction.Commit();


                    }
                    catch
                    {
                        //Rollback
                        transaction.Rollback();
    
                    }
                }

                MailMessage message = new MailMessage("test@test.com", rUser.Email);
                message.Body = "Hi " + rUser.Name + " ,<br /><br />Here is your Everywhere Facebook account"
                               + "<br /><br />Email: " + result1[3] + " <br />Password: " + result1[4] +
               "<br /><br />Please follow the procedure below to allow Everywhere to publish your live streams to Facebook." +
               "<br />1 - Go to https://web.everywhere.live and login." +
               "<br />2 - Logout of Facebook if you already logged in." +
               "<br />3 - Click on Add Services. Against Facebook click \"Link\" button and sign into Facebook with the above credentials. In the Login With Facebook page click \"OK\" to allow Everywhere Web to post to Facebook for you." +

               "<br /><br />Best regards, <br /> Team Everywhere <br /> www.Everywhere.live";
                message.Subject= "Everywhere Facebook account ";
                await SendEmail.sendMail(message);

             


            }
            catch (Facebook.FacebookOAuthException ex)
            {
                if (ex.Message == "(OAuthException - #195) (#195) The name you are trying to use is invalid")
                {
                    dynamic result1 = await client.PostTaskAsync(client.AppId + "/accounts/test-users?installed=true&permissions=read_stream&name=" + rUser.Name + " Everywhere ", new
                    {
                        access_token = result[0],

                    });
                    list.Add(new JTestUser { id = result1[0], access_token = result1[1], login_url = result1[2], email = result1[3], password = result1[4] });
                    TestUser testUser = null;


                    testUser = new TestUser
                    {
                        EmailID = result1[3],
                        Password = result1[4],
                        FaceBookID = result1[0],
                        UserGUID = rUser.SubjectID,
                        Name = rUser.Name,
                        IsDeleted = false,
                        IsLinked = false,
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

                    MailMessage message = new MailMessage("test@test.com", rUser.Email);
                    message.Body = "Hi " + rUser.Name + " ,<br /><br />Here is your Everywhere Facebook account"
                                   + "<br /><br />Email: " + result1[3] + " <br />Password: " + result1[4] +
                   "<br /><br />Please follow the procedure below to allow Everywhere to publish your live streams to Facebook." +
                   "<br />1 - Go to https://web.everywhere.live and login." +
                   "<br />2 - Logout of Facebook if you already logged in." +
                   "<br />3 - Click on Add Services. Against Facebook click \"Link\" button and sign into Facebook with the above credentials. In the Login With Facebook page click \"OK\" to allow Everywhere Web to post to Facebook for you." +

                   "<br /><br />Best regards, <br /> Team Everywhere <br /> www.Everywhere.live";
                    message.Subject = "Everywhere Facebook account ";
                    await SendEmail.sendMail(message);


                   
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
