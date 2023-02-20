using System;
using System.Collections.Generic;

namespace Recipes
{
    public class Recipe
    {
        public int RecipeID { get; set; }
        public string CraftingMethod { get; set; }
        public double CraftingTime { get; set; }
        public List<ProductQuantity> Products { get; set; }
        public List<ProductQuantity> Ingredients { get; set; }
    }
}
