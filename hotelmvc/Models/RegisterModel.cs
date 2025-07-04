using System;
using System.ComponentModel.DataAnnotations;

namespace hotelmvc.Models
{
    public class RegisterModel
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]

        public string ConfirmPassword { get; set; }
    }
}

