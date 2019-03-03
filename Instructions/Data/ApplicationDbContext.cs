using System;
using System.Collections.Generic;
using System.Text;
using Instructions.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Instructions.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<Record> Records { get; set; }
        public DbSet<Step> Steps { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Image> Images { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
