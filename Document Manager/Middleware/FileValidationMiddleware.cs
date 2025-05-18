using Document_Manager.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Document_Manager.Middleware
{
    public class FileValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public FileValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IFileValidationService fileValidationService)
        {
            // Check if this is a file upload request
            if (context.Request.HasFormContentType && 
                context.Request.Form.Files.Count > 0 && 
                context.Request.Path.Value?.Contains("/upload") == true)
            {
                foreach (var file in context.Request.Form.Files)
                {
                    var validationResult = await fileValidationService.ValidateFileAsync(file);
                    if (!validationResult.IsValid)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(validationResult);
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

    public static class FileValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseFileValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FileValidationMiddleware>();
        }
    }
}
