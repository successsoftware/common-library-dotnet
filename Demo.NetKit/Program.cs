using Demo.NetKit.Data;
using Demo.NetKit.Mapping;
using Demo.NetKit.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SSS.AspNetCore.Extensions;
using SSS.AspNetCore.Extensions.Handlers;
using SSS.AspNetCore.Extensions.Jwt;
using SSS.AspNetCore.Extensions.ServiceProfiling;
using SSS.AspNetCore.Extensions.Swagger;
using SSS.AspNetCore.Extensions.Versioning;
using SSS.CommonLib.Interfaces;
using SSS.EntityFrameworkCore.Extensions;
using WebApi.Sample;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDbContext, TodoContext>();
builder.Services.AddAuditContext<TodoContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSwaggerConfig(new SwaggerInfo
{
    Title = "WebApi Sample",
    Version = "v1.0"
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddMapping();

builder.Services.AddValidation();

builder.Services.AddVersioning();

builder.Services.AddJwtBearerToken(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.AllowCredentials().AllowAnyHeader().AllowAnyMethod().WithOrigins(new string[] { "https://localhost:5001" });
    });
});

builder.Services.AddScoped<IDateTimeService, DateTimeService>();

builder.Services.AddProfiling<IToDoService, ToDoService>();

builder.Services.AddTransient<SeedData>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using var scope = app.Services.CreateScope();

var seeder = scope.ServiceProvider.GetService<SeedData>();

if (seeder is null) throw new ArgumentNullException(nameof(seeder), "{seedData} is not null");

seeder.PopulateData();

app.UseGlobalExceptionHandler();

app.UseCors("DefaultPolicy");

app.UseJwtBearerTokenMiddleware(builder.Configuration);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
