using System.Net; 

namespace EbayCloneBuyerService_CoreAPI.Exceptions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next,
                                   ILogger<ExceptionMiddleware> logger,
                                   IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ServiceException ex)
            {

                _logger.LogWarning(ex, "Lỗi dịch vụ (ServiceException): {Message}", ex.Message);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = ex.StatusCode;

                string responseMessage; 

                switch (ex.StatusCode)
                {
                    case StatusCodes.Status500InternalServerError:
                        responseMessage = $"API error: {ex.Message}";
                        break;

                    case StatusCodes.Status404NotFound:
                        responseMessage = $"Not found: {ex.Message}";
                        break;
                    default:
                        responseMessage = ex.Message;
                        break;
                }

                // Trả về đối tượng với message đã được tùy chỉnh
                await context.Response.WriteAsJsonAsync(new { message = responseMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống không lường trước: {Message}", ex.Message);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500

                var response = _env.IsDevelopment()
     ? new { message = ex.Message, details = ex.StackTrace?.ToString() }
     : new { message = "Đã xảy ra lỗi hệ thống, vui lòng thử lại sau.", details = (string?)null }; 

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
