using System;
using System.Collections.Generic;
using Lab3.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Lab3.Data
{
    public partial class BakeryDBContext : DbContext
    {

        public BakeryDBContext(DbContextOptions<BakeryDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BakeryProduct> BakeryProducts { get; set; } = null!;
        public virtual DbSet<BreadRecipe> BreadRecipes { get; set; } = null!;
        public virtual DbSet<Ingredient> Ingredients { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<Supply> Supplies { get; set; } = null!;
    }
}
