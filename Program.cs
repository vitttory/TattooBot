using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualBasic;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Diagnostics.Metrics;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using InputFile = Telegram.Bot.Types.InputFile;

class Program
{
    static async void Main(string[] args)
    {
        var botClient = new TelegramBotClient("5828952881:AAGhBHKR3OlyW27eFluslbsGXY_7LTvAyhE");
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };


        async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerErrorAsync, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"��� {botMe.Username} ����� ���������");
            Console.ReadKey();
        }

        async Task HandlerErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"������� � �������� ��� �ϲ:\n {apiRequestException.ErrorCode}" +
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
                await Update(botClient, update.Message);
            }
        }

        //client.StartReceiving(Update, Error);


        //var cts = new CancellationTokenSource();
        //var cancellationToken = cts.Token;
        //var receiverOptions = new ReceiverOptions
        //{
        //    AllowedUpdates = { }, // receive all update types
        //};

        //client.StartReceiving(
        //    Update,
        //    Error,
        //    receiverOptions,
        //    cancellationToken
        //);
        //await Update(BotClient, update, token);
        //Console.ReadKey();
    }

    async static Task Update(ITelegramBotClient BotClient, Update update, CancellationToken token)
    {
        var message = update.Message;
        if (message!=null && message.Text != null)
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

            if (tokens[0]=="/start")
            {
                await BotClient.SendTextMessageAsync(message.Chat.Id, "�����! ³��� ���� � ��� ��� ����������. � �������� ����� ������ ������ ������� ��� ����, �� ���� �����, ������ ����� �� ���������� ������������)");
                return;
            }
            else if (tokens[0]=="/healing")
            {
                await BotClient.SendTextMessageAsync(message.Chat.Id, "����� ����������, �� �� ������ ����������� ����� ������ �������, ��� ��� ����� ����� �� �������, �� � ������� ��� ���:\r\n1. ����� ���������� �������������� SPF-������, �� �������� ����� ��������� ���� �� ��������� ���������� �����������. � ������� � ������� ����, �� �������� �������� ���� ��������� �� �����.\r\n2. �� �������� � �������� ��������, �� ��������� ������ ������� ����� �� �� ����� �� ����.\r\n3. �� ������� ���� �� ��� ���������, ���� ������������ ������� ����������� �������, ��� ����� �������� ������.\r\n4. ������� ����������� ����� ���������� �������� �� � ���, �� ������� ��� ������ ������� �� ���� �������.\r\n5. �������� �� �������� ���������� ����.\r\n");
                return;
            }
           else if (tokens[0] == "/sketch_instruction")
           {
              await BotClient.SendTextMessageAsync(message.Chat.Id, "��� ��������� ������������� �� ����� ����� ����� �� ������ ������������ �������� /sketch. ��� ��� �������� ������ �� ���� �������������� ������ ������:\r\n1. ����� ���� �� ���� ������� ����������.\r\n2. ���� �� ���� ������� ���� ������� /sketch ����� �����. � ������� ���� ���������� �����/������� ����, �� ���� ��������������� ���� ������������� ������ ������. ��������� /sketch ***_*** (������� ��������� ��� ������ � ����� �������).");
              return;
           }

            if (tokens[0] == "/sketch" && tokens.Length != 1)
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

                    var masterInfo = $"³�: {age}, \n��'�: {name}, \n�����: {sex}";
                    mastersList.Add(masterInfo);
                }
                var textResponse = string.Join("\n\n", mastersList);
                await BotClient.SendTextMessageAsync(message.Chat.Id, textResponse);
                return;

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

                    var studiaInfo = $"�����: {name}, \n������� �� 1 �� 10: {rating}";
                    studiasList.Add(studiaInfo);
                }
                var textResponse = string.Join("\n\n", studiasList);
                await BotClient.SendTextMessageAsync(message.Chat.Id, textResponse);
                return;

            }
            else if (tokens[0] == "/create_masterfavlist")
            {
                var client = new HttpClient();
                var request = await client.GetAsync($"https://localhost:7068/Masters/PostFavMasters");
                var response = await request.Content.ReadAsStringAsync();
                await BotClient.SendTextMessageAsync(message.Chat.Id, "������ ���� ������� ����� ������� ������ �� ������ ������� � ������: �� ��'� �����", replyMarkup: new ForceReplyMarkup());

                return;
            }
            if (message.ReplyToMessage != null && message.ReplyToMessage.Text == "/create_masterfavlist")
            {
                var dataTokens = message.Text.Split(' ');
                if (dataTokens.Length < 3)
                {
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "����������� �����. ������ ��� ������� � ������: �� ��'� �����", replyMarkup: new ForceReplyMarkup());
                    return;
                }
                var Age = int.Parse(tokens[1]);
                var Sex = tokens.Last();
                string Name = "";
                for (int i = 2; i < tokens.Length - 1; i++)
                {
                    Name += tokens[i] + " ";
                }

                var values = new Dictionary<string, string>
                {
                    {"Age", Age.ToString() },
                    {"Name", Name },
                    {"Sex", Sex }
                };
                var client = new HttpClient();
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync($"https://localhost:7068/Masters/PostFavMasters", content);

                var responseString = await response.Content.ReadAsStringAsync(); 

                if (response.IsSuccessStatusCode)
                {
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "��� ������� ������ ������ �� ������ �������.");
                }
                else
                {
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "��������� ������ �� ��������");
                }
                return;
            }

           

            else if (tokens[0] == "/delete_masterfavlist")
            {
                await BotClient.SendTextMessageAsync(message.Chat.Id, "������ ��� �������, ����� ������� ��������, � ������: �� ��'� �����", replyMarkup: new ForceReplyMarkup());
                return;
            }

            if (message.ReplyToMessage != null && message.ReplyToMessage.Text == "/delete_masterfavlist")
            {
                var client = new HttpClient();
                var dataTokens = message.Text.Split(' ');

                if (dataTokens.Length < 3)
                {
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "����������� �����. ������ ��� ������� � ������: �� ��'� �����", replyMarkup: new ForceReplyMarkup());
                    return;
                }

                var Age = int.Parse(dataTokens[0]);
                var Sex = dataTokens.Last();
                string Name = string.Join(' ', dataTokens.Skip(1).Take(dataTokens.Length - 2));

                var url = $"https://localhost:7068/Masters/DeleteFavMaster/{Age}{Name}{Sex}";
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "��� ������� ������ �������� � ������ �������.");
                }
                else
                {
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "��������� ����� �� ��������.");
                }
                return;
            }


            //else if (tokens[0] == "/delete_masterfavlist")
            //{
            //    var client = new HttpClient();
            //    var Age = int.Parse(tokens[1]);
            //    var Sex = tokens.Last();
            //    string Name = "";
            //    for (int i = 2; i < tokens.Length; i++)
            //    {
            //        Name += tokens[i] + " ";
            //    }

            //    var values = new Dictionary<string, string>
            //    {
            //        {"Age", Age.ToString() },
            //        {"Name", Name },
            //        {"Sex", Sex }
            //    };
            //    var content = new FormUrlEncodedContent(values);
            //    var response = await client.DeleteAsync("https://localhost:7068/Masters/DeleteFavMasters");

            //    if (response.IsSuccessStatusCode)
            //    {
            //        await BotClient.SendTextMessageAsync(message.Chat.Id, "��� ������� ������ �������� � ������ �������.");
            //    }
            //    else
            //    {
            //        await BotClient.SendTextMessageAsync(message.Chat.Id, "��������� ����� �� ��������.");
            //    }
            //    return;
            //}



        }
        else
        {
            await BotClient.SendTextMessageAsync(message.Chat.Id, "����� �� ��� �� �������� ����� ��� ���� :) ��� �� ���� �������� �� ����. ��� �������� ������ �� ��������� ����� ��������, ���� �����, �������� �������� ������������.");
        }

    }


    private static async Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken arg3)
    {
        var error = exception.Message;
    }
}