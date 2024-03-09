namespace CryptoWeb
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<Program> logger)
        {
            this.next = next;
            this.logger = logger;
        }
        public async void InvokeAsync(HttpContext context)
        {
            await next.Invoke(context);
            string message = String.Empty;
            switch (context.Response.StatusCode)
            {
                case 460:
                    message = "Status code 460. Files are missing.";
                    await context.Response.WriteAsync("Status code 460. Files are missing.");
                    break;
                case 526:
                    message = "Status code 460. Error in middleware decryption.";
                    await context.Response.WriteAsync("Status code 460. Error in middleware decryption.");
                    break;
                case 527:
                    message = "Status code 527. Error in middleware encryption.";
                    await context.Response.WriteAsync("Status code 527. Error in middleware encryption.");
                    break;
                case 528:
                    message = "Status code 528. Error in the middleware SendFile.";
                    await context.Response.WriteAsync("Status code 528. Error in the middleware SendFile.");
                    break;
                case 529:
                    message = "Status code 529. Error in the middleware SendFile. The archive is missing.";
                    await context.Response.WriteAsync("Status code 529. Error in the middleware SendFile. The archive is missing.");
                    break;
                case 530:
                    message = "Status code 530. Error in the middleware SendFile. the file path was not received.";
                    await context.Response.WriteAsync("Status code 530. Error in the middleware SendFile. the file path was not received.");
                    break;
            }
            logger.LogError(message);
        }
    }
}
