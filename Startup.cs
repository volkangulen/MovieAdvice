using System;
using System.Text;
using MAdvice.DAL;
using MAdvice.Services;
using MAdvice.Services.Background;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace MAdvice
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
            //Cross origin resource paylaşımına izin veirr.
            services.AddCors();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //ClockSkew default 5 dk olan zaman aşımını engellemek için kullanıldı.
                    ClockSkew = TimeSpan.Zero,
                    //Validation tokenı zamanlı vermek için.
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"])),
                    
                };
            });

            //Postgre DBContext konfigurasyonu. Bağlantı bilgilerini appsettings'den alıyor.
            services.AddEntityFrameworkNpgsql().AddDbContext<MovieContext>(opt =>
                opt.UseNpgsql(Configuration.GetConnectionString("MovieConnection")));

            //Movie ve Auth servisleri ekleniyor.
            services.AddScoped<IMovieService, MovieService>();
            services.AddScoped<IAuthService, AuthService>();

            //Task servisi ekleniyor.
            services.AddSingleton<IHostedService, SeederTaskService>();

            //Controllerlar ekleniyor.
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.UseCors(x => x
            //    .AllowAnyOrigin()
            //    .AllowAnyMethod()
            //    .AllowAnyHeader());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();


            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
