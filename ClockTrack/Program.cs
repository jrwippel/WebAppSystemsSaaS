using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;
using ClockTrack.Data;
using ClockTrack.Helper;
using ClockTrack.Services;
using ClockTrack.Services.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;

namespace ClockTrack
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Define a cultura padrão para "pt-BR"
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

            var builder = WebApplication.CreateBuilder(args);

            // Configurar o contexto do banco de dados
            builder.Services.AddDbContext<ClockTrackContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ClockTrackContext")
                ?? throw new InvalidOperationException("Connection string 'ClockTrackContext' not found.")));

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddScoped<SeedingService>();
            builder.Services.AddScoped<BasicAuthenticationFilterAttribute, BasicAuthenticationFilterAttribute>();
            
            // Multi-tenant service (SaaS)
            builder.Services.AddScoped<ITenantService, TenantService>();
            
            // Segurança - Controle de tentativas de login
            builder.Services.AddSingleton<LoginAttemptService>();
            
            builder.Services.AddScoped<AttorneyService>();
            builder.Services.AddScoped<DepartmentService>();
            builder.Services.AddScoped<ProcessRecordService>();
            builder.Services.AddScoped<ProcessRecordsService>();
            builder.Services.AddScoped<ClientService>();
            builder.Services.AddScoped<ISessao, Sessao>();
            builder.Services.AddScoped<IEmail, Email>();
            builder.Services.AddScoped<MensalistaService>();
            builder.Services.AddScoped<ValorClienteService>();
            builder.Services.AddScoped<ParametroService>();
            builder.Services.AddScoped<ActivityTypeService>();
            builder.Services.AddHttpClient<MercadoPagoService>();

            builder.Services.AddScoped<ISpeechToTextService, SpeechToTextService>();
            builder.Services.AddScoped<ISummaryService, SummaryService>();

            // Assistente Jurídico IA
            builder.Services.AddScoped<DocumentTextExtractorService>();
            builder.Services.AddScoped<AIDocumentAnalysisService>();
            builder.Services.AddScoped<AttorneyRecommendationService>();

            builder.Services.AddHttpClient();

            builder.Services.AddSession(o =>
            {
                o.Cookie.HttpOnly = true;
                o.Cookie.IsEssential = true;
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.Cookie.SameSite = SameSiteMode.Strict;
                o.IdleTimeout = TimeSpan.FromMinutes(60);
            });

            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();

            // Limite de upload de arquivos (5MB)
            builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 5 * 1024 * 1024; // 5MB
            });
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 5 * 1024 * 1024; // 5MB
            });

            // Configurar autenticação JWT
            var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            // Registra o HttpClient
            builder.Services.AddHttpClient();

            // KeepAlive desativado no F1 (não tem Always On, consome CPU desnecessariamente)
            // builder.Services.AddHostedService<KeepAliveService>();

            var app = builder.Build();

            var ptBR = new CultureInfo("pt-BR");
            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(ptBR),
                SupportedCultures = new List<CultureInfo> { ptBR },
                SupportedUICultures = new List<CultureInfo> { ptBR }
            };

            app.UseRequestLocalization(localizationOptions);

            // Aplicar migrações pendentes no banco de dados
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var myDbContext = services.GetRequiredService<ClockTrackContext>();
                    myDbContext.Database.Migrate();
                    // Garante que AttorneyId seja nullable (valor padrão por cliente)
                    myDbContext.Database.ExecuteSqlRaw(
                        "IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='ValorCliente' AND COLUMN_NAME='AttorneyId' AND IS_NULLABLE='NO') " +
                        "ALTER TABLE ValorCliente ALTER COLUMN AttorneyId INT NULL");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }

            // Seed data
            try
            {
                app.Services.CreateScope().ServiceProvider.GetRequiredService<SeedingService>().Seed();
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }

            app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

            // Security Headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
                await next();
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Adicionar middlewares de autenticação e autorização
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers(); // Adicione suporte ao roteamento da API

            app.MapControllerRoute(
                name: "about",
                pattern: "about",
                defaults: new { controller = "Home", action = "About" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
