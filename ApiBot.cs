using static System.Net.Mime.MediaTypeNames;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace TattooBot
{
    public class ApiBot
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly TelegramBotClient _botClient = new TelegramBotClient("5828952881:AAGhBHKR3OlyW27eFluslbsGXY_7LTvAyhE");

    }

}
