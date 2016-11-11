using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class JLogin
    {
        [Display(Name = "Email address")]
        [Required(ErrorMessage = "The email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required]
        [StringLength(18, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Key")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Key is required")]
        public string Key { get; set; }

        [Display(Name = "UUID")]
        public string UUID { get; set; }    //Needed in case of User Login from App
    }
}