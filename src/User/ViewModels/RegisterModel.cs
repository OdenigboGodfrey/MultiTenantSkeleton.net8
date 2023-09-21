using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace webapi_80.src.User.ViewModels
{
    public class RegisterModel
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Password { get; set; } = "";

        public bool SendMail { get; set; }
        public string ConfirmPassword { get; set; }  = "";

        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public string Role { get; set; }
        public string PhoneNumber { get; set; }
        public string DateOfBirth { get; set; }
        public string Status { get; set; }
        public string InvitedBy { get; set; }
        public bool PasswordCreatedByAdmin { get; set; }
        public Guid TenantId { get; set; }
        public string GoogleAuthToken { get; set; }
        public string GoogleAuthId { get; set; }
        public string MicrosoftAccessToken { get; set; }
        public string MicrosoftAuthId { get; set; }


    }

    public class UserSignupModel
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Password { get; set; } = "";
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

}
