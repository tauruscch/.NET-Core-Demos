using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Ocelot;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace GeekTime.Mobile.Gateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region 
            //services.AddResponseCaching(options =>
            //{

            //});
            services.AddHealthChecks();

            // 读取秘钥并注入，是为了在controller中读取来生成JWT的token
            var secrityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecurityKey"]));
            services.AddSingleton(secrityKey);
            // 设置默认的认证方案为cookie，AddCookie方法增加cookie认证方案，AddJwtBearer方法增加JWT认证方案，两种认证方式可以同时支持
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    // 设置cookie过期时间等
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,//是否验证签发者
                        ValidateAudience = true,//是否验证接受者
                        ValidateLifetime = true,//是否验证失效时间
                        ClockSkew = TimeSpan.FromSeconds(30), // 失效的偏离时间，失效30秒内还可用
                        ValidateIssuerSigningKey = true,//是否验证SecurityKey
                        ValidAudience = "localhost",//有效的签发者
                        ValidIssuer = "localhost",//有效的接受者
                        IssuerSigningKey = secrityKey//传入校验的SecurityKey
                    };
                });
            #endregion

            services.AddOcelot(Configuration);
            #region

            //HttpClientHandler handler = new HttpClientHandler();

            //SocketsHttpHandler socketsHttpHandler = new SocketsHttpHandler();
            //var cc = new HttpClient();
            //var client = new HttpClient(socketsHttpHandler, disposeHandler: false);

            //client.Dispose();
            services.AddControllers();

            services.AddHttpClient("sss").ConfigurePrimaryHttpMessageHandler(() =>
            {
                var hand = new SocketsHttpHandler();
                ///SET Your Proxy
                return new SocketsHttpHandler();
            }).SetHandlerLifetime(TimeSpan.FromSeconds(60));


            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
                options.ForwardedHeaders = ForwardedHeaders.All;
            });
            #endregion


            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.UseHttpsRedirection();

            if (Configuration.GetValue("USE_Forwarded_Headers", false))
            {
                app.UseForwardedHeaders();
            }
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            //app.UseResponseCaching();

            // 启用认证和授权，有先后顺序，且在UseEndpoints之前
            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/live");
                endpoints.MapHealthChecks("/ready");
                endpoints.MapHealthChecks("/hc", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapControllers();

                endpoints.MapDefaultControllerRoute();
            });

            app.UseOcelot().Wait();
        }
    }
}
