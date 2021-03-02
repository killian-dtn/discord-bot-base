using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBotBase
{
    [Name("Basics")]
    public class BasicModule : ModuleBase<SocketCommandContext>
    {
        protected Random Rand { get; set; }
        protected CommandService Service { get; set; }

        public BasicModule(CommandService service) : base()
        {
            this.Rand = new Random();
            this.Service = service;
        }

        private Color RandomColor()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            foreach (string color in new List<string>() { "R", "G", "B" })
                dict.Add(color, this.Rand.Next(0, 256));

            return new Color(
                dict["R"],
                dict["G"],
                dict["B"]
            );
        }

        [Command("ping")]
        [Summary("Ping the bot.")]
        public async Task PingAsync()
        {
            await this.Context.Channel.SendMessageAsync("```pong```");
        }

        [Command("infos"), Alias("i", "inf")]
        [Summary("Show user characteristics.")]
        public async Task InfosAsync(IUser usr = null)
        {
            if (usr == null)
                usr = this.Context.Message.Author;

            EmbedBuilder embed = new EmbedBuilder
            {
                Title = usr.Username,
                ImageUrl = usr.GetAvatarUrl(),
                Color = this.RandomColor()
            };

            embed.AddField("Intelligence", $"{this.Rand.Next(0, 21)} / 20");
            embed.AddField("Force", $"{this.Rand.Next(0, 21)} / 20");
            embed.AddField("Défense", $"{this.Rand.Next(0, 21)} / 20");
            embed.AddField("Vitesse", $"{this.Rand.Next(0, 21)} / 20");

            await this.Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("help")]
        [Summary("Help users.")]
        public async Task HelpAsync()
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Title = "Help page",
                Color = this.RandomColor()
            };

            foreach (ModuleInfo module in this.Service.Modules)
            {
                string desc = "";

                foreach (CommandInfo command in module.Commands)
                {
                    PreconditionResult result = await command.CheckPreconditionsAsync(this.Context);
                    if (result.IsSuccess)
                        desc += $"{System.Configuration.ConfigurationManager.AppSettings["Prefix"]}{command.Name} [{string.Join(", ", command.Parameters)}] | Aliases : {string.Join(", ", command.Aliases)}\n";
                }

                if (!string.IsNullOrWhiteSpace(desc))
                    embed.AddField(module.Name, $"```{desc}```");
            }

            await this.Context.Message.Author.SendMessageAsync(null, false, embed.Build());
            await this.Context.Channel.SendMessageAsync($"{this.Context.Message.Author.Mention} maybe if you check your DM's you'll see a help page :rolling_eyes:.");
        }

        [Command("help")]
        [Summary("Help users.")]
        public async Task HelpAsync(string command)
        {
            SearchResult result = this.Service.Search(this.Context, command);

            if (!result.IsSuccess)
            {
                await this.Context.Channel.SendMessageAsync($"{this.Context.Message.Author.Mention} command \"{command}\" not found :japanese_goblin:.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder
            {
                Title = "Help page",
                Color = this.RandomColor()
            };

            foreach (CommandMatch cmdMatch in result.Commands)
            {
                embed.AddField(
                    $"{cmdMatch.Command.Name}",
                    $"```Aliases : {string.Join(", ", cmdMatch.Command.Aliases)}\n" +
                    $"Parameters : {string.Join(", ", cmdMatch.Command.Parameters)}\n" +
                    $"Summary : {cmdMatch.Command.Summary}```"
                );
            }

            await this.Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
