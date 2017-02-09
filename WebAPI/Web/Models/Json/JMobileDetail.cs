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
        [StringLength(13)]
        [RegularExpression("^(?!0+$)(\\+\\d{1,3}[- ]?)?(?!0+$)\\d{10,15}$", ErrorMessage = "Phone number provided is not valid.")]
        public string phone { get; set; }

    }
  
}