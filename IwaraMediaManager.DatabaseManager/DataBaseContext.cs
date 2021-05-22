using IwaraMediaManager.DatabaseManager;
using IwaraMediaManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IwaraMediaManager.DatabaseManager
{
    public class DataBaseContext : DbContext
    {
        public DbSet<Video> Videos { get; set; }
        public DbSet<Artist> Artists { get; set; }

        public DbSet<Setting> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=IwaraDatabase.sqlite");
        }
    }
}
