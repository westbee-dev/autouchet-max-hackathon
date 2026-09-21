using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetEnv;
using MAX.Bot;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request.Message;
using Autouchet_Bot.Keyboards;

namespace Autouchet_Bot;

class Bot
{
    static async Task Main(string[] args)
    {
        Env.Load();
        string botToken = Environment.GetEnvironmentVariable("API_KEY_MAX");
        string miniAppUrl = Environment.GetEnvironmentVariable("MINI_APP_URL");
        var client = new MaxBotClient("botToken");
       
}
}
