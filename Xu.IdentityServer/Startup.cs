using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;
using Xu.IdentityServer.Authorization;
using Xu.IdentityServer.Data;
using Xu.IdentityServer.Extensions;
using Xu.IdentityServer.Helper;
using Xu.IdentityServer.Models;

namespace Xu.IdentityServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSameSiteCookiePolicy();

            //��ȡ���ݿ������ַ���(ͨ���ļ���ȡ)
            string connectionStringFile = Configuration.GetConnectionString("DefaultConnection_file");
            var connectionString = File.Exists(connectionStringFile) ? File.ReadAllText(connectionStringFile).Trim() : Configuration.GetConnectionString("DefaultConnection");
            var isMysql = Configuration.GetConnectionString("IsMysql").ObjToBool();

            if (connectionString == "")
            {
                throw new Exception("���ݿ������쳣");
            }
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            // ���ݿ�����ϵͳӦ���û�����������
            if (isMysql)
            {
                // mysql
                services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(connectionString));
            }
            else
            {
                // sqlserver
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            };

            services.Configure<IdentityOptions>(
              options =>
              {
                  //options.Password.RequireDigit = false;
                  //options.Password.RequireLowercase = false;
                  //options.Password.RequireNonAlphanumeric = false;
                  //options.Password.RequireUppercase = false;
                  //options.SignIn.RequireConfirmedEmail = false;
                  //options.SignIn.RequireConfirmedPhoneNumber = false;
                  //options.User.AllowedUserNameCharacters = null;
              });

            // ���� Identity ���� ���ָ�����û��ͽ�ɫ���͵�Ĭ�ϱ�ʶϵͳ����
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User = new UserOptions
                {
                    RequireUniqueEmail = true, //Ҫ��EmailΨһ
                    AllowedUserNameCharacters = null //������û����ַ�
                };
                options.Password = new PasswordOptions
                {
                    RequiredLength = 8, //Ҫ��������С���ȣ�Ĭ���� 6 ���ַ�
                    RequireDigit = true, //Ҫ��������
                    RequiredUniqueChars = 3, //Ҫ������Ҫ���ֵ���ĸ��
                    RequireLowercase = true, //Ҫ��Сд��ĸ
                    RequireNonAlphanumeric = false, //Ҫ�������ַ�
                    RequireUppercase = false //Ҫ���д��ĸ
                };

                //options.Lockout = new LockoutOptions
                //{
                //    AllowedForNewUsers = true, // ���û������˻�
                //    DefaultLockoutTimeSpan = TimeSpan.FromHours(1), //����ʱ����Ĭ���� 5 ����
                //    MaxFailedAccessAttempts = 3 //��¼��������Դ�����Ĭ�� 5 ��
                //};
                //options.SignIn = new SignInOptions
                //{
                //    RequireConfirmedEmail = true, //Ҫ�󼤻�����
                //    RequireConfirmedPhoneNumber = true //Ҫ�󼤻��ֻ���
                //};
                //options.ClaimsIdentity = new ClaimsIdentityOptions
                //{
                //    // ���ﶼ���޸���Ӧ��Cliams������
                //    RoleClaimType = "IdentityRole",
                //    UserIdClaimType = "IdentityId",
                //    SecurityStampClaimType = "SecurityStamp",
                //    UserNameClaimType = "IdentityName"
                //};
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = new PathString("/oauth2/authorize");
            });

            //����session����Чʱ��,��λ��
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(30);
            });

            services.AddMvc();

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            //services.Configure<ForwardedHeadersOptions>(options =>
            //{
            //    options.ForwardedHeaders =
            //        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            //    options.KnownNetworks.Clear();
            //    options.KnownProxies.Clear();
            //});

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                // �鿴�����ĵ�
                if (Configuration["StartUp:IsOnline"].ObjToBool())
                {
                    options.IssuerUri = Configuration["StartUp:OnlinePath"].ObjToString();
                }
                options.UserInteraction = new IdentityServer4.Configuration.UserInteractionOptions
                {
                    LoginUrl = "/oauth2/authorize",//��¼��ַ
                };
            })

                // �Զ�����֤�����Բ���Identity
                //.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
                .AddExtensionGrantValidator<WeiXinOpenGrantValidator>()

                // ���ݿ�ģʽ
                .AddAspNetIdentity<ApplicationUser>()

                // ����������ݣ��ͻ��� �� ��Դ��
                .AddConfigurationStore(options =>
                {
                    if (isMysql)
                    {
                        options.ConfigureDbContext = b => b.UseMySql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                    }
                    else
                    {
                        options.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                    }
                })
                // ��Ӳ������� (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    if (isMysql)
                    {
                        options.ConfigureDbContext = b => b.UseMySql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                    }
                    else
                    {
                        options.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                    }

                    // �Զ����� token ����ѡ
                    options.EnableTokenCleanup = true;
                    // options.TokenCleanupInterval = 15; // frequency in seconds to cleanup stale grants. 15 is useful during debugging
                });

            builder.AddDeveloperSigningCredential();
            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.Requirements.Add(new ClaimRequirement("rolename", "Admin")));
                options.AddPolicy("SuperAdmin", policy => policy.Requirements.Add(new ClaimRequirement("rolename", "SuperAdmin")));
            });

            services.AddSingleton<IAuthorizationHandler, ClaimsRequirementHandler>();

            services.AddIpPolicyRateLimitSetup(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (ctx, next) =>
            {
                if (Configuration["StartUp:IsOnline"].ObjToBool())
                {
                    ctx.SetIdentityServerOrigin(Configuration["StartUp:OnlinePath"].ObjToString());
                }
                await next();
            });

            app.UseIpLimitMildd();

            //app.UseForwardedHeaders();
            app.UseCookiePolicy();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseSession();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseIdentityServer();

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