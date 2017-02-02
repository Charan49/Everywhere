using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EverywhereWeb.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email provided is not valid")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        [Display(Name = "Mobile Code")]
        public string MobileCode { get; set; }

        [Required]
        [RegularExpression(@"^[\+]?[1 - 9]{1,3}\s?[0 - 9]{6,11}$", ErrorMessage = "Phone number provided is not valid.")]
        //[RegularExpression(@"^((\+\d{1,3}(-| )?\(?\d\)?(-| )?\d{1,5})|(\(?\d{2,6}\)?))(-| )?(\d{3,4})(-| )?(\d{4})(( x| ext)\d{1,5}){0,1}$", ErrorMessage = "Phone number provided is not valid.")]
        //[RegularExpression(@"^(\+[1-9][0-9]*(\([0-9]*\)|-[0-9]*-))?[0]?[1-9][0-9\- ]*$", ErrorMessage = "Phone number provided is not valid.")]
        [Display(Name = "Mobile Phone Number")]
        public string phone { get; set; }

 
    }

    public class VerifyEmailCodeViewModel
    {
        [Required]
        [Display(Name = "Email Code")]
        public string EmailCode { get; set; }

        [Required]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email provided is not valid")]
        public string Email { get; set; }

    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email provided is not valid")]
        public string Email { get; set; }
    }

    public class ConfirmPhoneModel
    {
        [Required]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Phone number provided is not valid.")]
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Phone number provided is not valid.")]
        [RegularExpression(@"^[\+]?[1 - 9]{1,3}\s?[0 - 9]{6,11}$", ErrorMessage = "Phone number provided is not valid.")]

        public string Mobile { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email provided is not valid")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email provided is not valid")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Day")]
        [Range(1,31, ErrorMessage = "Invalid day.")]
        public int Day { get; set; }

        [Required]
        [Display(Name = "Month")]
        [Range(1, 12, ErrorMessage = "Invalid month.")]
        public int Month { get; set; }

        [Required]
        [Display(Name = "Year")]
        [Range(1, 3000, ErrorMessage = "Invalid year.")]
        public int Year { get; set; }

        [Required]
        [RegularExpression(@"^(\+[1-9][0-9]*(\([0-9]*\)|-[0-9]*-))?[0]?[1-9][0-9\- ]*$", ErrorMessage = "Phone number provided is not valid.")]
        //[RegularExpression(@"^((\+\d{1,3}(-| )?\(?\d\)?(-| )?\d{1,5})|(\(?\d{2,6}\)?))(-| )?(\d{3,4})(-| )?(\d{4})(( x| ext)\d{1,5}){0,1}$", ErrorMessage = "Phone number provided is not valid.")]
        [Display(Name = "Mobile Phone Number")]
        public string mobilenumber { get; set; }

    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email provided is not valid")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.CompareAttribute("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Display(Name = "Mobile Code")]
        public string MobileCode { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
		[DataType(DataType.EmailAddress, ErrorMessage = "Email provided is not valid")]
		public string Email { get; set; }
	public class ContactUsViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please select a subject")]
        [Display(Name = "Subject")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Please write a message")]
        [Display(Name = "Message")]
        public string Message { get; set; }

        public List<SelectListItem> SubjectList { get; set; }
        public ContactUsViewModel()
        {
            SubjectList = new List<SelectListItem>();
            SubjectList.Add(new SelectListItem() { Value = "1", Text = "Tell us how we can improve your experience" });
            SubjectList.Add(new SelectListItem() { Value = "2", Text = "Issue in Sign up or Login or Reset password" });
            SubjectList.Add(new SelectListItem() { Value = "3", Text = "Issue in Linking services" });
            SubjectList.Add(new SelectListItem() { Value = "3", Text = "Issue in Scheduling live stream" });
            SubjectList.Add(new SelectListItem() { Value = "3", Text = "Any other issue" });
            SubjectList.Add(new SelectListItem() { Value = "3", Text = "I would like to delete my account" });
            SubjectList.Add(new SelectListItem() { Value = "3", Text = "Business inquiry" });
            
        }
    }
}
