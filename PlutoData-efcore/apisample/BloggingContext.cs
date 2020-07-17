﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;


namespace apisample
{
    public class Blogging2Context : DbContext
    {
        public Blogging2Context(DbContextOptions<Blogging2Context> options)
            : base(options)
        {
        }

        public DbSet<Blog2> Blogs { get; set; }
        public DbSet<Post2> Posts { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

    }


    #region entitys

    public class Blog2
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }

        public List<Post2> Posts { get; set; }
    }

    public class Post2
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public List<Comment2> Comments { get; set; }
    }

    public class Comment2
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
    #endregion
}