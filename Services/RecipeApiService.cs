using Culinary_Assistant.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Culinary_Assistant.Services
{
    public class RecipeApiService
    {
        private static readonly HttpClient client =
            new HttpClient();

        private readonly string _apiKey;

        public RecipeApiService()
        {
            _apiKey = ConfigurationManager.AppSettings["ApiNinjasKey"];

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
        }

        public async Task<List<Recipe>> SearchRecipes(string query)
        {
            List<Recipe> result = new List<Recipe>();

            List<string> variants = new List<string>
            {
                query,
                query + " recipe",
                "best " + query,
                "easy " + query,
                "homemade " + query
            };

            foreach (var q in variants)
            {
                string url =
                    "https://api.api-ninjas.com/v1/recipe?query=" +
                    Uri.EscapeDataString(q);

                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(json))
                    continue;

                if (!json.TrimStart().StartsWith("["))
                    continue;

                JArray arr = JArray.Parse(json);

                foreach (var item in arr)
                {
                    try
                    {
                        string title = item["title"] != null ? item["title"].ToString() : "";
                        string ingredients = item["ingredients"] != null ? item["ingredients"].ToString() : "";

                        var recipe = new Recipe
                        {
                            Title = title,
                            Ingredients = ingredients,

                            RecipeKey = Recipe.GenerateKey(title, ingredients),

                            Instructions = ParseInstructions(
                                item["instructions"] != null ? item["instructions"].ToString() : ""
                            ),

                            Nutrition = new Nutrition
                            {
                                Calories = EstimateCalories(ingredients),
                                Protein = EstimateProtein(ingredients),
                                TotalCarbohydrates = EstimateCarbs(ingredients),
                                TotalFat = EstimateFat(ingredients)
                            }
                        };

                        result.Add(recipe);
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (result.Count >= 10)
                    break;
            }

            return result
                .GroupBy(x => x.RecipeKey)
                .Select(x => x.First())
                .Take(5)
                .ToList();
        }

        private string Normalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "";

            s = s.ToLower();

            s = s.Replace(",", " ")
                 .Replace(".", " ")
                 .Replace(";", " ");

            return s;
        }

        private double ExtractWeightFactor(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 1.0;

            text = text.ToLower();

            double factor = 1.0;

            // common kitchen units
            if (text.Contains("tb") || text.Contains("tbsp")) factor += 0.2;
            if (text.Contains("tsp")) factor += 0.1;
            if (text.Contains("oz")) factor += 0.5;
            if (text.Contains("cup") || text.Contains("c ")) factor += 0.8;
            if (text.Contains("ml")) factor += 0.1;
            if (text.Contains("l ")) factor += 1.5;
            if (text.Contains("kg")) factor += 2.0;
            if (text.Contains("g ")) factor += 0.5;
            if (text.Contains("qt")) factor += 1.2;
            if (text.Contains("cn") || text.Contains("can")) factor += 1.0;

            return Math.Max(1.0, factor);
        }

        private double EstimateCalories(string ingredients)
        {
            string s = Normalize(ingredients);
            double weight = ExtractWeightFactor(s);

            double baseValue = 140;

            double result =
                baseValue +

                Add(s, "chicken", 180) +
                Add(s, "beef", 220) +
                Add(s, "pork", 200) +
                Add(s, "fish", 160) +
                Add(s, "salmon", 200) +

                Add(s, "rice", 150) +
                Add(s, "pasta", 180) +
                Add(s, "bread", 140) +

                Add(s, "potato", 100) +
                Add(s, "carrot", 30) +
                Add(s, "onion", 25) +

                Add(s, "milk", 50) +
                Add(s, "cheese", 120) +
                Add(s, "cream", 100) +
                Add(s, "butter", 140) +

                Add(s, "oil", 120) +
                Add(s, "sugar", 80) +
                Add(s, "egg", 70);

            result *= weight;

            return Clamp(result, 120, 1200);
        }

        private double EstimateProtein(string ingredients)
        {
            string s = Normalize(ingredients);
            double weight = ExtractWeightFactor(s);

            double result =
                4 +
                Add(s, "chicken", 22) +
                Add(s, "beef", 20) +
                Add(s, "pork", 18) +
                Add(s, "fish", 18) +
                Add(s, "egg", 6) +
                Add(s, "milk", 3) +
                Add(s, "cheese", 8);

            result *= weight;

            return Clamp(result, 2, 80);
        }

        private double EstimateCarbs(string ingredients)
        {
            string s = Normalize(ingredients);
            double weight = ExtractWeightFactor(s);

            double result =
                6 +
                Add(s, "rice", 35) +
                Add(s, "pasta", 40) +
                Add(s, "bread", 30) +
                Add(s, "potato", 25) +
                Add(s, "banana", 20) +
                Add(s, "sugar", 20);

            result *= weight;

            return Clamp(result, 5, 150);
        }

        private double EstimateFat(string ingredients)
        {
            string s = Normalize(ingredients);
            double weight = ExtractWeightFactor(s);

            double result =
                3 +
                Add(s, "oil", 18) +
                Add(s, "butter", 20) +
                Add(s, "cream", 15) +
                Add(s, "cheese", 12) +
                Add(s, "pork", 12);

            result *= weight;

            return Clamp(result, 2, 120);
        }

        private double Add(string s, string key, double value)
        {
            return s.Contains(key) ? value : 0;
        }

        private double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return Math.Round(value, 1);
        }

        private List<string> ParseInstructions(string raw)
        {
            List<string> list = new List<string>();

            if (string.IsNullOrWhiteSpace(raw))
                return list;

            string[] parts = raw.Split('\n');

            for (int i = 0; i < parts.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(parts[i]))
                    list.Add(parts[i].Trim());
            }

            return list;
        }
    }
}
