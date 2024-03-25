using DemoWebAPI.Core.Extensions;
using DemoWebAPI.Core.Model;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

var appSettingSection = configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingSection);
builder.Services.AddJwtAuthorization(configuration);

var centerConfig = new CenterConfig();
new ConfigureFromConfigurationOptions<CenterConfig>(configuration).Configure(centerConfig);
builder.Services.AddSingleton(centerConfig);

var app = builder.Build();

// Sử dụng Cors middleware
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSetAuthContextHandler(centerConfig.AppSettings.RealUrlCheckAuth);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
