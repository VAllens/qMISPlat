using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CPFrameWork.Organ;
using Microsoft.EntityFrameworkCore.Infrastructure;
using CPFrameWork.Organ.Infrastructure;
using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using CPFrameWork.Global;
using CPFrameWork.UIInterface.Form;
using CPFrameWork.Utility;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;
using CPFrameWork.UIInterface.Grid;
using CPFrameWork.UIInterface.Tab;
using CPFrameWork.Global.Systems;
using System.Reflection;
using UEditorNetCore;
using CPFrameWork.UIInterface.Tree;
using CPFameWork.Portal.Module;
using CPFrameWork.Organ.Application;
using CPFrameWork.Flow;
using CPFrameWork.Global.Msg;

namespace CPFrameWork
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        { 
            var builder = new ConfigurationBuilder()
              .SetBasePath(env.ContentRootPath)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
               
              .AddEnvironmentVariables();
            
            //开发环境
            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            HostingEnvironment = env;
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }
        public IServiceCollection Services { get; set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        //注册服务
        public void ConfigureServices(IServiceCollection services)
        { 
            // Add framework services.
            //不加这句的话，发布到IIS里会运行不起来
            services.AddApplicationInsightsTelemetry(Configuration);
            //注册编辑器
            services.AddUEditorService();
            services.AddMvc(o => o.Conventions.Add(new FeatureConvention()))
                .AddJsonOptions(t => t.SerializerSettings.ContractResolver = new DefaultContractResolver())
                .AddRazorOptions(options =>
                {
                    // {0} - Action Name
                    // {1} - Controller Name
                    // {2} - Area Name
                    // {3} - Feature Name
                    // replace normal view location entirely 
                    //    options.ViewLocationFormats.Clear();
                    options.ViewLocationFormats.Add("/Plat/{3}/{1}/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Plat/{3}/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Plat/Shared/{0}.cshtml");


                    options.ViewLocationExpanders.Add(new FeatureViewLocationExpander());

                }).AddSessionStateTempDataProvider();

            //.AddApplicationPart(typeof(GridEngineController).Assembly).AddControllersAsServices(); ;
            //services.AddMvc()
            // .AddApplicationPart(typeof(GridEngineController).Assembly).AddControllersAsServices();
            //services.AddMvc().AddApplicationPart(Assembly.Load(new AssemblyName("CPFrameWork.UIInterface")));
            //使用session
            services.AddSession();
            Services = services;
            //设置文件大小
            services.Configure<FormOptions>(options =>
            {
                //1个G
                options.MultipartBodyLengthLimit = 1024*1024*1024;
            });
            //自己加的
            CPLogHelper.StartupInit(services, Configuration);
            CPSystemHelper.StartupInit(services, Configuration);
            CPAutoNumHelper.StartupInit(services, Configuration);
            COOrgans.StartupInit(services, Configuration);
            CPFormEngine.StartupInit(services, Configuration);
            CPFormTemplate.StartupInit(services, Configuration);
            CPGridEngine.StartupInit(services, Configuration);
            CPTabEngine.StartupInit(services, Configuration);
            CPTreeEngine.StartupInit(services, Configuration);
            CPModuleEngine.StartupInit(services, Configuration);
            CPFlowTemplate.StartupInit(services, Configuration);
            CPFlowEngine.StartupInit(services, Configuration);
            CPMsgs.StartupInit(services, Configuration);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            //自己加的
            CPAppContext.HttpContextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();
            CPAppContext.Configuration = Configuration;
            CPAppContext.ServiceCollection = Services;
            CPAppContext.HostingEnvironment = HostingEnvironment;
            CPUtils.Configuration = Configuration;
            if (env.IsDevelopment())
            {
                //开发环境
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            //允许访问wwwroot文件夹下的静态文件
            app.UseStaticFiles();
            //SEssion
            app.UseSession();
            // 设置MVC路由
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                  name: "PlatRoute",
                  template: "Plat/[controller]/[action]/{id?}"); 
            });

            
             
        }
    }
}
