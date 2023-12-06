using System;
using System.Collections.Generic;

namespace Lab3.Models
{
    public partial class Ingredient
    {
        public Ingredient()
        {
            BreadRecipes = new HashSet<BreadRecipe>();
            Supplies = new HashSet<Supply>();
        }

        public int IngredientId { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public int? Quantity { get; set; }

        public virtual ICollection<BreadRecipe> BreadRecipes { get; set; }
        public virtual ICollection<Supply> Supplies { get; set; }
    }
}
