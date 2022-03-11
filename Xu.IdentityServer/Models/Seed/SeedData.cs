using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Xu.IdentityServer.Data;
using Xu.IdentityServer.Helper;
using Xu.IdentityServer.Models;

namespace Xu.IdentityServer
{
    public class SeedData
    {
        private static string SeedDataFolder = "dataJson/{0}.json";

        public static void EnsureSeedData(IServiceProvider serviceProvider, string WebRootPath)
        {
            /*
             * 本项目同时支持Mysql和Sqlserver，我一直使用的是Mysql，所以Mysql的迁移文件已经配置号，在Data文件夹下，
             * 直接执行update-database xxxx,那三步即可。如果你使用sqlserver，可以先从迁移开始，下边有步骤
             *
             * 当然你也可以都删掉，自己重新做迁移。
             * 迁移完成后，执行dotnet run /seed 进行同步数据
                1、PM> add-migration InitialIdentityServerPersistedGrantDbMigrationMysql -c PersistedGrantDbContext -o Data/MigrationsMySql/IdentityServer/PersistedGrantDb
                Build started...
                Build succeeded.
                To undo this action, use Remove-Migration.
                2、PM> update-database -c PersistedGrantDbContext
                Build started...
                Build succeeded.
                Applying migration '20200509165052_InitialIdentityServerPersistedGrantDbMigrationMysql'.
                Done.
                3、PM> add-migration InitialIdentityServerConfigurationDbMigrationMysql -c ConfigurationDbContext -o Data/MigrationsMySql/IdentityServer/ConfigurationDb
                Build started...
                Build succeeded.
                To undo this action, use Remove-Migration.
                4、PM> update-database -c ConfigurationDbContext
                Build started...
                Build succeeded.
                Applying migration '20200509165153_InitialIdentityServerConfigurationDbMigrationMysql'.
                Done.
                5、PM> add-migration AppDbMigration -c ApplicationDbContext -o Data/MigrationsMySql
                Build started...
                Build succeeded.
                To undo this action, use Remove-Migration.
                6、PM> update-database -c ApplicationDbContext
                Build started...
                Build succeeded.
                Applying migration '20200509165505_AppDbMigration'.
                Done.
             *
             */

            if (string.IsNullOrEmpty(WebRootPath))
            {
                throw new Exception("获取wwwroot路径时，异常！");
            }

            SeedDataFolder = Path.Combine(WebRootPath, SeedDataFolder);

            Console.WriteLine("Seeding database...");

            //获取服务
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                //迁移1 PersistedGrantDbContext上下文
                scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                {
                    //迁移2 ConfigurationDbContext上下文
                    var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                    context.Database.Migrate();

                    //Seed 配置数据，
                    EnsureSeedData(context);
                }

                {
                    //获取用户管理上下文服务
                    var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                    context.Database.Migrate();

                    //实例化
                    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

                    //远程获取Xu.Core数据库的二张表数据
                    var BlogCore_Users = JsonHelper.ParseFormByJson<List<User>>(FileHelper.ReadFile(string.Format(SeedDataFolder, "User"), Encoding.UTF8));
                    var BlogCore_Roles = JsonHelper.ParseFormByJson<List<Role>>(FileHelper.ReadFile(string.Format(SeedDataFolder, "Role"), Encoding.UTF8));

                    //Seed用户数据
                    foreach (var user in BlogCore_Users)
                    {
                        if (user == null || user.LoginName == null)
                        {
                            continue;
                        }
                        var roleIds = user.RoleIds.Split(",").ToList();
                        var userItem = userMgr.FindByNameAsync(user.LoginName).Result;
                        var roleName = BlogCore_Roles.FirstOrDefault(d => roleIds.Contains(d.Guid))?.RoleName;

                        if (userItem == null)
                        {
                            if (roleIds.Count > 0)
                            {
                                userItem = new ApplicationUser
                                {
                                    UserName = user.RealName,
                                    sex = user.Sex,
                                    birth = user.Birth,
                                    addr = user.Address,
                                    DeleteTime = user.DeleteTime,
                                    Email = user.LoginName + "@email.com",
                                    EmailConfirmed = true,
                                    LoginName = user.LoginName,
                                    RealName = user.RealName,
                                };

                                //var result = userMgr.CreateAsync(userItem, "BlogIdp123$" + item.uLoginPWD).Result;

                                // 因为导入的密码是 MD5密文，所以这里统一都用初始密码了,可以先登录，然后修改密码，超级管理员：blogadmin
                                var result = userMgr.CreateAsync(userItem, "Idp123$InitPwd").Result;
                                if (!result.Succeeded)
                                {
                                    throw new Exception(result.Errors.First().Description);
                                }

                                var claims = new List<Claim>{
                                    new Claim(JwtClaimTypes.Name, user.RealName),
                                    new Claim(JwtClaimTypes.Email, $"{user.LoginName}@email.com"),
                                    new Claim("rolename", roleName),
                                };

                                claims.AddRange(roleIds.Select(s => new Claim(JwtClaimTypes.Role, s.ToString())));

                                result = userMgr.AddClaimsAsync(userItem, claims).Result;

                                if (!result.Succeeded)
                                {
                                    throw new Exception(result.Errors.First().Description);
                                }
                                Console.WriteLine($"{userItem?.UserName} created");//AspNetUserClaims 表
                            }
                            else
                            {
                                Console.WriteLine($"{user?.LoginName} doesn't have a corresponding role.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"{userItem?.UserName} already exists");
                        }
                    }

                    foreach (var role in BlogCore_Roles)
                    {
                        if (role == null || role.RoleName == null)
                        {
                            continue;
                        }
                        var roleItem = roleMgr.FindByNameAsync(role.RoleName).Result;

                        if (roleItem != null)
                        {
                            role.RoleName = role.RoleName + Guid.NewGuid().ToString("N");
                        }

                        roleItem = new ApplicationRole
                        {
                            Description = role.Remark,
                            DeleteTime = role.DeleteTime,
                            CreateTime = role.CreateTime,
                            Enabled = role.Enabled,
                            Name = role.RoleName,
                        };

                        var result = roleMgr.CreateAsync(roleItem).Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }
                        Console.WriteLine($"{roleItem?.Name} created");//AspNetUserClaims 表
                    }
                }
            }

            Console.WriteLine("Done seeding database.");
            Console.WriteLine();
        }

        private static void EnsureSeedData(ConfigurationDbContext context)
        {
            if (!context.Clients.Any())
            {
                Console.WriteLine("Clients being populated");
                foreach (var client in Config.GetClients().ToList())
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("Clients already populated");
            }

            if (!context.IdentityResources.Any())
            {
                Console.WriteLine("IdentityResources being populated");
                foreach (var resource in Config.GetIdentityResources().ToList())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("IdentityResources already populated");
            }

            if (!context.ApiResources.Any())
            {
                Console.WriteLine("ApiResources being populated");
                foreach (var resource in Config.GetApiResources().ToList())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("ApiResources already populated");
            }

            if (!context.ApiScopes.Any())
            {
                Console.WriteLine("ApiScopes being populated");
                foreach (var resource in Config.GetApiScopes().ToList())
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("ApiScopes already populated");
            }
        }
    }
}