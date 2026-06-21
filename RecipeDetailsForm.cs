using Culinary_Assistant.Models;
using Culinary_Assistant.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Culinary_Assistant.Helpers;
using NAudio.Wave;

namespace Culinary_Assistant
{
    public partial class RecipeDetailsForm : Form
    {
        private Recipe _recipe;
        private bool _isLiked;

        private readonly SpeechKitService _speechService =
            new SpeechKitService();

        private WaveOutEvent _waveOut;
        private Mp3FileReader _audioFile;

        private string _currentAudioPath;

        private Timer _speechTimer =
            new Timer();

        private bool _isDraggingTrackBar = false;

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

            _speechTimer.Interval = 500;
            _speechTimer.Tick += SpeechTimer_Tick;
        }

        private void StyleControls()
        {
            UIStyles.StyleButton(btnLike);

            UIStyles.StyleButton(btnBack10);
            UIStyles.StyleButton(btnForward10);

            UIStyles.StyleButton(btnPlaySpeech);
            UIStyles.StyleButton(btnPauseSpeech);
            UIStyles.StyleButton(btnStopSpeech);

            StyleTrackBar();
        }

        private void StyleTrackBar()
        {
            trackBarSpeech.BackColor = UIStyles.BackgroundColor;
            trackBarSpeech.TickStyle = TickStyle.None;

            trackBarSpeech.Minimum = 0;
            trackBarSpeech.Maximum = 100;
            trackBarSpeech.SmallChange = 1;
            trackBarSpeech.LargeChange = 5;
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

            pictureBox_RecipeImage.Image =
                File.Exists(imagePath)
                    ? Image.FromFile(imagePath)
                    : RecipeImageHelper.GetImage("fallback");

            label_Title.Text = ru
                ? (_recipe.TitleRu ?? _recipe.Title)
                : _recipe.Title;

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

            string ingredientsText =
                ru
                ? (_recipe.IngredientsRu ?? _recipe.Ingredients ?? "")
                : (_recipe.Ingredients ?? "");

            foreach (var i in ingredientsText.Split('|'))
            {
                if (!string.IsNullOrWhiteSpace(i))
                    listBox_Ingredients.Items.Add(i.Trim());
            }

            richTextBox_Instructions.Clear();

            var instructions =
                ru
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
            LikeChanged?.Invoke();
        }

        private void UpdateLikeButton()
        {
            btnLike.Text = _isLiked ? "❤️" : "🤍";
            btnLike.BackColor = _isLiked ? Color.IndianRed : Color.LightGray;
            btnLike.ForeColor = _isLiked ? Color.White : Color.Black;
        }

        private string GetInstructionsText()
        {
            bool ru = AppLanguage.IsRussian;

            var instructions =
                ru
                ? (_recipe.InstructionsRu ?? _recipe.Instructions)
                : _recipe.Instructions;

            if (instructions == null || instructions.Count == 0)
                return "";

            return string.Join(". ", instructions);
        }

        private async Task PrepareAudioAsync()
        {
            if (!string.IsNullOrEmpty(_currentAudioPath))
                return;

            string text = GetInstructionsText();

            _currentAudioPath =
                await _speechService.GenerateSpeechAsync(text);
        }

        private async void btnPlaySpeech_Click(object sender, EventArgs e)
        {
            try
            {
                if (_waveOut == null)
                {
                    await PrepareAudioAsync();

                    _audioFile = new Mp3FileReader(_currentAudioPath);

                    _waveOut = new WaveOutEvent();
                    _waveOut.Init(_audioFile);
                }

                _waveOut.Play();
                _speechTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка воспроизведения:\n" + ex.Message);
            }
        }

        private void btnPauseSpeech_Click(object sender, EventArgs e)
        {
            _waveOut?.Pause();
        }

        private void btnStopSpeech_Click(object sender, EventArgs e)
        {
            if (_waveOut == null)
                return;

            _waveOut.Stop();

            if (_audioFile != null)
                _audioFile.Seek(0, SeekOrigin.Begin);

            trackBarSpeech.Value = 0;
            lblSpeechTime.Text = "00:00 / 00:00";
        }

        private void SpeechTimer_Tick(object sender, EventArgs e)
        {
            if (_audioFile == null)
                return;

            double current = _audioFile.CurrentTime.TotalSeconds;
            double total = _audioFile.TotalTime.TotalSeconds;

            if (total <= 0)
                return;

            if (!_isDraggingTrackBar)
            {
                trackBarSpeech.Value =
                    Math.Min(100, (int)(current / total * 100));
            }

            lblSpeechTime.Text =
                $"{_audioFile.CurrentTime:mm\\:ss} / {_audioFile.TotalTime:mm\\:ss}";
        }

        private void trackBarSpeech_Scroll(object sender, EventArgs e)
        {
            if (_audioFile == null)
                return;

            double percent = trackBarSpeech.Value / 100.0;

            _audioFile.CurrentTime =
                TimeSpan.FromSeconds(
                    _audioFile.TotalTime.TotalSeconds * percent);
        }
        private void SeekToSecond(int second)
        {
            if (_audioFile == null) return;

            second = Math.Max(0, second);
            second = Math.Min(second, (int)_audioFile.TotalTime.TotalSeconds);

            _audioFile.CurrentTime = TimeSpan.FromSeconds(second);

            trackBarSpeech.Value =
                (int)((_audioFile.CurrentTime.TotalSeconds /
                      _audioFile.TotalTime.TotalSeconds) * 100);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _speechTimer?.Stop();

            _waveOut?.Stop();
            _waveOut?.Dispose();

            _audioFile?.Dispose();

            base.OnFormClosing(e);
        }

        private void btnBack10_Click(object sender, EventArgs e)
        {
            SeekToSecond((int)_audioFile.CurrentTime.TotalSeconds - 10);
        }

        private void btnForward10_Click(object sender, EventArgs e)
        {
            SeekToSecond((int)_audioFile.CurrentTime.TotalSeconds + 10);
        }
    }
}
