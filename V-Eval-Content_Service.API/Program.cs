using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using V_Eval_Content_Service.API.Middlewares;
using V_Eval_Content_Service.API.Services;
using V_Eval_Content_Service.Application.Common.Behaviors;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Diagnostic.Queries.GetDiagnosticTest;
using V_Eval_Content_Service.Infrastructure.Persistence;
using V_Eval_Content_Service.Infrastructure.Persistence.Seeds;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Kestrel: Cổng 5249 cho REST/Swagger (HTTP/1), Cổng 5250 cho gRPC Server (HTTP/2)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5249, lo => lo.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
    options.ListenLocalhost(5250, lo => lo.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
});

// 1. Đăng ký Controllers & API Explorer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Cấu hình Swagger UI trực quan
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "V-Eval Content Service API",
        Version = "v1",
        Description = "Microservice quản lý ngân hàng câu hỏi khảo thí, đề thi mô phỏng và bài thi chẩn đoán năng lực ban đầu (V-Eval Core Flow 1 - Bước 2)."
    });
});

// 3. Cấu hình kết nối PostgreSQL Supabase (Schema content)
builder.Services.AddDbContext<ContentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ContentDbContext).Assembly.FullName)));

// 4. Đăng ký Dependency Injection cho IContentDbContext
builder.Services.AddScoped<IContentDbContext>(provider => provider.GetRequiredService<ContentDbContext>());

// 5. Đăng ký FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(GetDiagnosticTestQuery).Assembly);

// 6. Đăng ký MediatR kèm ValidationBehavior pipeline
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetDiagnosticTestQuery).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// 7. Cấu hình gRPC Server
builder.Services.AddGrpc();

// 8. Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 9. Middleware xử lý lỗi toàn cục
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseCors("AllowAll");

// 10. Kích hoạt Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "V-Eval Content Service API v1");
    c.RoutePrefix = "swagger";
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 11. Đăng ký Controllers và gRPC Service
app.MapControllers();
app.MapGrpcService<ContentGrpcService>();

// 12. Tự động kiểm tra & Seed đề thi chẩn đoán 30 câu khi khởi động
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = services.GetRequiredService<ContentDbContext>();
        await DiagnosticExamSeeder.SeedDiagnosticExamAsync(dbContext, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Lỗi khi chạy DiagnosticExamSeeder khởi tạo dữ liệu mẫu.");
    }
}

app.Run();
