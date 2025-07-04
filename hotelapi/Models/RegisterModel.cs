using System;
namespace hotelapi.Models
{
    using System.ComponentModel.DataAnnotations;

    namespace hotelapi.Models
    {
        public class RegisterModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [MinLength(6)]
            public string Password { get; set; }

            [Required]
            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }
        }
    }

}

