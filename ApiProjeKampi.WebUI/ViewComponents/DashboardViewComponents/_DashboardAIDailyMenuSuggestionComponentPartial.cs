using ApiProjeKampi.WebUI.Dtos.AISuggestionDtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace ApiProjeKampi.WebUI.ViewComponents.DashboardViewComponents
{
    public class _DashboardAIDailyMenuSuggestionComponentPartial : ViewComponent
    {
        private const string OPENAI_API_KEY = "BURAYA_KEY_GELECEK";

        private readonly IHttpClientFactory _httpClientFactory;

        public _DashboardAIDailyMenuSuggestionComponentPartial(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://api.openai.com/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", OPENAI_API_KEY);

                string prompt = @"
                4 farklı dünya mutfağından tamamen rastgele günlük menü oluştur.

                ÖNEMLİ KURALLAR:
                - Mutlaka 4 FARKLI ülke mutfağı seç.
                - Daha önce seçtiğin mutfakları tekrar etme (iç mantığında çeşitlilik üret).
                - Popüler olmayan mutfaklardan da seçebilirsin (örneğin Peru, Tayland, Fas, İran, Kore, Şili, Portekiz, Endonezya, Lübnan vb.).
                - Ülkeleri HER SEFERİNDE FARKLI seç.
                - Tüm içerik TÜRKÇE olacak.
                - Ülke adını Türkçe yaz (ör: “Peru Mutfağı”).
                - ISO Country Code zorunlu (ör: PE, TH, MA, IR, KR vb.)
                - Örnek vermiyorum, tamamen özgün üret.
                - Cevap sadece geçerli JSON olsun.

                JSON formatı:
                [
                  {
                    ""Cuisine"": ""X Mutfağı"",
                    ""CountryCode"": ""XX"",
                    ""MenuTitle"": ""Günlük Menü"",
                    ""Items"": [
                      { ""Name"": ""Yemek 1"", ""Description"": ""Açıklama"", ""Price"": 100 },
                      { ""Name"": ""Yemek 2"", ""Description"": ""Açıklama"", ""Price"": 120 },
                      { ""Name"": ""Yemek 3"", ""Description"": ""Açıklama"", ""Price"": 90 },
                      { ""Name"": ""Yemek 4"", ""Description"": ""Açıklama"", ""Price"": 70 }
                    ]
                  }
                ]";

                var requestBody = new
                {
                    model = "gpt-4.1-mini",
                    messages = new[]
                    {
                        new { role = "system", content = "Sadece JSON üret." },
                        new { role = "user", content = prompt }
                    }
                };

                var jsonBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("v1/chat/completions", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                // API doğru dönmüş mü kontrol et
                dynamic obj = JsonConvert.DeserializeObject(responseJson);

                // Hata durumu (ör. invalid_key, rate_limit)
                if (obj?.error != null)
                {
                    throw new Exception("OpenAI API Error: " + (string)obj.error.message);
                }

                // Choices var mı?
                var choices = obj?.choices;
                if (choices == null || choices.Count == 0)
                    throw new Exception("OpenAI 'choices' alanını döndürmedi. Yanıt: " + responseJson);

                // message.content var mı?
                var msgContent = obj?.choices[0]?.message?.content;
                if (msgContent == null)
                    throw new Exception("OpenAI yanıtında message.content yok. Yanıt: " + responseJson);

                string aiContent = msgContent.ToString();

                // JSON menü listesine çevir
                List<MenuSuggestionDto> menus;
                try
                {
                    menus = JsonConvert.DeserializeObject<List<MenuSuggestionDto>>(aiContent);
                }
                catch (Exception ex)
                {
                    throw new Exception("OpenAI JSON parse edilemedi.\nAI Yanıtı:\n" + aiContent, ex);
                }

                return View(menus);
            }
            catch (Exception ex)
            {
                // Hata durumunda boş liste yolla ama ekrana hata yazmasın
                ViewBag.AIError = ex.Message;
                return View(new List<MenuSuggestionDto>());
            }
        }
    }
}
