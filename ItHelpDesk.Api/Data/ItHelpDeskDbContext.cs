using ItHelpDesk.Api.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ItHelpDesk.Api.Data
{
    public class ItHelpDeskDbContext : IdentityDbContext<ItHelpDeskUser>
    {
        public ItHelpDeskDbContext(DbContextOptions<ItHelpDeskDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketHistory> TicketHistory { get; set; }
        public DbSet<Session> Sessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ItHelpDeskUser>(o =>
            {
                o.Property(e => e.FullName).HasMaxLength(100);
            });

            modelBuilder.Entity<Session>(o =>
            {
                o.Property(e => e.LastRefreshTokenHash).HasMaxLength(64);
                o.Property(e => e.Device).HasMaxLength(256);

                o.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Category>(o =>
            {
                o.Property(e => e.Name).HasMaxLength(100);
                o.HasIndex(e => e.Name).IsUnique();
            });

            modelBuilder.Entity<Ticket>(o =>
            {
                o.Property(e => e.Title).HasMaxLength(200);
                o.Property(e => e.Description).HasMaxLength(4000);
                o.Property(e => e.ResolutionNote).HasMaxLength(2000);
                o.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsConcurrencyToken();
                o.Property(e => e.Priority).HasConversion<string>().HasMaxLength(20);

                o.HasIndex(e => e.Status);

                o.HasOne(e => e.Category)
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                o.HasOne(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                o.HasOne(e => e.AssignedTo)
                    .WithMany()
                    .HasForeignKey(e => e.AssignedToId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TicketComment>(o =>
            {
                o.Property(e => e.Text).HasMaxLength(4000);

                o.HasOne(e => e.Ticket)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                o.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TicketHistory>(o =>
            {
                o.Property(e => e.Action).HasMaxLength(50);
                o.Property(e => e.OldValue).HasMaxLength(200);
                o.Property(e => e.NewValue).HasMaxLength(200);

                o.HasOne(e => e.Ticket)
                    .WithMany(t => t.History)
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                o.HasOne(e => e.ChangedBy)
                    .WithMany()
                    .HasForeignKey(e => e.ChangedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}