using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CongMotCua.Api.Filters;
public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["Referrer-Policy"] = "same-origin";
        context.Response.Headers["Cache-Control"] = context.Request.Path.StartsWithSegments("/api") ? "no-store" : "no-cache";
        try
        {
            await next(context);
        }
        catch (BusinessError error)
        {
            await Error(context, error.Status, error.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            await Error(context, 409, "Dữ liệu vừa thay đổi. Vui lòng tải lại hồ sơ.");
        }
        catch (DbUpdateException error)
        {
            logger.LogWarning(error, "SQL constraint rejected request {TraceId}", context.TraceIdentifier);
            await Error(context, 409, "Dữ liệu bị trùng hoặc xung đột ràng buộc. Vui lòng tải lại và kiểm tra.");
        }
        catch (SqlException error)when (error.Number is 1205 or 1222)
        {
            logger.LogWarning(error, "Concurrent SQL operation {TraceId}", context.TraceIdentifier);
            await Error(context, 409, "Hồ sơ đang được cập nhật. Vui lòng tải lại và thử lại.");
        }
        catch (BadHttpRequestException)
        {
            await Error(context, 400, "Dữ liệu yêu cầu không đúng định dạng.");
        }
        catch (OperationCanceledException)when (context.RequestAborted.IsCancellationRequested)
        {
        }
        catch (Exception error)
        {
            logger.LogError(error, "Unhandled request {TraceId}", context.TraceIdentifier);
            await Error(context, 500, "Chưa thực hiện được thao tác. Vui lòng thử lại hoặc liên hệ cán bộ hỗ trợ.");
        }
    }

    private static Task Error(HttpContext context, int status, string message)
    {
        if (context.Response.HasStarted)
            return Task.CompletedTask;
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(new { message, traceId = context.TraceIdentifier });
    }
}
