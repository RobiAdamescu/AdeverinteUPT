﻿using System.ComponentModel.DataAnnotations;

namespace AngularAuthAPI.Models.Dto
{
    public class UserLogin
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
