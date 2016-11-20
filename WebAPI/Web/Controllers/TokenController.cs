using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using JWT;
using Web.Models;

namespace Web.Controllers
{
    public class TokenController : ApiController
    {
        /// <summary>
        /// Create a Jwt with user information
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dbUser"></param>
        /// <returns></returns>
        public static string CreateToken(JLogin loginInfo, User user)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var expiry = Math.Round((DateTime.UtcNow.AddMinutes(GetTokenExpireMinutes()) - unixEpoch).TotalSeconds);
            var issuedAt = Math.Round((DateTime.UtcNow - unixEpoch).TotalSeconds);
            var notBefore = Math.Round((DateTime.UtcNow.AddMonths(6) - unixEpoch).TotalSeconds);
            var sessionID = Guid.NewGuid();

            var payload = new Dictionary<string, object>
            {
                {"email", user.Email},                
                {"role", user.UserType},
                {"sub", user.UserGUID},
                {"sid", sessionID},
                {"nbf", notBefore},
                {"iat", issuedAt},
                {"exp", expiry}
            };

            var apikey = ConfigurationManager.AppSettings.Get("JwtKey");
            var token = JsonWebToken.Encode(payload, apikey, JwtHashAlgorithm.HS256);
            var tokenhash = token.Split(new char[] { '.' });

            //Add Entry in Redis
            ////////////////////
            var rApiUser = new Models.Redis.RApiUser();                        
            rApiUser.SubjectID = user.UserGUID;
            rApiUser.ClientType = user.UserType;
            rApiUser.Email = user.Email;
            rApiUser.Name = user.FirstName;
            rApiUser.LastName = user.LastName;
            rApiUser.SessionID = sessionID;
            rApiUser.TokenHash = tokenhash[2];

            if (user.UserType == "User")
                rApiUser.DeviceID = Guid.Parse(loginInfo.uuid);

            RedisDb.RedisCache.Add<Models.Redis.RApiUser>("ApiUser:" + user.UserGUID + ":Session:" + rApiUser.SessionID, rApiUser, new TimeSpan(0, GetTokenExpireMinutes(), 0));

            return token;
        }

        private static int GetTokenExpireMinutes()
        {
            return Convert.ToInt32(ConfigurationManager.AppSettings.Get("JwtExpireMinutes"));
        }
    }
}
