using CongMotCua.Api.Controllers;
using CongMotCua.Api.Filters;
using CongMotCua.Api.Models.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace CongMotCua.Services;

public static class AppSetup
{
    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true).AddEnvironmentVariables();
        builder.Services.AddDbContext<AppDb>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
        builder.Services.AddScoped<WorkflowService>();
        builder.Services.AddScoped<CatalogService>();
        builder.Services.AddScoped<HoSoQueryService>();
        builder.Services.AddSingleton<FileStore>();
        builder.Services.AddSingleton<SlaService>();
        builder.Services.AddSingleton<ThuThachService>();
        var local = !string.Equals(builder.Configuration["Auth:Mode"], "Keycloak", StringComparison.OrdinalIgnoreCase);
        var settings = new AuthSettings(local, local ? "congmotcua-local-identity" : builder.Configuration["Auth:Authority"] ?? "http://localhost:8180/realms/congmotcua");
        var identity = new LocalIdentity(builder.Configuration, builder.Environment);
        builder.Services.AddSingleton(settings);
        builder.Services.AddSingleton(identity);
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            if (!settings.IsLocal)
            {
                options.Authority = settings.Issuer;
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
            }

            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateAudience = true,
                ValidAudience = "cong-mot-cua-api",
                ValidateLifetime = true,
                IssuerSigningKey = settings.IsLocal ? new SymmetricSecurityKey(identity.Key) : null,
                ClockSkew = TimeSpan.FromSeconds(20)
            };
        });
        builder.Services.AddAuthorization();
        builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173").AllowAnyHeader().AllowAnyMethod()));
        builder.Services.ConfigureHttpJsonOptions(options => ConfigureJson(options.SerializerOptions));
        var controllers = builder.Services.AddControllers(options =>
        {
            options.Filters.Add<RequestValidationFilter>();
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        }).AddJsonOptions(options => ConfigureJson(options.JsonSerializerOptions))
          .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);
        if (!local)
        {
            controllers.ConfigureApplicationPartManager(manager =>
                manager.FeatureProviders.Add(new KeycloakControllerFeatureProvider()));
        }
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "local", _ => new FixedWindowRateLimiterOptions { PermitLimit = 240, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            options.AddFixedWindowLimiter("public", limiter =>
            {
                limiter.PermitLimit = 20;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.QueueLimit = 0;
            });
        });
        builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 11 * 1024 * 1024);
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Cổng một cửa — Backend API", Version = "2.0.0", Description = "API cho 3 vai trò, gồm 9 use case nghiệp vụ chính và 3 use case hỗ trợ. Đăng nhập ở /api/auth/login, sao chép accessToken rồi bấm Authorize. Các thao tác ghi chạy trên SQL Server thật. Dùng database kiểm thử khi thử lệnh." });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", Description = "Nhập accessToken nhận từ API đăng nhập, không nhập mật khẩu tại đây." });
            options.OperationFilter<BackendOperationFilter>();
        });
    }

    private static void ConfigureJson(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.Converters.Add(new UtcDateConverter());
    }

    public static async Task<bool> InitializeDatabase(this WebApplication app, string[] args)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDb>();
        if (args.Contains("--schema"))
        {
            Console.WriteLine(db.Database.GenerateCreateScript());
            return true;
        }

        await db.Database.EnsureCreatedAsync();
        var auth = app.Services.GetRequiredService<AuthSettings>();
        if (args.Contains("--cap-tai-khoan"))
        {
            if (!auth.IsLocal)
                throw new InvalidOperationException("Cấp tài khoản qua nhà cung cấp danh tính khi dùng Keycloak.");
            await StaffAccountSetup.Run(db, app.Services.GetRequiredService<LocalIdentity>(), auth, args);
            return true;
        }

        if (args.Contains("--seed-sample"))
        {
            if (!args.Contains("--initialize") || !auth.IsLocal)
                throw new InvalidOperationException("Chỉ nạp dữ liệu mẫu với chế độ xác thực cục bộ.");
            if (await db.ThuTuc.AnyAsync() || await db.NguoiDung.AnyAsync() || await db.BoPhan.AnyAsync())
                throw new InvalidOperationException("Chỉ nạp dữ liệu mẫu vào cơ sở dữ liệu trống.");
            await app.Services.GetRequiredService<LocalIdentity>().Seed();
            await SeedData.Run(db, app.Services.GetRequiredService<FileStore>(), auth.Issuer);
        }
        return args.Contains("--initialize");
    }

    private sealed class KeycloakControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var localAuthController = feature.Controllers.FirstOrDefault(type => type.AsType() == typeof(XacThucController));
            if (localAuthController is not null)
                feature.Controllers.Remove(localAuthController);
        }
    }
}
