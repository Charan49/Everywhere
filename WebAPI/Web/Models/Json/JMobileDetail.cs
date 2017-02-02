using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Web.Models.Json
{
    public class JMobileDetail
    {
        [Required]
        [StringTrim]
        [EmailAddress]
        public string email { get; set; }

        [StringTrim]
      
        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Mobile Phone Number")]
        //[RegularExpression(@"^(\+[1-9][0-9]*(\([0-9]*\)|-[0-9]*-))?[0]?[1-9][0-9\- ]*$", ErrorMessage = "Phone number provided is not valid.")]
        [RegularExpression(@"^[\+]?[1 - 9]{1,3}\s?[0 - 9]{6,11}$", ErrorMessage = "Phone number provided is not valid.")]
        //[RegularExpression(@"^((\+\d{1,3}(-| )?\(?\d\)?(-| )?\d{1,5})|(\(?\d{2,6}\)?))(-| )?(\d{3,4})(-| )?(\d{4})(( x| ext)\d{1,5}){0,1}$", ErrorMessage = "Phone number provided is not valid.")]
        public string phone { get; set; }

    }
  
}