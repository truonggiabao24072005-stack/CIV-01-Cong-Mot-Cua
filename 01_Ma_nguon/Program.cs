using CongMotCua.Api.Filters;
using CongMotCua.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddApplicationServices();

var app = builder.Build();
if (await app.InitializeDatabase(args))
    return;

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseRouting();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "Cổng một cửa — Tài liệu API";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Cổng một cửa API v2");
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.Map("/api/{**path}", () => Results.NotFound(new { message = "Đường dẫn API không tồn tại." }));
app.MapFallbackToFile("index.html");
app.Run();
