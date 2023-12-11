﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISD.IdentityServer.Data.Models;

public class ApplicationUser : IdentityUser<int>
{
    public bool AccountEnabled { get; set; }

    [NotMapped]
    public bool IsActiveAndConfirmed => EmailConfirmed && AccountEnabled && PasswordHash != null;
}
