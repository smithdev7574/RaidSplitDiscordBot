using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using RaidRobot.Data;
using RaidRobot.Data.Interfaces;
using RaidRobot.Infrastructure;
using RaidRobot.Logic;
using RaidRobot.Messaging;
using RaidRobot.Users;
using System;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot
{
    class Program
    {
        private Configuration config;
        private DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;
        static void Main(string[] args) => new Program().start().GetAwaiter().GetResult();

        private async Task start()
        {
            config = new Configuration();
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            });
            commands = new CommandService();

            configureServices();

            client.Log += handleLog;
            await registerCommands();


            await client.LoginAsync(TokenType.Bot, config.Settings.Token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private void configureServices()
        {
            var serviceCollection = new ServiceCollection()
                .AddSingleton<IRaidSplitConfiguration>(config)
                .AddSingleton(client)
                .AddSingleton(commands)
                .AddScoped<IPermissionChecker, PermissionChecker>()
                .AddSingleton<ISplitDataStore, SplitDataStore>()
                .AddScoped<ITextCommunicator, TextCommunicator>()
                .AddScoped<IEventCreator, EventCreator>();

            services = serviceCollection.BuildServiceProvider();
        }

        private async Task registerCommands()
        {
            client.MessageReceived += HandleCommand;
            await commands.AddModuleAsync<Commands>(services);
        }

        private async Task HandleCommand(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(client, message);
            if (message.Author.IsBot)
                return;

            int y = 0;
            var x = message.HasStringPrefix("!rs", ref y);

            int argPos = 0;
            if (message.HasStringPrefix($"{config.Settings.MessagePrefix.ToUpper()} ", ref argPos) || message.HasStringPrefix($"{config.Settings.MessagePrefix.ToLower()} ", ref argPos))
            {
                try
                {
                    var result = await commands.ExecuteAsync(context, argPos, services);
                    if (!result.IsSuccess)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("I am sorry, I don't understand your request.");
                        sb.AppendLine($"Remember, you can always type {config.Settings.MessagePrefix} help for a list of commands.");
                        sb.AppendLine($"Here is the error message and reason incase it helps. {result.Error} {result.ErrorReason}");
                        await context.User.SendMessageAsync(sb.ToString());
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        await context.Channel.SendMessageAsync($"Something bad happened. {ex.Message}");
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine($"{DateTime.Now} - Exception: {ex2.Message}");
                    }
                    finally
                    {
                        Console.WriteLine($"{DateTime.Now} - Exception: {ex.Message}");
                    }
                }
            }
        }

        private Task handleLog(LogMessage arg)
        {
            Console.WriteLine($"{DateTime.Now} - Log Message: {arg}");
            return Task.CompletedTask;
        }
    }
}
