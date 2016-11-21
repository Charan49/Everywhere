using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Web.Models
{
    public class JService
    {
        public Guid ID { get; set; }

        [Display(Name = "Service Name")]
        [Required(ErrorMessage = "Service Name is required")]
        public string name { get; set; }

        [Display(Name = "Authentication Method")]
        [Required(ErrorMessage = "Authentication Method is required")]
        public string authenticationMethod { get; set; }

        [Required(ErrorMessage = "Service Provider Information is required")]
        [Display(Name = "ServiceProviderInfo")]
        public string serviceProviderInfo  { get; set; }

        public string IsDeleted { get; set; }

        public void Trim()
        {
            //Trim Strings
            authenticationMethod = authenticationMethod.Trim();
            serviceProviderInfo = serviceProviderInfo.Trim();
        }
    }

    public class JServiceLogin
    {
        [Display(Name = "Authentication Method")]
        [Required(ErrorMessage = "Authentication Method is required")]
        public string authenticationMethod { get; set; }
        
    }

    public class JServiceEntry
    {
        public string id { get; set; }
        public string name { get; set; }
        public string authenticationMethod { get; set; }
        public string serviceProviderInfo { get; set; }

    }

    public class JServiceUpdate
    {
        public Guid id { get; set; }
        public string accessToken { get; set; }
        public string tokenExpiresAt { get; set; }

    }
}