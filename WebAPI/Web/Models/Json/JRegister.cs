using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class JRegister
    {
        [Display(Name = "Name")]
        [Required(ErrorMessage = "Name is required")]
        public string name { get; set; }

        [Display(Name = "Email address")]
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string email { get; set; }

        [Required]
        [StringLength(18, MinimumLength = 8, ErrorMessage = "{0} must be at least {2} characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }

        public string dob { get; set; }
        public string location { get; set; }
        public string url { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Mobile Phone Number")]
        [RegularExpression("^(?!0+$)(\\+\\d{1,3}[- ]?)?(?!0+$)\\d{10,15}$", ErrorMessage = "Phone number provided is not valid.")]
        public string mobilenumber { get; set; }
        public void Trim()
        {
            //Trim Strings
            name = name.Trim();
            email = email.Trim();
            password = password.Trim();

            if (!String.IsNullOrEmpty(dob))
                dob = dob.Trim();

            if (!String.IsNullOrEmpty(location))
                location = location.Trim();
        }
    }
}