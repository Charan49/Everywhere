using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Web.Models.Identity;

namespace System.Web.Http
{
    public static class ApiControllerExtensions
    {
        public static ApiUser ApiUser(this System.Web.Http.ApiController controller)
        {
            return controller.User as ApiUser;
        }

    }
}