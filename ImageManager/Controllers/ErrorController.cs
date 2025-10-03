using ImageManager.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PixivAuthException = PixivCS.Exceptions.PixivAuthException;
using PixivRateLimitException = PixivCS.Exceptions.PixivRateLimitException;

namespace ImageManager.Controllers;

[ApiController]
public class ErrorController(ILogger<ErrorController> loggerService) : Controller
{
    [Route("/error")]
    [HttpGet, HttpPost]
    public IActionResult HandleError([FromServices] IHostEnvironment env)
    {
        var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

        loggerService.LogError($"Exception: {exception?.GetType().Name}, Message: {exception?.Message}, StackTrace: {exception?.StackTrace}");

        return exception switch
        {
            PixivAuthException => Problem(title: "Pixiv authentication failed", statusCode: 401),
            PixivRateLimitException => Problem(title: "Pixiv rate limit exceeded", statusCode: 429),
            PixivApiException => Problem(title: "Pixiv API error", statusCode: 502),
            _ => Problem(title: "An unexpected error occurred", statusCode: 500)
        };
    }
}