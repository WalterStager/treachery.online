﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
//using Treachery.Server.Migrations;

namespace Treachery.Server.Data;

public partial class TreacheryContext(DbContextOptions<TreacheryContext> options, IConfiguration configuration)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<PlayedGame> Games { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite(configuration.GetConnectionString("TreacheryDatabase"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
