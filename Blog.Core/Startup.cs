using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Blog.Core.Common.Helper;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Autofac;
using Blog.Core.IServices;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.DynamicProxy;
using Blog.Core.Extensions;
using Blog.Core.AOP;
using Blog.Core.Common.MemoryCache;

namespace Blog.Core
{
    public class Startup
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }



        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {

            var basePath = AppContext.BaseDirectory;
            var basePath2 = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;//这种方法可以取得路径
            //数据库配置
            //BaseDBConfig.ConnectionString = Configuration.GetSection("Appsettings:Mysql:ConnectionStrings").Value;
           services.AddSqlsugarSetup();

            #region Swagger
            //配置项目注释到Swagger中，此步骤需要在项目-属性-生成，勾选输出下面得XML文件，,一般得生成路径为..\Blog.Core\Blog.Core.xml，这样可以将xml文件内直接生成到bin目录下

            services.AddSwaggerGen(c =>
            {
                #region 配置Swagger的基本说明
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1.0.0",
                    Title = "Blog.Core.API",
                    Description = "框架说明文档",
                    TermsOfService = null,
                    Contact = new OpenApiContact
                    {
                        Name = "Blog.Core",
                        Email = "Blog.Core@xxx.com",
                        Url = new Uri("https://www.jianshu.com/u/94102b59cc2a")
                    }
                });
                #endregion

                #region 读取xml信息，并配置注释
                //配置本项目注释

                var xmlPath = Path.Combine(basePath, "Blog.Core.xml");
                c.IncludeXmlComments(xmlPath, true);//查看方法可知，第二个参数是是否考虑controller的注释

                //配置连结项目注释
                var modelPath = Path.Combine(basePath2, "Blog.Core.Model.xml");
                c.IncludeXmlComments(modelPath);
                #endregion

                #region Token绑定到Configuration
                //添加header验证信息
                //c.OperationFilter<SwaggerHeader>();
                //var security = new Dictionary<string, IEnumerable<string>> { { "Blog.Core", new string[] { } }, };

                var securitySchema = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Blog.Core",
                        Type = ReferenceType.SecurityScheme
                    }
                };
                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                    securitySchema,Array.Empty<string>()
                    }
                };

                c.AddSecurityRequirement(securityRequirement);

                //方案名称“Blog.Core”可自定义，上下一致即可     
                c.AddSecurityDefinition("Blog.Core", new OpenApiSecurityScheme
                {
                    Description = "JWT授权(数据将在请求头中进行传输) 直接在下框中输入Bearer {token}（注意两者之间是一个空格）\"",
                    Name = "Authorization",//jwt默认的参数名称
                    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.ApiKey

                });
                #endregion
            });
            #endregion

            #region 接口授权之角色认证
            services.AddAuthorization(c =>
            {
                #region 添加认证策略，可以在控制器中查看对应角色        [Authorize(Roles  = "Admin")]
                c.AddPolicy("Client", policy => policy.RequireRole("Client").Build());
                c.AddPolicy("Admin", policy => policy.RequireRole("Admin").Build());
                c.AddPolicy("SystemOrAdmin", policy => policy.RequireRole("Admin", "System"));
                c.AddPolicy("SystemAndAdmin", policy => policy.RequireRole("Admin").RequireRole("System"));
                #endregion

            });
            #endregion


            // 可以将一个用户的多个角色全部赋予；
            // 作者：DX 提供技术支持；
            // claims.AddRange(tokenModel.Role.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));

            #region 认证 所需参数
            //读取配置文件    
            var audienceConfig = Configuration.GetSection("Audience");
            var symmetricKeyAsBase64 = audienceConfig["Secret"];
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            #endregion

            #region 认证            
            services.AddAuthentication(c =>
            {
                //此部分即为AuthenticationOptions的委托，用于配置scheme
                //如果不进行认证服务配置的话，会报错：
                //No authenticationScheme was specified, and there was no DefaultChallengeScheme found.
                c.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                c.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })// 也可以直接写字符串，AddAuthentication("Bearer")             
                .AddJwtBearer(c =>
                {
                    c.TokenValidationParameters = new TokenValidationParameters
                    {
                        //3+2(密钥 发行人 订阅人+过期时间的两个)
                        //密钥
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,//参数配置在下边
                        //发行人
                        ValidateIssuer = true,
                        ValidIssuer = audienceConfig["Issuer"],
                        //订阅人
                        ValidateAudience = true,
                        ValidAudience = audienceConfig["Audience"],

                        //过期时间
                        RequireExpirationTime = true,

                        ValidateLifetime = true,

                        //这里是为了以防万一，一般加上，但是主要的上面的
                        ClockSkew = TimeSpan.Zero,//这个是缓冲过期时间，也就是说，即使我们配置了过期时间，这里也要考虑进去，过期时间+缓冲，默认好像是7分钟，你可以直接设置为0

                    };
                });
            #endregion


            services.AddMvc();

            services.AddScoped<ICaching, MemoryCaching>();//注入缓存
            #region 依赖注入
            ////初始化AutoFac容器
            var builder = new ContainerBuilder();

            #region 其他类型的注入
            //拦截器的依赖注入
            builder.RegisterType<BlogLogAOP>();
            //缓存的依赖注入
            builder.RegisterType<BlogCacheAOP>();
            #endregion


            #region 项目层级的依赖注入
            #region 普通方法，与netcore提供的方法一致
            ////注册要通过反射创建的组件
            //builder.RegisterType<AdvertisementServices>().As<IAdvertisementServices>();
            #endregion

            #region Load模式 用于批量注入 此处需要在在API层加载引用或者是将services层和repository层的dll文件生成到api层中
            #region  API层加载引用时方法  Load
            //在API层加载引用时使用
            ////通过反射加载Serviecs程序集(加载解决方案名)
            //var assemblyServices = Assembly.Load("Blog.Core.Services");
            ////指定已扫描程序集中的类型注册为提供所有其实现的接口。
            //builder.RegisterAssemblyTypes(assemblyServices).AsImplementedInterfaces();

            ////通过反射加载Repository程序集合(加载解决方案名)
            //var assemblysRepository = Assembly.Load("Blog.Core.Repository");
            ////指定已扫描程序集中的类型注册为提供所有其实现的接口。
            //builder.RegisterAssemblyTypes(assemblysRepository).AsImplementedInterfaces();
            #endregion

            #region 引用dll文件时方法 LoadFile
            var servicesDllFile = Path.Combine(basePath, "Blog.Core.Services.dll");
            var assemblyServices = Assembly.LoadFile(servicesDllFile);
            builder.RegisterAssemblyTypes(assemblyServices)
                .AsImplementedInterfaces()
                .InstancePerDependency()
                .EnableInterfaceInterceptors()//引用Autofac.Extras.DynamicProxy;//对目标类型启用接口拦截。拦截器将被确定，通过在类或接口上截取属性, 或添加
                .InterceptedBy(typeof(BlogCacheAOP));//可以直接替换拦截器


            var repositoryDllFile = Path.Combine(basePath, "Blog.Core.Repository.dll");
            var assemblyRepository = Assembly.LoadFile(repositoryDllFile);
            builder.RegisterAssemblyTypes(assemblyRepository)
                .AsImplementedInterfaces();
            #endregion
            #endregion
            #endregion

            ////将services填充Autofac容器生成器
            builder.Populate(services);

            //使用已进行的组件登记创建新容器
            var ApplicationContainer = builder.Build();
            //第三方IOC接管 core内置DI容器
            return new AutofacServiceProvider(ApplicationContainer);
            #endregion





        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // 在开发环境中，使用异常页面，这样可以暴露错误堆栈信息，所以不要放在生产环境
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // 在非开发环境中，使用HTTP严格安全传输(or HSTS) 对于保护web安全是非常重要的。
                // 强制实施 HTTPS 在 ASP.NET Core，配合 app.UseHttpsRedirection
                app.UseExceptionHandler("/Error");
                //app.UseHsts();
            }

            #region Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                var apiName = "Blog.Core";
                //根据版本名称倒序 遍历展示
                //var apiName = AppSettings.app(new string[] { "Startup", "AppSettings" });
                var version = "v1";

                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{apiName} {version}");


                //配置swagger首页信息--替换swagger的首页为自定义的index.html
                //c.IndexStream = () => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("Blog.Core.index.html");
                //c.IndexStream = () => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("Blog.Core.index.html");
                //这里是配合MiniProfiler进行性能监控的，《文章：完美基于AOP的接口性能分析》，如果你不需要，可以暂时先注释掉，不影响大局。
                //需要安装MiniProfiler，并在index.html配置对应的版本


                //配置访问swagger的访问路径--/index.html为首先访问页
                c.RoutePrefix = ""; //我们可以指定字符串，作为 swagger 的地址
                //路径配置，设置为空，表示直接在根域名(localhost:5000)访问该文件,此时localhost:5000/swagger不能访问
                //这个时候去 launchsetting.json中把"launchUrl": "swagger/index.html"，然后直接访问localhost:5000/index.html即可


            });


            #endregion

            #region Authentication
            //app.UseMiddleware<JwtTokenAuth>();//注意此授权方法已经放弃，请使用下边的官方验证方法。但是如果你还想传User的全局变量，还是可以继续使用中间件
            app.UseAuthentication();
            #endregion

            #region CORS
            //跨域第二种方法，使用策略，详细策略信息在ConfigureService中
            app.UseCors("LimitRequests");//将 CORS 中间件添加到 web 应用程序管线中, 以允许跨域请求。


            //跨域第一种版本，请要ConfigureService中配置服务 services.AddCors();
            //    app.UseCors(options => options.WithOrigins("http://localhost:8021").AllowAnyHeader()
            //.AllowAnyMethod()); 
            #endregion
            //跳转https
            app.UseHttpsRedirection();

            //使用静态文件
            app.UseStaticFiles();

            //使用Cookie
            app.UseCookiePolicy();

            //返回错误码
            app.UseStatusCodePages();//把错误码返回前台          

            app.UseMvc();


        }
    }
}
