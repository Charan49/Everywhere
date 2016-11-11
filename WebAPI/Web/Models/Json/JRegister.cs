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
        public string Name { get; set; }

        [Display(Name = "Email address")]
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required]
        [StringLength(18, MinimumLength=8, ErrorMessage = "{0} must be at least {2} characters long.")]        
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public string DOB { get; set; }        
        public string Location { get; set; }

        public void Trim()
        {
            //Trim Strings
            Name = Name.Trim();
            Email = Email.Trim();
            Password = Password.Trim();
            DOB = DOB.Trim();
            Location = Location.Trim();
        }
    }
}