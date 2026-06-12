using Culinary_Assistant.Models;
using Culinary_Assistant.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Culinary_Assistant.Helpers;
using System.IO;

namespace Culinary_Assistant
{
    public partial class RecipeDetailsForm : Form
    {
        private Recipe _recipe;
        private bool _isLiked;

        private DatabaseHelper db =
            new DatabaseHelper();

        public event Action LikeChanged;

        private string Key
        {
            get
            {
                return Recipe.GenerateKey(
                    _recipe.Title,
                    _recipe.Ingredients
                );
            }
        }

        public RecipeDetailsForm(Recipe recipe)
        {
            InitializeComponent();

            _recipe = recipe;

            WindowState = FormWindowState.Maximized;

            UIStyles.ApplyFormStyle(this);

            StyleControls();
        }

        private void StyleControls()
        {
            UIStyles.StyleButton(btnLike);
        }

        private async void RecipeDetailsForm_Load(object sender, EventArgs e)
        {
            if (AppLanguage.IsRussian)
            {
                var loc = new RecipeLocalizationService();
                _recipe = await loc.EnsureRussianAsync(_recipe);
            }

            bool ru = AppLanguage.IsRussian;

            string imagePath =
                RecipeImageHelper.GetImagePath(_recipe.ImageKey);

            if (File.Exists(imagePath))
            {
                pictureBox_RecipeImage.Image =
                    Image.FromFile(imagePath);
            }
            else
            {
                pictureBox_RecipeImage.Image =
                    RecipeImageHelper.GetImage("fallback");
            }

            label_Title.Text = ru
                ? (_recipe.TitleRu ?? _recipe.Title)
                : _recipe.Title;

            label_Title.ForeColor = UIStyles.TextColor;

            label_Calories.Text = ru
                ? "Калории: " + (_recipe.Nutrition?.Calories ?? 0)
                : "Calories: " + (_recipe.Nutrition?.Calories ?? 0);

            label_Protein.Text = ru
                ? "Белки: " + (_recipe.Nutrition?.Protein ?? 0)
                : "Protein: " + (_recipe.Nutrition?.Protein ?? 0);

            label_Carbs.Text = ru
                ? "Углеводы: " + (_recipe.Nutrition?.TotalCarbohydrates ?? 0)
                : "Carbs: " + (_recipe.Nutrition?.TotalCarbohydrates ?? 0);

            label_Fat.Text = ru
                ? "Жиры: " + (_recipe.Nutrition?.TotalFat ?? 0)
                : "Fat: " + (_recipe.Nutrition?.TotalFat ?? 0);

            listBox_Ingredients.Items.Clear();

            string ingredientsText = ru
                ? (_recipe.IngredientsRu ?? _recipe.Ingredients ?? "")
                : (_recipe.Ingredients ?? "");

            foreach (var i in ingredientsText.Split('|'))
            {
                if (!string.IsNullOrWhiteSpace(i))
                    listBox_Ingredients.Items.Add(i.Trim());
            }

            richTextBox_Instructions.Clear();

            var instructions = ru
                ? (_recipe.InstructionsRu ?? _recipe.Instructions)
                : _recipe.Instructions;

            if (instructions != null)
            {
                richTextBox_Instructions.Text =
                    string.Join("\n\n", instructions);
            }

            _isLiked = db.IsRecipeLiked(Key);
            UpdateLikeButton();
        }

        private void btnLike_Click(object sender, EventArgs e)
        {
            if (_isLiked)
            {
                db.RemoveLikedRecipe(Key);
                _isLiked = false;
            }
            else
            {
                db.SaveLikedRecipe(
                    Key,
                    _recipe.Title,
                    _recipe.Nutrition?.Calories ?? 0,
                    _recipe.Nutrition?.Protein ?? 0,
                    _recipe.Nutrition?.TotalCarbohydrates ?? 0,
                    string.Join("\n", _recipe.Instructions ?? new List<string>()),
                    _recipe.Ingredients ?? ""
                );

                db.SaveLikedRecipeRu(
                    Key,
                    _recipe.TitleRu ?? _recipe.Title,
                    string.Join("\n", _recipe.InstructionsRu ?? _recipe.Instructions ?? new List<string>()),
                    _recipe.IngredientsRu ?? _recipe.Ingredients ?? ""
                );

                _isLiked = true;
            }

            UpdateLikeButton();

            LikeChangeTracker.RegisterChange();

            LikeChanged?.Invoke();
        }

        private void UpdateLikeButton()
        {
            btnLike.Text =
                _isLiked ? "❤️" : "🤍";

            btnLike.BackColor =
                _isLiked
                ? Color.IndianRed
                : Color.LightGray;

            btnLike.ForeColor =
                _isLiked
                ? Color.White
                : Color.Black;
        }
    }
}
