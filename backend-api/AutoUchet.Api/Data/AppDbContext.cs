using AutoUchet.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoUchet.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Activity> Activities { get; set; }
    }
}
