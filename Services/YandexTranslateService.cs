using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Culinary_Assistant.Services
{
    public class YandexTranslateService
    {
        private readonly string _apiKey;
        private readonly string _folderId;

        public YandexTranslateService()
        {
            _apiKey = ConfigurationManager.AppSettings["YandexApiKey"];
            _folderId = ConfigurationManager.AppSettings["YandexFolderId"];
        }

        public async Task<List<string>> TranslateTexts(List<string> texts)
        {
            if (texts == null || texts.Count == 0)
                return texts;

            using (var client = new HttpClient())
            {
                var body = new
                {
                    folderId = _folderId,
                    targetLanguageCode = "ru",
                    texts = texts
                };

                var json = JsonConvert.SerializeObject(body);

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://translate.api.cloud.yandex.net/translate/v2/translate"
                );

                request.Headers.Add("Authorization", $"Api-Key {_apiKey}");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Ошибка перевода:\n" + error);
                    return texts;
                }
                var responseJson = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject(responseJson);

                var translatedList = new List<string>();

                foreach (var item in result.translations)
                {
                    translatedList.Add((string)item.text);
                }

                return translatedList;

            }
        }

        public async Task<string> TranslateText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            using (var client = new HttpClient())
            {
                var body = new
                {
                    folderId = _folderId,
                    targetLanguageCode = "ru",
                    texts = new[] { text }
                };

                var json = JsonConvert.SerializeObject(body);

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://translate.api.cloud.yandex.net/translate/v2/translate"
                );

                request.Headers.Add("Authorization", $"Api-Key {_apiKey}");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Ошибка перевода:\n" + error);
                    return text;
                }

                var responseJson = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject(responseJson);

                return result.translations[0].text;
            }
        }

        public async Task<List<string>> TranslateTextsTo(string targetLang, List<string> texts)
        {
            if (texts == null || texts.Count == 0)
                return texts;

            using (var client = new HttpClient())
            {
                var body = new
                {
                    folderId = _folderId,
                    targetLanguageCode = targetLang,
                    texts = texts
                };

                var json = JsonConvert.SerializeObject(body);

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://translate.api.cloud.yandex.net/translate/v2/translate"
                );

                request.Headers.Add("Authorization", $"Api-Key {_apiKey}");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Ошибка перевода:\n" + error);
                    return texts;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseJson);

                var translatedList = new List<string>();

                foreach (var item in result.translations)
                {
                    translatedList.Add((string)item.text);
                }

                return translatedList;
            }
        }
    }
}
