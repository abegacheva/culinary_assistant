using Culinary_Assistant.Models;
using Culinary_Assistant.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Culinary_Assistant
{
    public partial class LikedRecipesForm : Form
    {
        private FlowLayoutPanel flow;
        private DatabaseHelper db;

        public LikedRecipesForm()
        {
            InitializeComponent();

            WindowState = FormWindowState.Maximized;

            UIStyles.ApplyFormStyle(this);

            db = new DatabaseHelper();

            flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                BackColor = UIStyles.BackgroundColor
            };

            Controls.Add(flow);

            Load += LikedRecipesForm_Load;
        }

        private async void LikedRecipesForm_Load(object sender, EventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            flow.SuspendLayout();
            flow.Controls.Clear();

            DataTable enTable = db.GetLikedRecipes();
            DataTable ruTable = db.GetLikedRecipesRu();

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
                        TotalCarbohydrates = ToDouble(r["carbs"])
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

                flow.Controls.Add(CreateCard(recipe));
            }

            flow.ResumeLayout();

            await Task.CompletedTask;
        }

        private Panel CreateCard(Recipe recipe)
        {
            Panel card = UIStyles.CreateCard();

            card.Width = 300;
            card.Height = 290;
            card.BackColor = Color.White;
            card.Cursor = Cursors.Hand;

            PictureBox pic = new PictureBox
            {
                Width = 280,
                Height = 150,
                Top = 10,
                Left = 10,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            string imgPath =
                Culinary_Assistant.Helpers.RecipeImageHelper.GetImagePath(recipe.Title);

            if (System.IO.File.Exists(imgPath))
            {
                using (var temp = Image.FromFile(imgPath))
                {
                    pic.Image = new Bitmap(temp);
                }
            }

            Label lbl = new Label
            {
                Text = AppLanguage.IsRussian
                    ? (recipe.TitleRu ?? recipe.Title)
                    : recipe.Title,

                Left = 10,
                Top = 170,
                Width = 280,
                Height = 45,

                ForeColor = UIStyles.TextColor,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Button btnLike = new Button
            {
                Text = "❤",
                Width = 42,
                Height = 42,
                Top = 220,
                Left = 240,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };

            btnLike.FlatAppearance.BorderSize = 0;

            btnLike.Click += (s, e) =>
            {
                db.RemoveLikedRecipe(recipe.RecipeKey);
                flow.Controls.Remove(card);
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

            double.TryParse(
                value.ToString(),
                out double result
            );

            return result;
        }
    }
}
