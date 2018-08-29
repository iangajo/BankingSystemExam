using System.ComponentModel.DataAnnotations;

namespace Website.ViewModels
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "Login Name is required.")]
        [Display(Name = "Login Name")]
        public string LoginName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Firstname is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Lastname is required.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string Address { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }
    }
}
