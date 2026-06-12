using Culinary_Assistant.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Culinary_Assistant.Services
{
    public class RecipeLocalizationService
    {
        private YandexTranslateService _translator =
            new YandexTranslateService();

        public async Task<Recipe> EnsureRussianAsync(Recipe recipe)
        {
            if (recipe == null)
                return null;

            if (string.IsNullOrWhiteSpace(recipe.TitleRu))
            {
                recipe.TitleRu =
                    await _translator.TranslateText(
                        recipe.Title ?? ""
                    );
            }

            if (string.IsNullOrWhiteSpace(recipe.IngredientsRu))
            {
                recipe.IngredientsRu =
                    await _translator.TranslateText(
                        recipe.Ingredients ?? ""
                    );
            }

            if (recipe.InstructionsRu == null ||
                recipe.InstructionsRu.Count == 0)
            {
                if (recipe.Instructions != null &&
                    recipe.Instructions.Count > 0)
                {
                    recipe.InstructionsRu =
                        await _translator.TranslateTextsTo(
                            "ru",
                            recipe.Instructions
                        );
                }
                else
                {
                    recipe.InstructionsRu =
                        new List<string>();
                }
            }
    
            return recipe;
        }
    }
}
