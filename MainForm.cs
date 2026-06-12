using Culinary_Assistant.Models;
using Culinary_Assistant.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Culinary_Assistant.Helpers;
using System.IO;
using System.Drawing.Drawing2D;

namespace Culinary_Assistant
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            WindowState = FormWindowState.Maximized;
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            UIStyles.ApplyFormStyle(this);

            flowRecipes.AutoScroll = true;
            flowRecipes.WrapContents = true;

            UIStyles.StyleButton(btnSearchRecipes);
            UIStyles.StyleButton(btnLiked);

            UIStyles.StyleButton(btnLanguage);

            btnLanguage.Text = AppLanguage.IsRussian
                ? "RU"
                : "EN";

            ApplyLanguageUI();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            
            LikeChangeTracker.OnThresholdReached += async () =>
            {
                var service = new RecommendationService();
                await service.GenerateRecommendations();

                await LoadRecommendations();

                MessageBox.Show(
                    AppLanguage.IsRussian
                        ? "Рекомендации обновлены"
                        : "Recommendations updated",
                    "Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            };
            ApplyLanguageUI();
            await LoadRecommendations();
        }

        private async Task LoadRecommendations()
        {
            flowRecipes.SuspendLayout();
            flowRecipes.Controls.Clear();

            var db = new DatabaseHelper();

            DataTable enTable = db.GetRecommendations();
            DataTable ruTable = db.GetRecommendationsRu();

            Dictionary<string, DataRow> ruMap =
                ruTable.AsEnumerable()
                .ToDictionary(
                    x => x["recipe_key"].ToString(),
                    x => x
                );

            foreach (DataRow r in enTable.Rows)
            {
                string key = r["recipe_key"].ToString();

                DataRow ruRow = null;

                if (ruMap.ContainsKey(key))
                    ruRow = ruMap[key];

                var recipe = new Recipe
                {
                    RecipeKey = key,

                    Title = r["title"]?.ToString(),
                    Ingredients = r["ingredients"]?.ToString(),

                    Instructions = SafeSplit(
                        r["instructions"]?.ToString()
                    ),

                    Nutrition = new Nutrition
                    {
                        Calories = ToDouble(r["calories"]),
                        Protein = ToDouble(r["protein"]),
                        TotalCarbohydrates = ToDouble(r["carbs"]),
                        TotalFat = ToDouble(r["fat"])
                    }
                };

                if (ruRow != null)
                {
                    recipe.TitleRu =
                        ruRow["title_ru"]?.ToString();

                    recipe.IngredientsRu =
                        ruRow["ingredients_ru"]?.ToString();

                    recipe.InstructionsRu = SafeSplit(
                        ruRow["instructions_ru"]?.ToString()
                    );
                }

                flowRecipes.Controls.Add(CreateRecipeCard(recipe));
            }

            flowRecipes.ResumeLayout();
            await Task.CompletedTask;
        }

        private Panel CreateRecipeCard(Recipe recipe)
        {
            Panel card = UIStyles.CreateCard();

            card.Width = 300;
            card.Height = 290;
            card.BackColor = Color.White;

            PictureBox pic = new PictureBox
            {
                Width = 280,
                Height = 150,
                Top = 10,
                Left = 10,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            string imagePath = RecipeImageHelper.GetImagePath(recipe.ImageKey);

            if (File.Exists(imagePath))
                pic.Image = Image.FromFile(imagePath);

            Label lbl = new Label
            {
                Text = AppLanguage.IsRussian
                    ? (recipe.TitleRu ?? recipe.Title)
                    : recipe.Title,

                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = UIStyles.TextColor,

                Width = 280,
                Height = 45,
                Top = 170,
                Left = 10,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Button btnLike = new Button
            {
                Width = 42,
                Height = 42,
                Top = 225,
                Left = 240,
                Text = "❤",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                Cursor = Cursors.Hand
            };

            btnLike.FlatAppearance.BorderSize = 0;

            string key = recipe.RecipeKey;
            var db = new DatabaseHelper();

            void UpdateLike()
            {
                bool liked = db.IsRecipeLiked(key);

                btnLike.BackColor = liked ? Color.IndianRed : Color.LightGray;
                btnLike.ForeColor = liked ? Color.White : Color.Black;
            }

            UpdateLike();

            btnLike.Click += (s, e) =>
            {
                if (db.IsRecipeLiked(key))
                    db.RemoveLikedRecipe(key);
                else
                    db.SaveLikedRecipe(
                        key,
                        recipe.Title,
                        recipe.Nutrition?.Calories ?? 0,
                        recipe.Nutrition?.Protein ?? 0,
                        recipe.Nutrition?.TotalCarbohydrates ?? 0,
                        string.Join("\n", recipe.Instructions ?? new List<string>()),
                        recipe.Ingredients ?? ""
                    );

                UpdateLike();
                LikeChangeTracker.RegisterChange();
            };

            void OpenRecipe(object sender, EventArgs e)
            {
                new RecipeDetailsForm(recipe).ShowDialog();
            }

            card.Click += OpenRecipe;
            lbl.Click += OpenRecipe;
            pic.Click += OpenRecipe;

            card.Controls.Add(pic);
            card.Controls.Add(lbl);
            card.Controls.Add(btnLike);

            Color normal = Color.White;
            Color hover = Color.MistyRose;

            int hoverCount = 0;

            void SetHover(bool enter)
            {
                hoverCount += enter ? 1 : -1;
                if (hoverCount < 0) hoverCount = 0;

                card.BackColor = hoverCount > 0 ? hover : normal;
            }

            void AddHover(Control c)
            {
                c.MouseEnter += (s, e) => SetHover(true);
                c.MouseLeave += (s, e) => SetHover(false);
            }

            AddHover(card);
            AddHover(pic);
            AddHover(lbl);

            btnLike.MouseEnter += (s, e) => SetHover(false);
            btnLike.MouseLeave += (s, e) => { };

            return card;
        }

        private List<string> SafeSplit(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<string>();

            return input
                .Split('\n')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        private double ToDouble(object value)
        {
            if (value == null)
                return 0;

            double.TryParse(value.ToString(), out double result);

            return result;
        }

        private void btnSearchRecipes_Click(object sender, EventArgs e)
        {
            new RecipesSearchForm().ShowDialog();
        }

        private async void btnLiked_Click(object sender, EventArgs e)
        {
            new LikedRecipesForm().ShowDialog();

            await LoadRecommendations();
        }

        private async void btnLanguage_Click(object sender, EventArgs e)
        {
            AppLanguage.Toggle();

            ApplyLanguageUI();

            await LoadRecommendations();
        }

        private void ApplyLanguageUI()
        {
            if (AppLanguage.IsRussian)
            {
                btnSearchRecipes.Text = "Поиск рецептов";
                btnLiked.Text = "Избранное";
                btnLanguage.Text = "RU";

                label1.Text = "Ваши рекомендации";
            }
            else
            {
                btnSearchRecipes.Text = "Search recipes";
                btnLiked.Text = "Liked";
                btnLanguage.Text = "EN";

                label1.Text = "Your recommendations";
            }
        }
    }
}
