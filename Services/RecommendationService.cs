using Culinary_Assistant.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Culinary_Assistant.Services
{
    public class RecommendationService
    {
        private RecipeApiService api = new RecipeApiService();
        private DatabaseHelper db = new DatabaseHelper();

        private YandexTranslateService tr = new YandexTranslateService();

        private static readonly HashSet<string> IngredientDictionary = new HashSet<string>
        {
            "chicken","beef","pork","lamb","turkey","duck","bacon",
            "fish","salmon","tuna","shrimp","crab","lobster",
            "carrot","potato","tomato","onion","garlic","cucumber",
            "pepper","broccoli","spinach","lettuce","zucchini",
            "mushroom","corn","peas","beans",
            "apple","banana","orange","lemon","pear","grape",
            "mango","pineapple",
            "strawberry","blueberry","raspberry","blackberry",
            "milk","butter","cheese","cream","yogurt",
            "rice","pasta","flour","bread","oats",
            "egg","oil","sugar","salt"
        };

        public async Task GenerateRecommendations()
        {
            DataTable liked = db.GetLikedRecipes();

            if (liked.Rows.Count == 0)
                return;

            List<string> ingredients = new List<string>();

            foreach (DataRow row in liked.Rows)
            {
                ingredients.AddRange(
                    ExtractKnownIngredients(
                        row["ingredients"] == null
                            ? ""
                            : row["ingredients"].ToString()
                    )
                );
            }

            if (ingredients.Count == 0)
                ingredients.Add("chicken");

            List<string> top = ingredients
                .GroupBy(x => x)
                .OrderByDescending(x => x.Count())
                .Take(4)
                .Select(x => x.Key)
                .ToList();

            List<Recipe> recipes = new List<Recipe>();

            foreach (string ing in top)
            {
                List<Recipe> result = await api.SearchRecipes(ing);

                if (result != null)
                    recipes.AddRange(result);
            }

            List<Recipe> final = recipes
                .GroupBy(x => x.RecipeKey)
                .Select(x => x.First())
                .Take(10)
                .ToList();

            db.SaveRecommendations(final);

            foreach (Recipe r in final)
            {
                r.TitleRu = await tr.TranslateText(r.Title ?? "");

                r.IngredientsRu =
                    await tr.TranslateText(r.Ingredients ?? "");

                if (r.Instructions != null && r.Instructions.Count > 0)
                {
                    r.InstructionsRu =
                        await tr.TranslateTextsTo("ru", r.Instructions);
                }
                else
                {
                    r.InstructionsRu = new List<string>();
                }
            }

            db.SaveRecommendationsRu(final);
        }

        private List<string> ExtractKnownIngredients(string raw)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrWhiteSpace(raw))
                return result;

            string text = raw.ToLower();

            foreach (string ing in IngredientDictionary)
            {
                if (text.Contains(ing))
                    result.Add(ing);
            }

            return result;
        }
    }
}
