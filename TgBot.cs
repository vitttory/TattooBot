using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Text.Json.Nodes;
using System.Text.Json;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json;
using System.Text;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Utilities.Encoders;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;
using System.Diagnostics.Eventing.Reader;

namespace TattooBot
{
    public class TgBot
    {
        public int index { get; set; }


        TelegramBotClient botClient = new TelegramBotClient("5828952881:AAGhBHKR3OlyW27eFluslbsGXY_7LTvAyhE");
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };


        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerErrorAsync, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"Бот {botMe.Username} почав працювати");
            Console.ReadKey();
        }

        async Task HandlerErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в телеграм бот АПІ:\n {apiRequestException.ErrorCode}" +
                $"\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            if (!cancellationToken.IsCancellationRequested)
            {
                try
                {

                    await Task.Delay(1000, cancellationToken);

                    botClient.StartReceiving(HandlerUpdateAsync, HandlerErrorAsync, receiverOptions, cancellationToken);
                }
                catch (Exception restartException)
                {
                    Console.WriteLine($"An error occurred while restarting the bot: {restartException.Message}");
                }
            }
        }

        async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                await Update(botClient, update);
            }
        }



        public async Task Update(ITelegramBotClient BotClient, Update update)
        {
            var message = update.Message;

            if (message != null && message.Text != null)
            {
                string[] tokens;
                if (message.Text.Contains(' '))
                {
                    tokens = message.Text.Split(' ');
                }
                else tokens = new string[]
                {
                message.Text
                };

                if (tokens[0] == "/start")
                {
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "Привіт! Вітаю тебе у боті про татуювання. У командах можеш обрати потрібні функції для себе, та будь ласка, зверни увагу на інструкцію використання)");
                }
                else if (tokens[0] == "/healing")
                {
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "Варто наголосити, що ви повинні дослухатися порад вашого майстра, але ось кілька порад та заборон, які є дійсними для усіх:\r\n1. Влітку обов’язково використовуйте SPF-захист, це зменшить ризик сонячного опіку та можливого подальшого травмування. У випадку зі загоєним тату, це допоможе зберегти його яскравість та якість.\r\n2. Не плавайте у відкритих водоймах, не приймайте довгих гарячих душів та не ходіть до саун.\r\n3. Не чухайте тату під час заживання, ліпше використайте вологий прохолодний компрес, щоб трохи зменшити свербіж.\r\n4. Знімайте загоювальну плівку попередньо змочивши її у воді, це зробить цей процес швидшим та менш болючим.\r\n5. Ретельно та обережно промивайте тату.\r\n");
                }
                else if (tokens[0] == "/sketch_instruction")
                {
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "Для отримання згенерованого за вашою темою ескізу ви можете скористатися функцією /sketch. Але для коректної роботи ви маєте притримуватися певних правил:\r\n1. Назва теми має бути введена англійською.\r\n2. Тема має бути введена після команди /sketch через пробіл. У випадку більш детального опису/обширної теми, ви маєте використовувати нижнє підкреслювання замість пробілу. Наприклад /sketch ***_*** (приклад наведений для запиту з двома словами).");

                }
                else if (tokens[0] == "/sketch" && tokens.Length != 1)
                {
                    tokens = tokens[1..];

                    var httpClient = new HttpClient();
                    var request = await httpClient.GetAsync($"https://localhost:7068/index/{tokens.Aggregate((x, y) => x + "_" + y)}");
                    var response = await request.Content.ReadAsStringAsync();
                    await BotClient.SendPhotoAsync(message.Chat.Id, InputFile.FromUri(JsonObject.Parse(response)["data"][0]["url"].ToString()));
                    return;
                }

                else if (tokens[0] == "/masters")
                {
                    var client = new HttpClient();
                    var request = await client.GetAsync($"https://localhost:7068/Masters/GetMastersList");
                    var response = await request.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(response);
                    var mastersArray = jsonDocument.RootElement.EnumerateArray();
                    var mastersList = new List<string>();

                    foreach (var master in mastersArray)
                    {

                        var age = master.GetProperty("age").GetInt32();
                        var name = master.GetProperty("name").GetString();
                        var sex = master.GetProperty("sex").GetString();

                        var masterInfo = $"Вік: {age}, \nІм'я: {name}, \nСтать: {sex}";
                        mastersList.Add(masterInfo);
                    }
                    var textResponse = string.Join("\n\n", mastersList);
                    await BotClient.SendTextMessageAsync(message.Chat.Id, textResponse);

                }

                else if (tokens[0] == "/studios")
                {
                    var client = new HttpClient();
                    var request = await client.GetAsync($"https://localhost:7068/GetStudiosList");
                    var response = await request.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(response);
                    var studiasArray = jsonDocument.RootElement.EnumerateArray();
                    var studiasList = new List<string>();

                    foreach (var studia in studiasArray)
                    {

                        var name = studia.GetProperty("name").GetString();
                        var rating = studia.GetProperty("rating").GetInt32();

                        var studiaInfo = $"Назва: {name}, \nРейтинг від 1 до 10: {rating}";
                        studiasList.Add(studiaInfo);
                    }
                    var textResponse = string.Join("\n\n", studiasList);
                    await BotClient.SendTextMessageAsync(message.Chat.Id, textResponse);


                }
                else if (tokens[0] == "/create_masterfavlist")
                {
                    index = 1;

                    await BotClient.SendTextMessageAsync(message.Chat.Id, "Введіть данні майстра якого хочетет додати до списку обраних у форматі: вік ім'я стать", replyMarkup: new ForceReplyMarkup());
                    return;

                }
                if (message.ReplyToMessage != null && index == 1)
                {
                    var dataTokens = message.Text.Split(' ');
                    if (dataTokens.Length < 3)
                    {
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Недостатньо даних. Введіть дані майстра у форматі: вік ім'я стать", replyMarkup: new ForceReplyMarkup());
                        return;

                    }
                    var Age = int.Parse(tokens[0]);
                    var Sex = tokens.Last();
                    string Name = " ";
                    for (int i = 1; i < tokens.Length - 1; i++)
                    {
                        Name += tokens[i] + " ";
                    }

                    var values = new
                    {
                        age = Age,
                        name = $"{Name}",
                        sex = $"{Sex}",
                    };

                    HttpResponseMessage httpresponse;
                    string Serialized = JsonConvert.SerializeObject(values);
                    using (var httpclient = new HttpClient())
                    {
                        httpclient.DefaultRequestHeaders.Accept.Clear();
                        httpclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpContent httpcontent = new StringContent(Serialized, Encoding.Unicode, "application/json");
                        httpresponse = await httpclient.PostAsync("https://localhost:7068/Masters/PostFavMasters", httpcontent);
                    }

                    var responseString = await httpresponse.Content.ReadAsStringAsync();

                    if (httpresponse.IsSuccessStatusCode)
                    {
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Дані майстра успішно додано до списку обраних.");
                    }
                    else
                    {
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Додавання данних не відбулося");
                    }
                    index = 0;
                    return;
                }

                else if (tokens[0] == "/favmasterslist")
                {

                    var client = new HttpClient();
                    var request = await client.GetAsync($"https://localhost:7068/Masters/GetFavMastersList");
                    var response = await request.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(response);
                    var mastersArray = jsonDocument.RootElement.EnumerateArray();
                    var mastersList = new List<string>();

                    foreach (var master in mastersArray)
                    {

                        var age = master.GetProperty("age").GetInt32();
                        var name = master.GetProperty("name").GetString();
                        var sex = master.GetProperty("sex").GetString();

                        var masterInfo = $"Вік: {age}, \nІм'я: {name}, \nСтать: {sex}";
                        mastersList.Add(masterInfo);
                    }
                    var textResponse = string.Join("\n\n", mastersList);
                    await BotClient.SendTextMessageAsync(message.Chat.Id, textResponse);
                    return;
                }

                else
                if (tokens[0] == "/delete_masterfavlist")
                {
                    index = 7;
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "Введіть данні майстра якого хочете видалити зі списку обраних у форматі: вік ім'я стать", replyMarkup: new ForceReplyMarkup());
                    return;
                }

                if (message.ReplyToMessage != null && index == 7)
                {
                    var dataTokens = message.Text.Split(' ');
                    if (dataTokens.Length < 3)
                    {
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Недостатньо даних. Введіть дані майстра у форматі: вік ім'я стать", replyMarkup: new ForceReplyMarkup());
                        return;
                    }
                    var Age = int.Parse(tokens[0]);
                    var Sex = tokens.Last();
                    string Name = " ";
                    for (int i = 1; i < tokens.Length - 1; i++)
                    {
                        Name += tokens[i] + " ";
                    }
                    var httpclient = new HttpClient();
                    var Url = $"https://localhost:7068/Masters/DeleteFavMaster?Age={Age}&Name={Name.Replace(" ", "%20")}&Sex={Sex}";

                    var request = new HttpRequestMessage(HttpMethod.Delete, Url);
                    var response = await httpclient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Дані майстра успішно видалено зі списку обраних.");
                    }
                    else
                    {
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Видалення даних не відбулося");
                    }
                    index = 0;
                    return;
                }

                else if (tokens[0] == "/create_studiosfavlist")
                {
                    index = 3;
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "Введіть данні студії яку хочете додати до списку обраних у форматі: рейтинг назва", replyMarkup: new ForceReplyMarkup());
                    return;

                }
                if (message.ReplyToMessage != null && index == 3)
                {
                    var StdataTokens = message.Text.Split(' ');
                    if (StdataTokens.Length < 2)
                    {
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Недостатньо даних. Введіть дані студії у форматі: рейтинг назва", replyMarkup: new ForceReplyMarkup());
                        return;

                    }

                    var Rating = int.Parse(tokens[0]);
                    string StudioName = " ";
                    for (int i = 1; i < tokens.Length; i++)
                    {
                        StudioName += tokens[i] + " ";
                    }

                    var stvalues = new
                    {
                        rating = Rating,
                        name = $"{StudioName}"

                    };

                    HttpResponseMessage Sthttpresponse;
                    string StSerialized = JsonConvert.SerializeObject(stvalues);
                    using (var Sthttpclient = new HttpClient())
                    {
                        Sthttpclient.DefaultRequestHeaders.Accept.Clear();
                        Sthttpclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpContent Sthttpcontent = new StringContent(StSerialized, Encoding.Unicode, "application/json");
                        Sthttpresponse = await Sthttpclient.PostAsync("https://localhost:7068/PostFavStudios", Sthttpcontent);
                    }

                    var StresponseString = await Sthttpresponse.Content.ReadAsStringAsync();

                    if (Sthttpresponse.IsSuccessStatusCode)
                    {
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Дані студії успішно додано до списку обраних.");
                    }
                    else
                    {
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Додавання данних не відбулося");
                    }
                    index = 0;
                    return;
                }
                else if (tokens[0] == "/favstudioslist")
                {
                    var client = new HttpClient();
                    var request = await client.GetAsync($"https://localhost:7068/GetFavStudios");
                    var response = await request.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(response);
                    var studiasArray = jsonDocument.RootElement.EnumerateArray();
                    var studiasList = new List<string>();

                    foreach (var studia in studiasArray)
                    {

                        var name = studia.GetProperty("name").GetString();
                        var rating = studia.GetProperty("rating").GetInt32();

                        var studiaInfo = $"Назва: {name}, \nРейтинг від 1 до 10: {rating}";
                        studiasList.Add(studiaInfo);
                    }
                    var textResponse = string.Join("\n\n", studiasList);
                    await BotClient.SendTextMessageAsync(message.Chat.Id, textResponse);

                }
                else if (tokens[0] == "/delete_studiofavlist")
                {
                    index = 4;
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "Введіть данні студії яку хочете додати до списку обраних у форматі: рейтинг назва", replyMarkup: new ForceReplyMarkup());
                    return;

                }
                if (message.ReplyToMessage != null && index == 4)
                {
                    var StdataTokens = message.Text.Split(' ');
                    if (StdataTokens.Length < 2)
                    {
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Недостатньо даних. Введіть дані студії у форматі: рейтинг назва", replyMarkup: new ForceReplyMarkup());
                        return;

                    }

                    var Rating = int.Parse(tokens[0]);
                    string StudioName = " ";
                    for (int i = 1; i < tokens.Length; i++)
                    {
                        StudioName += tokens[i] + " ";
                    }

                    var httpclient = new HttpClient();
                    var Url = $"https://localhost:7068/DeleteFavStudio?Rating={Rating}&Name={StudioName.Replace(" ", "%20")}";
                    var request = new HttpRequestMessage(HttpMethod.Delete, Url);
                    var response = await httpclient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Дані студії успішно видалено зі списку обраних.");
                    }
                    else
                    {
                        await BotClient.SendTextMessageAsync(message.Chat.Id, "Видалення даних не відбулося");
                    }
                    index = 0;
                    return;


                }

                return;

            }
            else
            {
                await BotClient.SendTextMessageAsync(message.Chat.Id, "Скоріш за все ви надіслали стікер або фото :) Бот не може обробити ці данні. Для коректної роботи та виконання ваших побажань, будь ласка, слідкуйте правилам користування.");
            }

        }


        static async Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken arg3)
        {
            var error = exception.Message;
        }
    }
}

