﻿using System.ComponentModel.DataAnnotations;

namespace BFF.IdentityServer.Models;

public class LoginViewModel
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = string.Empty;
}
