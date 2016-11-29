using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Web.Models.Json
{
    public class JForgetPassword
    {
        [Required]
        [StringTrim]
        [EmailAddress]
        public string email { get; set; }


    }

    public class JResetPasswordModel
    {

        [Required]
        [StringTrim]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        [StringTrim]
        public string ConfirmationCode { get; set; }

        [Required]
        [StringTrim]
        [StringLength(18, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string newPassword { get; set; }
    }
}