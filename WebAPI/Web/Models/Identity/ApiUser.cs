using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Web.Models.Identity
{
    public class ApiUser: ClaimsPrincipal
    {
        public ApiUser(ClaimsIdentity identity) : base(identity)
        {
        }

        public Models.Redis.RApiUser RUser;

        //*** Add Custom Claims Here
        public string Email
        {
            get
            {                   
                return this.FindFirst(ClaimTypes.Email).Value;
            } 
        }

        public string Role
        {
            get
            {
                return this.FindFirst(ClaimTypes.Role).Value;
            }
        }

        public string ID
        {
            get
            {
                return this.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
        }
    }
}