using Duende.IdentityServer.Services;
using ISD.IdentityServer.Models.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISD.IdentityServer.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    readonly IIdentityServerInteractionService interaction;

    public HomeController(IIdentityServerInteractionService interaction)
    {
        this.interaction = interaction;
    }

    [HttpGet]
    public IActionResult ServerError()
    {
        var viewModel = new ServerErrorViewModel { ExceptionMessage = "Unknown server error" };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Error(string errorId)
    {
        var viewModel = new ErrorViewModel();
        var message = await this.interaction.GetErrorContextAsync(errorId);

        if (message != null)
        {
            viewModel.Error = message;
            return View(viewModel);
        }

        return View(viewModel);
    }
}
