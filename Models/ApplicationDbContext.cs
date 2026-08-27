using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ProjectManagementSystem.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<ProjectMember> ProjectMembers { get; set; }

        public DbSet<TaskItem> TaskItems { get; set; }

        public DbSet<TaskComment> TaskComments { get; set; }

        public DbSet<TaskFile> TaskFiles { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<EmailLog> EmailLogs { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<SystemSetting> SystemSettings { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Disable cascade delete on relationships to ApplicationUser to prevent SQL Server multiple cascade paths (Error 1785)
            modelBuilder.Entity<Project>()
                .HasRequired(p => p.ProjectManager)
                .WithMany()
                .HasForeignKey(p => p.ProjectManagerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Project>()
                .HasRequired(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TaskItem>()
                .HasRequired(t => t.AssignedToUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedToUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TaskItem>()
                .HasRequired(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProjectMember>()
                .HasRequired(pm => pm.User)
                .WithMany()
                .HasForeignKey(pm => pm.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TaskComment>()
                .HasRequired(tc => tc.User)
                .WithMany()
                .HasForeignKey(tc => tc.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TaskFile>()
                .HasRequired(tf => tf.UploadedByUser)
                .WithMany()
                .HasForeignKey(tf => tf.UploadedByUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Notification>()
                .HasRequired(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .WillCascadeOnDelete(false);
        }
    }
}