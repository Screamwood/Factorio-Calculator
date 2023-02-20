using Factorio;
using System;

namespace Factorio
{
    class FactorioConsole
    {
        static void Main(string[] args)
        {
            Calculator calc = new Calculator();
            calc.PopulateRecipes(@"C:\Users\hudso\source\repos\Factorio\Recipes.json");
            
            foreach (var item in calc.GetMachineRatios(16))
            {
                Console.WriteLine(calc.GetItemName(item.Key) + " Machine Ratio: " + item.Value);
            }
            Console.WriteLine("\n\n\n\n");
            foreach (var item in calc.GetMachineRatios(16))
            {
                Console.WriteLine(calc.GetItemName(item.Key) + " Machine Ratio: " + item.Value);
            }

            

            calc.GetItemName(16);
        }
    }
}
