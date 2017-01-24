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
        public int accountState;
        public Guid guid;

        public string callbackURL;
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

        public string callbackURL;
    }
    public class ForgetPassword
    {
        [Required]
        [StringTrim]
        [EmailAddress]
        public string MobileConfirmationCode { get; set; }

    }

    public class ResetPasswordModel
    {
        [Required]
        [StringLength(18, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string newPassword;

        [Required]
        [StringLength(18, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string confirmPassword { get; set; }

        public string email { get; set; }

        public string emailcode { get; set; }

        public string usertype { get; set; }
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

    public class JConfirmEmail
    {
        [Required]
        [StringTrim]
        public string mobilecode { get; set; }

        [StringTrim]
        public string codeType { get; set; }


    }

    public class JVerifyEmail
    {
        [Required]
        [StringTrim]
        public string emailcode { get; set; }

        [StringTrim]
        public string code { get; set; }

    }
}