using Microsoft.EntityFrameworkCore;
using MediatR;
using V_Eval_Content_Service.Infrastructure.Persistence;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.MockExams.Commands.ImportMockExam;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Cấu hình kết nối PostgreSQL Supabase
builder.Services.AddDbContext<ContentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ContentDbContext).Assembly.FullName)));

// Đăng ký Dependency Injection cho IContentDbContext
builder.Services.AddScoped<IContentDbContext>(provider => provider.GetRequiredService<ContentDbContext>());

// Đăng ký MediatR cho Application Layer
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ImportMockExamCommand).Assembly));

// Cấu hình CORS
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

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Endpoint Minimal API để import dữ liệu đề thi đã bóc tách từ AI Engine
app.MapPost("/api/content/exams/import", async (ImportMockExamCommand command, IMediator mediator) =>
{
    try
    {
        var examId = await mediator.Send(command);
        return Results.Ok(new { exam_id = examId, message = "Import đề thi thành công vào Supabase PostgreSQL!" });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500, title: "Lỗi khi import đề thi");
    }
})
.WithName("ImportMockExam")
.DisableAntiforgery();

app.Run();
