namespace ProjectManagementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using ProjectManagementSystem.Models;

    public sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "ProjectManagementSystem.Models.ApplicationDbContext";
        }

        public void RunSeed(ApplicationDbContext context)
        {
            Seed(context);
        }

        protected override void Seed(ApplicationDbContext context)
        {
            // Seed Roles
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            string[] roles = new string[] { "Admin", "ProjectManager", "User" };

            foreach (var roleName in roles)
            {
                if (!roleManager.RoleExists(roleName))
                {
                    roleManager.Create(new IdentityRole(roleName));
                }
            }

            // Seed Default Department
            Department defaultDept = context.Departments.FirstOrDefault(d => d.Name == "Software Engineering");
            if (defaultDept == null)
            {
                defaultDept = new Department
                {
                    Name = "Software Engineering",
                    Description = "Primary Engineering & Software Development Team",
                    IsActive = true
                };
                context.Departments.Add(defaultDept);
                context.SaveChanges();
            }

            // Seed User Manager
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            userManager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // 1. Seed Admin User
            string adminEmail = "admin@pms.com";
            var adminUser = userManager.FindByEmail(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Admin",
                    DepartmentId = defaultDept.DepartmentId,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };
                var result = userManager.Create(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    userManager.AddToRole(adminUser.Id, "Admin");
                }
            }

            // 2. Seed Project Manager User
            string pmEmail = "pm@pms.com";
            var pmUser = userManager.FindByEmail(pmEmail);
            if (pmUser == null)
            {
                pmUser = new ApplicationUser
                {
                    UserName = pmEmail,
                    Email = pmEmail,
                    EmailConfirmed = true,
                    FirstName = "Sarah",
                    LastName = "Manager",
                    DepartmentId = defaultDept.DepartmentId,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };
                var result = userManager.Create(pmUser, "User@123456");
                if (result.Succeeded)
                {
                    userManager.AddToRole(pmUser.Id, "ProjectManager");
                }
            }

            // 3. Seed Normal User
            string userEmail = "user@pms.com";
            var normalUser = userManager.FindByEmail(userEmail);
            if (normalUser == null)
            {
                normalUser = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true,
                    FirstName = "John",
                    LastName = "Developer",
                    DepartmentId = defaultDept.DepartmentId,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };
                var result = userManager.Create(normalUser, "User@123456");
                if (result.Succeeded)
                {
                    userManager.AddToRole(normalUser.Id, "User");
                }
            }

            // Seed System Settings
            if (!context.SystemSettings.Any(s => s.SettingKey == "SystemName"))
            {
                context.SystemSettings.Add(new SystemSetting
                {
                    SettingKey = "SystemName",
                    SettingValue = "Project & Task Management System",
                    Description = "Global Application Title",
                    UpdatedDate = DateTime.Now
                });
            }

            if (!context.SystemSettings.Any(s => s.SettingKey == "EnableEmailNotifications"))
            {
                context.SystemSettings.Add(new SystemSetting
                {
                    SettingKey = "EnableEmailNotifications",
                    SettingValue = "True",
                    Description = "Toggle for automated email notifications",
                    UpdatedDate = DateTime.Now
                });
            }

            context.SaveChanges();
        }
    }
}
