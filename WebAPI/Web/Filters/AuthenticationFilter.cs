using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using JWT;
using System.Collections;
using System.Configuration;
using Web.Models.Identity;

namespace Web
{
    public class AuthenticationHandler:DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                    CancellationToken cancellationToken)
        {
            HttpResponseMessage errorResponse = null;

            try
            {                
                IEnumerable<string> authHeaderValues;
                request.Headers.TryGetValues("Authorization", out authHeaderValues);

                //No Authorization Header specified - Let the Request Flow Through
                if (authHeaderValues == null)
                    return base.SendAsync(request, cancellationToken); // cross fingers

                //Get JSON Token
                var bearerToken = authHeaderValues.ElementAt(0);
                var token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;

                //Load Secret Key 
                var secret = ConfigurationManager.AppSettings.Get("JwtKey");                

                //Validate Token
                Thread.CurrentPrincipal = ValidateToken(token, secret, true);

                if (HttpContext.Current != null)
                {
                    HttpContext.Current.User = Thread.CurrentPrincipal;
                }
            }
            catch (SignatureVerificationException ex)
            {
                errorResponse = request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (Exception ex)
            {
                errorResponse = request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }


            return errorResponse != null
                ? Task.FromResult(errorResponse)
                : base.SendAsync(request, cancellationToken);
        }

        private static ClaimsPrincipal ValidateToken(string token, string secret, bool checkExpiration)
        {
            var jsonSerializer = new JavaScriptSerializer();
            var payloadJson = JsonWebToken.Decode(token, secret);
            var payloadData = jsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson);


            object exp;
            if (payloadData != null && (checkExpiration && payloadData.TryGetValue("exp", out exp)))
            {
                var validTo = FromUnixTime(long.Parse(exp.ToString()));
                if (DateTime.Compare(validTo, DateTime.UtcNow) <= 0)
                {
                    throw new Exception(
                        string.Format("Token is expired. Expiration: '{0}'. Current: '{1}'", validTo, DateTime.UtcNow));
                }
            }


            //Validate Token in RedisCache            
            var rApiUser = RedisDb.RedisCache.Get<Models.Redis.RApiUser>("ApiUser:" + payloadData["sub"] + ":Session:" + payloadData["sid"]);
            if (rApiUser == null)
                throw new SignatureVerificationException("Entry Not Found in Redis");

            //Make sure Hash Matches as well. There is a chance that the user generated a new token and someone is trying
            //to use the old one
            var tokenhash = token.Split(new char[]{'.'});
            if (rApiUser.TokenHash != tokenhash[2])
                throw new SignatureVerificationException("Token Hash Mismatch. Possibly Outdated Token");

            var subject = new ClaimsIdentity("Federation", ClaimTypes.Name, ClaimTypes.Role);

            var claims = new List<Claim>();

            if (payloadData != null)
                foreach (var pair in payloadData)
                {
                    var claimType = pair.Key;

                    var source = pair.Value as ArrayList;

                    if (source != null)
                    {
                        claims.AddRange(from object item in source
                                        select new Claim(claimType, item.ToString(), ClaimValueTypes.String));

                        continue;
                    }

                    switch (pair.Key)
                    {
                        case "name":
                            claims.Add(new Claim(ClaimTypes.Name, pair.Value.ToString(), ClaimValueTypes.String));
                            break;
                        case "surname":
                            claims.Add(new Claim(ClaimTypes.Surname, pair.Value.ToString(), ClaimValueTypes.String));
                            break;
                        case "email":
                            claims.Add(new Claim(ClaimTypes.Email, pair.Value.ToString(), ClaimValueTypes.Email));
                            break;
                        case "role":
                            claims.Add(new Claim(ClaimTypes.Role, pair.Value.ToString(), ClaimValueTypes.String));
                            break;
                        case "sub":
                            claims.Add(new Claim(ClaimTypes.NameIdentifier, pair.Value.ToString(), ClaimValueTypes.String));
                            break;
                        default:
                            claims.Add(new Claim(claimType, pair.Value.ToString(), ClaimValueTypes.String));
                            break;
                    }
                }

            subject.AddClaims(claims);
            var user = new ApiUser(subject);
            user.RUser = rApiUser;
            return user;
        }

        private static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
    }
}
