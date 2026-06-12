using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Culinary_Assistant.Helpers
{
    public static class RecipeImageHelper
    {
        private static readonly Dictionary<string, string[]> Categories =
            new Dictionary<string, string[]>
            {
                ["dessert"] = new[] {
                "cake","cookie","dessert","pie","tart","cupcake","biscuit",
                "торт","десерт","пирог","печенье","кекс","маффин","сладкое"
            },

                ["ice_cream"] = new[] {
                "ice cream","icecream","gelato","sorbet",
                "мороженое","пломбир","сорбет","крем"
            },

                ["soup"] = new[] {
                "soup","broth","bisque",
                "суп","бульон","окрошка","щи","борщ"
            },

                ["salad"] = new[] {
                "salad","caesar","greek",
                "салат","оливье","винегрет"
            },

                ["drink"] = new[] {
                "juice","tea","coffee","cocktail","smoothie","drink","lemonade",
                "сок","чай","кофе","напиток","лимонад","компот","морс"
            },

                ["pasta"] = new[] {
                "pasta","spaghetti","noodle","macaroni","ramen","udon",
                "макароны","лапша","спагетти","вермишель"
            },

                ["pizza"] = new[] {
                "pizza","пицца"
            },

                ["steak"] = new[] {
                "steak","beef steak","ribeye",
                "стейк","рибай"
            },

                ["meat"] = new[] {
                "beef","chicken","pork","lamb","meat",
                "говядина","курица","свинина","баранина","мясо","фарш"
            },

                ["fish"] = new[] {
                "fish","salmon","tuna","trout","cod",
                "рыба","лосось","тунец","форель","щука","сельдь"
            },

                ["casserole"] = new[] {
                "casserole","bake","baked","gratin",
                "запеканка","запечён","запек"
            },

                ["vegetables"] = new[] {
                "vegetable","veggie","broccoli","carrot","tomato","cucumber","pepper",
                "овощ","овощи","помидор","огурец","морковь","перец","капуста","баклажан","кабачок"
            },

                ["fruit"] = new[] {
                "apple","banana","orange","pear","grape","berry","strawberry",
                "яблоко","банан","апельсин","груша","виноград","ягода","клубника","малина"
            }
            };

        private static readonly string BasePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Food");

        private static readonly Dictionary<string, Image> Cache =
            new Dictionary<string, Image>();

        public static Image GetImage(string title)
        {
            string category = Classify(title);
            string path = Path.Combine(BasePath, category + ".jpg");

            if (Cache.ContainsKey(path))
                return Cache[path];

            try
            {
                var img = Image.FromFile(path);
                Cache[path] = img;
                return img;
            }
            catch
            {
                return LoadFallback();
            }
        }

        public static string GetImagePath(string title)
        {
            string category = Classify(title);
            return Path.Combine(BasePath, category + ".jpg");
        }
      
        private static string Classify(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return "casserole";

            string text = Normalize(title);

            string bestCategory = "casserole";
            int bestScore = 0;

            foreach (var cat in Categories)
            {
                int score = 0;

                foreach (var keyword in cat.Value)
                {
                    if (text.Contains(keyword))
                        score += keyword.Length; 
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCategory = cat.Key;
                }
            }

            return bestCategory;
        }

        private static string Normalize(string text)
        {
            text = text.ToLowerInvariant();

            text = text.Replace("ё", "е");

            text = Regex.Replace(text, @"[^a-zа-я0-9\s]", " ");

            return text;
        }

        private static Image LoadFallback()
        {
            string path = Path.Combine(BasePath, "casserole.jpg");

            try
            {
                return Image.FromFile(path);
            }
            catch
            {
                return new Bitmap(1, 1);
            }
        }
    }
}
