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

            // ��ȡ��Կ��ע�룬��Ϊ����controller�ж�ȡ������JWT��token
            var secrityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecurityKey"]));
            services.AddSingleton(secrityKey);
            // ����Ĭ�ϵ���֤����Ϊcookie��AddCookie��������cookie��֤������AddJwtBearer��������JWT��֤������������֤��ʽ����ͬʱ֧��
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    // ����cookie����ʱ���
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,//�Ƿ���֤ǩ����
                        ValidateAudience = true,//�Ƿ���֤������
                        ValidateLifetime = true,//�Ƿ���֤ʧЧʱ��
                        ClockSkew = TimeSpan.FromSeconds(30), // ʧЧ��ƫ��ʱ�䣬ʧЧ30���ڻ�����
                        ValidateIssuerSigningKey = true,//�Ƿ���֤SecurityKey
                        ValidAudience = "localhost",//��Ч��ǩ����
                        ValidIssuer = "localhost",//��Ч�Ľ�����
                        IssuerSigningKey = secrityKey//����У���SecurityKey
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

            // ������֤����Ȩ�����Ⱥ�˳������UseEndpoints֮ǰ
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
