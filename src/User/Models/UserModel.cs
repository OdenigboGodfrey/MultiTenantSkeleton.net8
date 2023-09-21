using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace webapi_80.src.User.Models
{
    public class UserModel
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created_At { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string Status { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}

