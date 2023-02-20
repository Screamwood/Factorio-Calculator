using Recipes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace JSON_Builder
{
    class JSONBuilder
    {
        private const string inputFile = @"..\..\..\..\..\Recipes Unformatted.txt";
        private const string outputJSON = @"..\..\..\..\..\Recipes.json";
        private const string outputDictionary = @"..\..\..\..\..\Item Dictionary.txt";

        private static Dictionary<string, int> productDictionary;

        static void Main(string[] args)
        {
            PopulateProductList(inputFile);

            string[] input = ReadFile(inputFile);

            Recipe[] recipes = new Recipe[input.Length];
             
            for (int n = 0; n < input.Length; n++)
            {
                string[] recipeData = input[n].Split(',');

                recipes[n] = new Recipe
                {
                    RecipeID = n,
                    CraftingMethod = recipeData[6],
                    CraftingTime = double.Parse(recipeData[7]),
                    Products = new List<ProductQuantity>(),
                    Ingredients = new List<ProductQuantity>()
                };

                for (int i = 0; i < 6; i += 2)
                {
                    if (recipeData[i] != "")
                    {
                        recipes[n].Products.Add(new ProductQuantity
                        {
                            Quantity = int.Parse(recipeData[i + 1]),
                            Item = new Product
                            {
                                Name = recipeData[i],
                                ProductID = productDictionary[recipeData[i]]
                            }
                        });
                    }
                }

                for (int i = 8; i < 24; i += 2)
                {
                    if (recipeData[i] != "")
                    {
                        recipes[n].Ingredients.Add(new ProductQuantity
                        {
                            Quantity = int.Parse(recipeData[i + 1]),
                            Item = new Product
                            {
                                Name = recipeData[i],
                                ProductID = productDictionary[recipeData[i]]
                            }
                        });
                    }
                }

            }


            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(recipes, options);
            Console.WriteLine(json);

            File.WriteAllText(outputJSON, json);



            string dictionary = "";
            foreach (var pair in productDictionary)
            {
                dictionary += String.Format(pair.Value + "," + pair.Key + "\n"); 
            }
            File.WriteAllText(outputDictionary, dictionary);

        }

        private static void PopulateProductList(string inputFile)
        {
            string[] input = ReadFile(inputFile);
            productDictionary = new Dictionary<string, int>();

            int nextID = 0;
            for (int i = 0; i < input.Length; i++)
            {
                string[] recipeData = input[i].Split(',');

                for (int j = 0; j < 3; j++)
                {
                    if (!recipeData[j * 2].Equals(""))
                    {
                        if (!productDictionary.ContainsKey(recipeData[j * 2]))
                        {
                            productDictionary.Add(recipeData[j * 2], nextID);
                            nextID++;
                        }
                    }
                }
            }
            foreach (var item in productDictionary)
            {
                Console.WriteLine(item.Value + " " + item.Key);
            }

        }

        private static string[] ReadFile(string fileName)
        {
            return File.ReadAllLines(fileName);
        }
    }
}
