using System.Collections.Generic;

namespace Culinary_Assistant.Models
{
    public class Recipe
    {
        public string RecipeKey { get; set; }

        public string Title { get; set; }

        public string TitleRu { get; set; }

        public string Ingredients { get; set; }

        public string IngredientsRu { get; set; }

        public List<string> Instructions { get; set; } =
            new List<string>();

        public List<string> InstructionsRu { get; set; } =
            new List<string>();

        public Nutrition Nutrition { get; set; } =
            new Nutrition();

        public string ImageKey =>
            !string.IsNullOrWhiteSpace(Title)
                ? Title
                : (TitleRu ?? "");

        public static string GenerateKey(string title, string ingredients)
        {
            return $"{title}_{ingredients}".GetHashCode().ToString();
        }
    }
}
