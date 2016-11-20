using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Web.Models
{
    public class JUser
    {
        [Required]
        public int id;
        [Required]
        [EmailAddress]
        public string email;
        [Required]
        public string firstName;
        public string lastName;
    }

    public class JUserAdd
    {
        [Required]
        [StringTrim]
        [EmailAddress]
        public string email;
        
        [Required]
        [StringTrim]
        public string firstName;
        
        [StringTrim]
        public string lastName;

        [Required]
        [StringLength(18, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [StringTrim]
        public string role;
    }
    public class ForgetPassword
    {
        [Required]
        [StringTrim]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringTrim]
        public string VerificationCode { get; set; }

        [Required]
        [StringTrim]
        [StringLength(18, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }

    public class ResetPasswordModel
    {
        [Required]
        [StringTrim]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ParamDisable
    {
        [Required]
        [StringTrim]
        public int id { get; set; }

        [Required]
        [StringTrim]
        public string disable { get; set; }

        [Required]
        [StringTrim]
        public string notify { get; set; }
    }
}