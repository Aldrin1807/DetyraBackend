﻿using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Confirmations> Confirmations { get; set; }
    }
}
