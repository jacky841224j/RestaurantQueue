using FrontStage.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using NLog;
using NLog.Web;
using CommonLibrary.Middleware;
using CommonLibrary.Service;
using CommonLibrary.Dto;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("�Ұʵ{��");

try
{

    var builder = WebApplication.CreateBuilder(args);
    var jwtConfigSection = builder.Configuration.GetSection(nameof(JwtConfig));
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();


    builder.Services.Configure<JwtConfig>(jwtConfigSection);
    // Add services to the container.

    builder.Services.AddControllers();
    builder.Services.AddScoped<RedisService>();
    builder.Services.AddScoped<DbService>();
    builder.Services.AddScoped<QueueService>();
    builder.Services.AddScoped<IdentityService>();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    //�]�wSwagger
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "�\�U�Ԧ�����t��",
            Description = "�\�U�Ԧ�����t��",
            TermsOfService = new Uri("https://igouist.github.io/post/2021/10/swagger-enable-authorize/"),
            Contact = new OpenApiContact
            {
                Name = "Tian",
                Email = "jacky841224j@gmail.com",
                Url = new Uri("https://github.com/jacky841224j/")
            }

        });

        //��swagger�ͦ�����
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);

        options.AddSecurityDefinition("Bearer",
            new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Example: \"Bearer xxxxxxxxxxxxxxx\""
            });

        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
            });
    });

    //�]�w����
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
        .AddJwtBearer(jwt =>
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var jwtSecret = config.GetSection("JwtConfig")["SecretKey"];

            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JwtConfig:Secret is missing or empty in appsettings.json");
            }

            jwt.SaveToken = true;
            jwt.TokenValidationParameters = new TokenValidationParameters
            {
                RequireExpirationTime = false,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });


    //���Usqlite
    builder.Services.AddScoped(x =>
    {
        string SavePath = $" ..\\{builder.Configuration["Sqlite:DbName"]}.db";
        return new SqliteConnection($"Data Source={SavePath}");
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseMiddleware<ExceptionHandling>();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}

