using Fractions;
using Recipes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;

namespace Factorio
{
    public class Calculator
    {
        
        private Recipe[] recipes;
        private readonly string oilProcess = "Basic Oil Processing";

        //Dictionary<PID, ratio>
        public Dictionary<int, double> GetMachineRatios(int productID)
        {
            var ingredients = GetTotalIngredients(productID);
            foreach (var i in ingredients)
            {
                Console.WriteLine(GetItemName(i.Key) + ": " + i.Value);
            }
            Dictionary<int, Fraction> originalRatios = GetBaseRatios(productID);

            Dictionary<int, double> machineRatios = new Dictionary<int, double>();

            //common denominator of the original ratios
            int commonDenominator = 1;

            List<int> denominators = new List<int>();

            foreach (KeyValuePair<int, Fraction> ratio in originalRatios)
            {
                if (!denominators.Contains((int)ratio.Value.Denominator) && ratio.Key != 0 && ratio.Key != 7)
                {
                    commonDenominator = commonDenominator * (int)ratio.Value.Denominator;
                    denominators.Add((int)ratio.Value.Denominator);
                }
            }
            Console.WriteLine("Common Denominator: " + commonDenominator);

            foreach (var ratio in originalRatios)
            {
                double quantity = GetTotalIngredients(productID)[ratio.Key];
                double machineSpeed = GetMachineSpeed(GetRID(ratio.Key));
                Fraction multiplier = new Fraction((BigInteger)(commonDenominator * quantity * 40),
                                                   (BigInteger)(int)(machineSpeed * 4));
                machineRatios.Add(ratio.Key, (double)(ratio.Value.Multiply(multiplier)));
            }

            foreach (var ratio in machineRatios)
            {
                if (ratio.Key == 7)
                {
                    machineRatios.Remove(ratio.Key);
                }
            }



            //greatest common factor
            double gcf = 1;
            for (int i = 300000; i > 0; i--)
            {
                bool flag2 = true;
                foreach (var ratio in machineRatios)
                {
                    if (ratio.Value % i != 0 && ratio.Key != 0 && ratio.Key != 7)
                    {
                        flag2 = false;
                    }
                }
                if (flag2)
                {
                    gcf = i;
                    i = 0;
                }
            }

            Console.WriteLine("GCF = " + gcf);
            Dictionary<int, double> machineRatiosSimplified = new Dictionary<int, double>();

            foreach (var ratio in machineRatios)
            {
                machineRatiosSimplified.Add(ratio.Key, machineRatios[ratio.Key] / gcf);
            }
            return machineRatiosSimplified;
        }

        //Dictionary<PID, quantity>
        private Dictionary<int, double> GetTotalIngredients(int productID)
        {
            Dictionary<int, double> finalIngredients = new Dictionary<int, double>();

            Recipe currentRecipe = GetRecipeFromPID(productID);

            int productIndex = GetProductIndexFromPID(productID);
            double productQuantity = currentRecipe.Products[productIndex].Quantity;
    
            finalIngredients.Add(productID, 1);

            foreach (var ing in currentRecipe.Ingredients)
            {
                Dictionary<int, double> lowerIngredients = GetTotalIngredients(ing.Item.ProductID);

                foreach (var pair in lowerIngredients)
                {
                    if(!finalIngredients.ContainsKey(pair.Key))
                    {
                        finalIngredients.Add(pair.Key, 0);
                    }
                    finalIngredients[pair.Key] += pair.Value * ing.Quantity / productQuantity;
                }
            }

            return finalIngredients;
        }   

        //Dictionary<PID, ratio>
        private Dictionary<int, Fraction> GetBaseRatios(int productID)
        {
            Dictionary<int, double> ingredients = GetTotalIngredients(productID);

            Dictionary<int, Fraction> ratios = new Dictionary<int, Fraction>();
            
            int[] productIDs = new int[ingredients.Count];

            int n = 0;
            foreach (KeyValuePair<int, double> pair in ingredients)
            {
                productIDs[n] = pair.Key;
                n++;
            }

            for (int i = 0; i < ingredients.Count; i++)
            {
                int recipeID = GetRID(productIDs[i]);
                ratios.Add(productIDs[i], GetBaseRatio(recipeID, productIDs[i]));
            }
            return ratios;
        }

        //ratio is seconds/item crafted
        private Fraction GetBaseRatio(int recipeID, int productID)
        {
            //base ratio = sec/craft
            int productIndex = GetProductIndexFromPID(productID);
            Recipe recipe = GetRecipeFromRID(recipeID);

            double numerator = recipe.CraftingTime;
            int denominator = recipe.Products[productIndex].Quantity;

            Fraction ratio = new Fraction((BigInteger)(numerator * 100) , denominator * 100);
            
            return ratio;
        }

        private Recipe GetRecipeFromPID(int productID)
        {
            int recipeID = GetRID(productID);

            //if product is petroleum gas
            if (productID == 24)
            {
                switch (oilProcess)
                {
                    case "Basic Oil Processing":
                        recipeID = 25;
                        break;
                    default:
                        break;
                }
            }
            return GetRecipeFromRID(recipeID);
        }

        private Recipe GetRecipeFromRID(int recipeID)
        {
            Recipe recipe = null;

            foreach (Recipe r in recipes)
            {
                if(r.RecipeID == recipeID)
                {
                    recipe = r;
                }
            }
            return recipe;
        }

        private int GetRID(int productID)
        {
            int recipeID = -1;
            
            foreach (Recipe r in recipes)
            {
                foreach (ProductQuantity p in r.Products)
                {
                    if (p.Item.ProductID.Equals(productID))
                    {
                        recipeID = r.RecipeID;
                    }
                }
            }
            if (productID == 24 || productID == 25 || productID == 26)
                return 25;
            return recipeID;
        }

        private int GetProductIndexFromPID(int productID)
        {
            Recipe currentRecipe = GetRecipeFromPID(productID);
            
            int productIndex = 0;
            bool found = false;
            foreach (var i in currentRecipe.Products)
            {
                if (i.Item.ProductID.Equals(productID) && !found)
                {
                    found = true;
                }
                if (!found)
                {
                    productIndex++;
                }
            }
            return productIndex;
        }

        public string GetItemName(int productID)
        {
            string[] itemDictionary = File.ReadAllLines(@"C:\Users\hudso\source\repos\Factorio\Item Dictionary.txt");
            string name = "X";
            foreach (string s in itemDictionary)
            {
                string[] entry = s.Split(',');
                if(int.Parse(entry[0]) == productID)
                {
                    name = entry[1];
                }
            }
            return name;
        }

        private string GetCraftingMethod(int recipeID)
        {
            return GetRecipeFromRID(recipeID).CraftingMethod;
        }

        private double GetMachineSpeed(int recipeID)
        {
            string craftingMethod = GetCraftingMethod(recipeID);
            switch (craftingMethod)
            {
                case "Assembling Machine":
                    return 1.25;
                case "Chemical Plant":
                    return 1;
                case "Drill":
                    return 0.5;
                case "Furnace":
                    return 2;
                case "Oil Refinery":
                    return 1;
                case "Pumpjack":
                    return 1;
                default: 
                    return 1;

            }
        }

        public void PopulateRecipes(string inputFile)
        {
            string input = File.ReadAllText(inputFile);
            recipes = JsonSerializer.Deserialize<Recipe[]>(input);
        }
    }
}
