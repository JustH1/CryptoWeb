
namespace CryptoWeb
{
    public class RouteMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;
        public RouteMiddleware(RequestDelegate next, ILogger logger)
        {
            this.next = next;
            this.logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            string path = context.Request.Path;
            logger.LogInformation($"Request: {path}");
            if (path == "/")
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.SendFileAsync("wwwroot\\index.html");
            }
            else if (path == "/Encrypt" | path == "/Decrypt" | path == "/Download")
            {
                await next.Invoke(context);
            }
        }
    }
}

