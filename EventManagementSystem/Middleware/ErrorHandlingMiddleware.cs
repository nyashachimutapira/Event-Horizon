using EventManagementSystem.Services;
using System.Net;

namespace EventManagementSystem
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggingService _loggingService;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILoggingService loggingService, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _loggingService = loggingService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                // Handle 404 errors
                if (context.Response.StatusCode == (int)HttpStatusCode.NotFound)
                {
                    var path = context.Request.Path.Value;
                    await _loggingService.LogInfoAsync($"404 Not Found: {path}");
                    
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync($@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <title>404 - Page Not Found</title>
                            <style>
                                body {{ font-family: Arial; text-align: center; margin-top: 50px; background: #f5f5f5; }}
                                .container {{ background: white; padding: 40px; border-radius: 5px; max-width: 500px; margin: 0 auto; }}
                                h1 {{ color: #d32f2f; }}
                                p {{ color: #666; }}
                                a {{ color: #1976d2; text-decoration: none; }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <h1>404 - Page Not Found</h1>
                                <p>The page you requested could not be found: {path}</p>
                                <a href='/'>Go back to home</a>
                            </div>
                        </body>
                        </html>
                    ");
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Unhandled exception occurred", ex);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "text/html";

            var errorResponse = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>500 - Internal Server Error</title>
                    <style>
                        body {{ font-family: Arial; text-align: center; margin-top: 50px; background: #f5f5f5; }}
                        .container {{ background: white; padding: 40px; border-radius: 5px; max-width: 600px; margin: 0 auto; }}
                        h1 {{ color: #d32f2f; }}
                        p {{ color: #666; }}
                        .error-details {{ background: #f0f0f0; padding: 15px; border-left: 4px solid #d32f2f; text-align: left; margin: 20px 0; }}
                        a {{ color: #1976d2; text-decoration: none; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>500 - Internal Server Error</h1>
                        <p>An unexpected error occurred while processing your request.</p>
                        <div class='error-details'>
                            <strong>Error:</strong> {exception.Message}
                        </div>
                        <a href='/'>Go back to home</a>
                    </div>
                </body>
                </html>
            ";

            return context.Response.WriteAsync(errorResponse);
        }
    }
}
