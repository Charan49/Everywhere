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
        public string Name { get; set; }

        [Display(Name = "Authentication Method")]
        [Required(ErrorMessage = "Authentication Method is required")]
        public string AuthenticationMethod { get; set; }

        [Required(ErrorMessage = "Service Provider Information is required")]
        [Display(Name = "ServiceProviderInfo")]
        public string ServiceProviderInfo  { get; set; }
    
        public void Trim()
        {
            //Trim Strings
            AuthenticationMethod = AuthenticationMethod.Trim();
            ServiceProviderInfo = ServiceProviderInfo.Trim();
        }
    }

    public class JServiceLogin
    {
        [Display(Name = "Authentication Method")]
        [Required(ErrorMessage = "Authentication Method is required")]
        public string AuthenticationMethod { get; set; }
        
    }

    public class JServiceEntry
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string AuthenticationMethod { get; set; }
        public string ServiceProviderInfo { get; set; }

    }
}