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
        public string AppName { get; set; }

        [Required(ErrorMessage = "The UUID is required")]
        [Display(Name = "UUID")]
        public string UUID { get; set; }

        [Required(ErrorMessage = "The Operating System is required")]
        [Display(Name = "Operating System")]
        public string OS { get; set; }

        [Required(ErrorMessage = "The Version is required")]
        [Display(Name = "Version")]
        public string Version { get; set; }

        [Required(ErrorMessage = "DefaultKey is required")]
        [Display(Name = "DefaultKey")]
        public string DefaultKey { get; set; }

        public void Trim()
        {
            //Trim Strings
            AppName = AppName.Trim();
            UUID = UUID.Trim();
            OS = OS.Trim();
            Version = Version.Trim();
            DefaultKey = DefaultKey.Trim();
        }
    }
}