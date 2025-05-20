using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using project.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<adminAccount> Users { get; set; }
}