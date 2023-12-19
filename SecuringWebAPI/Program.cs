using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SecuredWebAPIBestPractices.Repositories.Domain;
using SecuringWebAPI.Data;
using SecuringWebAPI.Model.Domain;
using SecuringWebAPI.Repositories.Abstract;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<SecuringWebAPIContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ), ServiceLifetime.Scoped);


//registering the service for identity core.
builder.Services.AddIdentity<ApplicationUser,
                IdentityRole>().AddEntityFrameworkStores<SecuringWebAPIContext>().AddDefaultTokenProviders();

builder.Services.AddLogging();


builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
//var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

//builder.Configuration.Bind(nameof(jwtsettings),jwtsettings);
//builder.Services.AddSingleton(new JwtSettings());


////Adding authentication
builder.Services.AddAuthentication(
   options =>
   {
       options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
       options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
       options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
   })
     //adding jwt bearer as authentication
     .AddJwtBearer(options =>
     {

         options.SaveToken = true;
         options.RequireHttpsMetadata = false;
         options.TokenValidationParameters = new TokenValidationParameters
         {
             
             ValidateIssuer = false,
             ValidateAudience = false,
             ValidateLifetime = true,
             ValidateIssuerSigningKey = true,
             RequireExpirationTime = false,
             ValidIssuer = builder.Configuration["Jwt:ValidIssuer"],
             ValidAudience = builder.Configuration["Jwt:ValidAudience"],
             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
         };
     });

builder.Services.AddScoped<IIdentityService, IdentityService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(x =>
{
    var security = new Dictionary<string, IEnumerable<string>>
    {
        { "Bearer", new string[0] }
    };

    var securityScheme = new OpenApiSecurityScheme
    {
        Description = "JWT authorization header using the bearer scheme",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "Authorization", // Fix the typo in the header name
    };

    x.AddSecurityDefinition("Bearer", securityScheme);
    x.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new List<string>() }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();
//app.UseMvc();
app.Run();
