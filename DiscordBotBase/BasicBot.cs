using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordBotBase
{
    public class BasicBot
    {
        protected string Token { get; set; }
        protected DiscordSocketClient Client { get; set; }
        protected CommandService Commands { get; set; }

        public BasicBot(string token)
        {
            this.Token = token;
            this.Client = new DiscordSocketClient();
            this.Commands = new CommandService();

            this.Client.Log += this.Log;
            this.Client.MessageReceived += this.HandleCommandAsync;

            this.Commands.AddModulesAsync(assembly: Assembly.GetExecutingAssembly(), services: null);
        }

        /// <summary>
        /// Run the bot.
        /// </summary>
        /// <returns></returns>
        public virtual async Task StartAsync()
        {
            await this.Client.LoginAsync(
                TokenType.Bot,
                this.Token
            );
            await this.Client.StartAsync();
            await this.Client.SetGameAsync(
                $"Prefix : {System.Configuration.ConfigurationManager.AppSettings["Prefix"]}",
                null, ActivityType.Watching
            );
            await this.Client.SetStatusAsync(UserStatus.DoNotDisturb);

            await Task.Delay(-1);
        }

        /// <summary>
        /// Write logs in console.
        /// </summary>
        /// <param name="msg">Log message</param>
        /// <returns></returns>
        protected virtual Task Log(LogMessage msg)
        {
            Console.WriteLine($"{DateTime.Now} : {msg.Message}");
            return Task.CompletedTask;
        }

        protected virtual async Task HandleCommandAsync(SocketMessage msg)
        {
            SocketUserMessage message = (SocketUserMessage)msg;
            if (message == null) return;

            int argPos = 0;

            if (!(message.HasMentionPrefix(this.Client.CurrentUser, ref argPos) ||
                message.HasStringPrefix(System.Configuration.ConfigurationManager.AppSettings["Prefix"], ref argPos)
                ) || message.Author.IsBot) return;

            SocketCommandContext context = new SocketCommandContext(this.Client, message);

            IResult result = await this.Commands.ExecuteAsync(context, argPos, null);

            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
