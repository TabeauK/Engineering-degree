using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Serilog;
using UsosFix.ExchangeRealization;
using UsosFix.ExchangeRealization.MinCostFlowAlgorithms;
using UsosFix.Services;
using UsosFix.UsosApi;

namespace UsosFix
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddCors(options =>
            {
                options.AddPolicy("Default",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });

                options.AddPolicy("SignalR", builder =>
                {
                    builder.WithOrigins("https://usosfix-ui.herokuapp.com")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            services.AddSignalR().AddJsonProtocol(options => {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddDbContext<ApplicationDbContext>(op =>
            {
                var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL") ??
                            throw new ConfigurationErrorsException("Missing database connection string.");
                var databaseUri = new Uri(dbUrl);
                var userInfo = databaseUri.UserInfo.Split(':');
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = databaseUri.Host,
                    Port = databaseUri.Port,
                    Username = userInfo[0],
                    Password = userInfo[1],
                    Database = databaseUri.LocalPath.TrimStart('/'),
                    SslMode = SslMode.Require,
                    TrustServerCertificate = true
                };
                op.UseNpgsql(builder.ConnectionString);
            });

            services.AddSingleton(new ApiConnector("http://apps.usos.pw.edu.pl/",
                "8AcjB4QBJHuneWSfYWfy",
                "aUsgJvWQxMq2hL8UHpe7wqHFa2VaCTF8T2pQYj7K"));

            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(s => s.FullName);
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, "publish", xmlFile);
                xmlPath = File.Exists(xmlPath) ? xmlPath : Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);
            });

            services.AddHttpClient();

            services.AddScoped<DataLoader>();

            services.AddScoped<TimetableService>();
            services.AddScoped<IExchangeSolver, GoogleOrSolver>();

            var sendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_KEY") ??
                                 throw new ConfigurationErrorsException("Missing SendGrid API key.");
            services.AddScoped<IMailService>(provider => new MailService(sendGridApiKey,
                provider.GetService<TimetableService>() ??
                throw new InvalidOperationException($"No registered service of type {typeof(TimetableService)}")));

            services.AddScoped<ISemesterService, SemesterService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "UsosFix API");
                c.RoutePrefix = string.Empty;
            });
            
            app.UseRouting();
            
            app.UseWebSockets();

            app.UseCors("Default");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/chat", options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                }).RequireCors("SignalR");
                endpoints.MapControllers();
            });
        }
    }
}
