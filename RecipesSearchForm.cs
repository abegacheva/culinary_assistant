using Culinary_Assistant.Helpers;
using Culinary_Assistant.Models;
using Culinary_Assistant.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Culinary_Assistant
{
    public partial class RecipesSearchForm : Form
    {
        private RecipeApiService _apiService;
        private YandexTranslateService _translator;
        public RecipesSearchForm()
        {
            InitializeComponent();

            _apiService = new RecipeApiService();
            _translator = new YandexTranslateService();

            UIStyles.ApplyFormStyle(this);

            WindowState = FormWindowState.Maximized;

            UIStyles.StyleButton(btnSearch);
            UIStyles.StyleButton(btn_Video);

            UIStyles.StyleTextBox(textBox1);
            UIStyles.StyleTextBox(textBox4);

            flowRecipes.AutoScroll = true;
            flowRecipes.WrapContents = true;

            ApplyLanguageUI();
            AppLanguage.OnLanguageChanged += ApplyLanguageUI;


        }

        private void ApplyLanguageUI()
        {
            if (AppLanguage.IsRussian)
            {
                btnSearch.Text = "Поиск";
                btn_Video.Text = "Поиск";

                label1.Text = "Введите рецепт";
                label5.Text = "Поиск видео";
            }
            else
            {
                btnSearch.Text = "Search";
                btn_Video.Text = "Search";

                label1.Text = "Enter recipe";
                label5.Text = "Video search";
            }
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            string input = textBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                CustomMessageBox.Show(
                    AppLanguage.IsRussian
                        ? "Введите название рецепта"
                        : "Enter recipe name"
                );

                return;
            }

            try
            {
                string query = input;

                if (AppLanguage.IsRussian)
                {
                    var translated =
                        await _translator.TranslateTextsTo("en", new List<string> { input });

                    query = translated.FirstOrDefault() ?? input;
                }

                var recipes = await _apiService.SearchRecipes(query);

                CustomMessageBox.Show("Найдено: " + recipes.Count + " рецептов");

                recipes = recipes.Take(5).ToList();

                var loc = new RecipeLocalizationService();

                for (int i = 0; i < recipes.Count; i++)
                {
                    recipes[i] = await loc.EnsureRussianAsync(recipes[i]);
                }

                flowRecipes.SuspendLayout();
                flowRecipes.Controls.Clear();

                foreach (var recipe in recipes)
                {
                    flowRecipes.Controls.Add(CreateRecipeCard(recipe));
                }

                flowRecipes.ResumeLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private Panel CreateRecipeCard(Recipe recipe)
        {
            Panel card = new Panel
            {
                Width = 270,
                Height = 260,
                BackColor = Color.White,
                Margin = new Padding(15),
                Cursor = Cursors.Hand
            };

            PictureBox pic = new PictureBox
            {
                Width = 250,
                Height = 140,
                Top = 10,
                Left = 10,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            string imgPath = RecipeImageHelper.GetImagePath(recipe.Title);

            if (System.IO.File.Exists(imgPath))
                pic.Image = Image.FromFile(imgPath);

            var lbl = new Label
            {
                Text = AppLanguage.IsRussian
                    ? (recipe.TitleRu ?? recipe.Title)
                    : recipe.Title,

                Top = 160,
                Left = 10,
                Width = 250,
                Height = 45,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = UIStyles.TextColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            void OpenRecipe(object sender, EventArgs e)
            {
                new RecipeDetailsForm(recipe).ShowDialog();
            }

            card.Click += OpenRecipe;
            pic.Click += OpenRecipe;
            lbl.Click += OpenRecipe;

            card.Controls.Add(pic);
            card.Controls.Add(lbl);

            Color normalColor = Color.White;
            Color hoverColor = Color.MistyRose;

            void SetHover(bool hover)
            {
                card.BackColor = hover ? hoverColor : normalColor;
            }

            card.MouseEnter += (s, e) => SetHover(true);
            card.MouseLeave += (s, e) => SetHover(false);

            return card;
        }

   
        private void btn_Video_Click(object sender, EventArgs e)
        {
            string query = textBox4.Text.Trim();

            if (string.IsNullOrWhiteSpace(query))
            {
                CustomMessageBox.Show(
                    AppLanguage.IsRussian
                        ? "Введите название рецепта"
                        : "Enter recipe name"
                );
                return;
            }

            new VideoForm(query).ShowDialog();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AppLanguage.OnLanguageChanged -= ApplyLanguageUI;
            base.OnFormClosed(e);
        }
    }
}
