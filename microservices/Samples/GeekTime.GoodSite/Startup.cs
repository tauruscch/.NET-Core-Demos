using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GeekTime.GoodSite
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
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/home/login";
                    options.Cookie.HttpOnly = true;  // 设置为true，表示客户端不能通过脚本读取cookie
                });
            services.AddControllersWithViews();

            // 启用防跨站脚本攻击策略，设置请求头，服务端会同时校验指定header的值和cookie中的值是否一致
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });

            // 开启全局AntiforgeryToken验证
            //services.AddMvc(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

            // 1. 开启跨域，并增加跨域策略
            services.AddCors(options =>
            {
                options.AddPolicy("api", builder =>
                 {
                     builder.WithOrigins("https://localhost:5001") // 设置运行跨域访问的源地址
                        .AllowAnyHeader() // 允许携带任何header
                        .AllowCredentials() // 允许携带认证信息，比如：cookie
                        .WithExposedHeaders("abc"); // 允许脚本能够访问到的响应header

                     // 也可以使用委托来设置运行跨域访问的源
                     builder.SetIsOriginAllowed(orgin => {
                         return true;
                     }).AllowCredentials().AllowAnyHeader();
                 });
            });


            #endregion

            // 增加缓存组件
            services.AddMemoryCache();
            services.AddStackExchangeRedisCache(options =>
            {
                Configuration.GetSection("RedisCache").Bind(options);
            });
            services.AddResponseCaching();
            services.AddEasyCaching(options =>
            {
                options.UseRedis(Configuration, name: "easycaching");
            });
            
        }

        //public void ConfigureContainer(ContainerBuilder builder)
        //{

        //}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseResponseCaching();

            // 2. 使用跨域中间件
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
