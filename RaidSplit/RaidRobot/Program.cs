using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using RaidRobot.Data;
using RaidRobot.Infrastructure;
using RaidRobot.Logic;
using RaidRobot.Logic.Interfaces;
using RaidRobot.Messaging;
using RaidRobot.Messaging.Interfaces;
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
        private Logger logger;
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
            logger = new Logger(config);

            configureServices();

            client.Log += handleLog;
            await registerCommands();


            await client.LoginAsync(TokenType.Bot, config.Settings.Token);
            await client.StartAsync();

            var reactionMonitor = services.GetRequiredService<IReactionMonitor>();
            reactionMonitor.Initialize(); //Starts the watcher...

            var messageMonitor = services.GetRequiredService<IMessageMonitor>();
            messageMonitor.Initialize(); //Starts the watcher...

            await Task.Delay(-1);
        }

        private void configureServices()
        {
            var serviceCollection = new ServiceCollection()
                .AddSingleton<IRaidSplitConfiguration>(config) //Wrapper around appsettings.json to access the config data
                .AddSingleton(client) //The Discord.Net client that is maintaining the connection and interaction to discord
                .AddSingleton(commands) //A reference to the commands we wireup to accept command input from discord
                .AddSingleton<ILogger>(logger) //A Cheap logger to help troubleshoot some issues.
                .AddScoped<IEventCreator, EventCreator>() // Creates Events and Sets up the Initial Announcements
                .AddScoped<IEventOrchestrator, EventOrchestrator>() //Handles Interactions for a given Raid Event
                .AddScoped<IGuildMemberConverter, GuildMemberConverter>() //Helper to convert a Guild Member into an Attendee (probably could have been an extension method)
                .AddScoped<IMessageBuilder, MessageBuilder>() //Lots of string manipulation to build output for the Raid Events and Splits
                .AddSingleton<IMessageMonitor, MessageMonitor>() //Watches Incoming Messages for the raid split and responds NOTE: Not Commands
                .AddScoped<IPermissionChecker, PermissionChecker>() //Checks Discord Permissions
                .AddScoped<IPreSplitOrchestrator, PreSplitOrchestrator>() //Commands to manipulate the PreSplit Data
                .AddScoped<IRaidSplitter, RaidSplitter>() //Logic class to actually perform the Split Logic
                .AddSingleton<IRandomizer, Randomizer>() //Wrapper around .Net Random so we can keep a singleton around for all random numbers
                .AddSingleton<IReactionMonitor, ReactionMonitor>() //Watches and responds to reactions
                .AddScoped<IRegistrantLoader, RegistrantLoader>() //Interacts with discord to see who has registered
                .AddScoped<IRosterOrchestrator, RosterOrchestrator>() //Handles Interactions for the Roster Management
                .AddScoped<IRosterParser, RosterParser>() //Parses an EQ Guid Dump to create all the characters and classes
                .AddScoped<ISplitCommandInterpreter, SplitCommandInterpreter>() //Provides parsing to understand split commands and execute them
                .AddScoped<ISplitAuditor, SplitAuditor>() //Helps output a Split Audit so we can see the logic
                .AddSingleton<ISplitDataStore, SplitDataStore>() //Wrapper around a JsonFile with all our data so we can pretend its a database
                .AddScoped<ISplitOrchestrator, SplitOrchestrator>() //Handles Interactions for a given Split
                .AddScoped<ITextCommunicator, TextCommunicator>() //Wrapper to send messages in discord
                .AddSingleton<IUploadMonitor, UploadMonitor>(); //Monitors Discord for a file to be posted

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
                logger.DebugLog($"Received message with bot configuration: {message.Content}");
                try
                {
                    logger.DebugLog("Attempting to execute message command.");
                    var result = await commands.ExecuteAsync(context, argPos, services);
                    if (!result.IsSuccess)
                    {
                        logger.DebugLog($"Message was not successful: {result.Error}");
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("I am sorry, I don't understand your request.");
                        sb.AppendLine($"Remember, you can always type {config.Settings.MessagePrefix} help for a list of commands.");
                        sb.AppendLine($"Here is the error message and reason incase it helps. {result.Error} {result.ErrorReason}");
                        await context.User.SendMessageAsync(sb.ToString());
                    }
                    else
                    {
                        logger.DebugLog($"Message Command was succesful");
                    }
                }
                catch (Exception ex)
                {
                    logger.DebugLog($"Error Occured Processing message {ex.Message}");
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
