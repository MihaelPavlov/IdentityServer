using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISD.Application.Controllers;

[Authorize]
public class BaseController : Controller
{
    [Authorize]
    [Route("/")]
    [HttpGet]
    public string Index() => "We are hidden. Boom Hackers";
}
