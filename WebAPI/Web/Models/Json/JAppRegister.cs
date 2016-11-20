using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Web.Models.Json
{
    public class JAppRegister
    {
        [Display(Name = "Application Name")]
        [Required(ErrorMessage = "The Application Name is required")]
        public string appName { get; set; }

        [Required(ErrorMessage = "The UUID is required")]
        [Display(Name = "UUID")]
        public string uuid { get; set; }

        [Required(ErrorMessage = "The Operating System is required")]
        [Display(Name = "Operating System")]
        public string os { get; set; }

        [Required(ErrorMessage = "The Version is required")]
        [Display(Name = "Version")]
        public string version { get; set; }

        [Required(ErrorMessage = "DefaultKey is required")]
        [Display(Name = "DefaultKey")]
        public string defaultKey { get; set; }

        public void Trim()
        {
            //Trim Strings
            appName = appName.Trim();
            uuid = uuid.Trim();
            os = os.Trim();
            version = version.Trim();
            defaultKey = defaultKey.Trim();
        }
    }
}