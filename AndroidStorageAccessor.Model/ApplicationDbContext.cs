using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AndroidStorageAccessor.Model
{
    public class ApplicationDbContext : DbContext
    {
        private string filePath;
        public ApplicationDbContext()
        {

        }
        public ApplicationDbContext(string filePath)
        {
            this.filePath = filePath;
        }


        public DbSet<FileItem> FileItems { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename= {filePath}");
           // base.OnConfiguring(optionsBuilder);
        } 
    }
}
