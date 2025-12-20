using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Play.Catalog.Service.Entities;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Common.Settings;

var builder = WebApplication.CreateBuilder(args);

// var allowedOrigin = builder.Configuration
//     .GetSection("AllowedOrigin")
//     .Get<string[]>();


var allowedOrigin = builder.Configuration["AllowedOrigin"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigin!)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(options =>
    options.SuppressAsyncSuffixInActionNames = false
);

    var serviceSettings = builder.Configuration
        .GetSection("ServiceSettings")
        .Get<ServiceSettings>();


builder.Services.AddMongo(builder.Configuration)
    .AddMongoRepository<Item>("items")
    .AddMassTransitWithRabbitMq();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:5003";
        options.Audience = serviceSettings.ServiceName;
    });

var app = builder.Build();

app.UseCors("CorsPolicy");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();            
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();