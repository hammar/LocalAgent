using System;
using Microsoft.EntityFrameworkCore;
using LocalAgent.ApiService.Models;

namespace LocalAgent.ApiService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Agent> Agents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Agent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd();
                entity.Property(e => e.Name)
                      .IsRequired();
                entity.Property(e => e.SystemInstructions)
                      .IsRequired();
            });
        }
    }
}
