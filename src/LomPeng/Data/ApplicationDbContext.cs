using System;
using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LomPeng.Models;
using Microsoft.Extensions.DependencyInjection;


namespace LomPeng.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
       
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        public DbSet<Transcation> Transactions { get; set; }
        public DbSet<ChildAccount> ChildAccounts { get; set; }
        public DbSet<ChildAccountAdministrator> ChildAccountAdministrators { get; set; }
        public DbSet<UnknownUser> UnknownUsers { get; set; }

    }

    public class DatabaseSeeder
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            try
            {
                var context = serviceProvider.GetService<ApplicationDbContext>();
                context.Database.Migrate();
                string[] roles = new string[] { "Parent", "Child" };

                foreach (string role in roles)
                {
                    var roleStore = new RoleStore<IdentityRole>(context);

                    if (!context.Roles.Any(r => r.Name == role))
                    {
                        var result = roleStore.CreateAsync(new IdentityRole() { Name = role, NormalizedName = role.ToUpper() }).Result;
                    }
                }
                context.SaveChanges();
            }
            catch(Exception ex)
            {

            }
        }
        

    }
}