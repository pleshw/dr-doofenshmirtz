using System.Security.Cryptography;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

[Group("doofens")]
public class MainModule : ModuleBase<SocketCommandContext>
{
    [Group("roletar")]
    public class RollModule : ModuleBase<SocketCommandContext>
    {
        [Command("agente")]
        [Summary("Mostra um agente aleatório")]
        public async Task RollAgentAsync()
        {
            var agent = ValorantRoll.RollAgent();

            var embedBuilder = await GetAgentEmbed(agent);

            await Context.Channel.SendMessageAsync("", false, embedBuilder.Build());
        }

        [Command("mapa")]
        [Summary("Indica um mapa aleatório")]
        public Task RollMapAsync()
        {
            return ReplyAsync($"{ValorantRoll.RollMap().Name}");
        }

        [Command("time")]
        [Summary("Mostra 5 agentes aleatórios se o usuário não estiver em um chat de voz.\nCaso esteja em um chat de voz, mostra um agente para cada pessoa no chat, e indica quem deve pegar qual")]
        public async Task RollTeamAsync()
        {
            var listUsersFromChannel = GetListUsersInContextVoiceChannel(Context);
            if (listUsersFromChannel.Count > 0)
            {
                await DeleteAgentsStickers();

                var users = new Stack<string>(listUsersFromChannel);
                var agents = new Stack<ValorantEntity>(ValorantRoll.RollAgents(users.Count));
                while (agents.TryPop(out var agent))
                {
                    if (users.TryPop(out var username))
                    {
                        var embedBuilder = await GetAgentEmbed(agent, username);

                        await Context.Channel.SendMessageAsync("", false, embedBuilder.Build());
                    }
                }
            }
            else
            {
                var agents = ValorantRoll.RollTeam();

                for (int i = 0; i < agents.Count; ++i)
                {
                    var embedBuilder = await GetAgentEmbed(agents[i], $"{i + 1}");

                    await Context.Channel.SendMessageAsync("", false, embedBuilder.Build());
                }
            }
        }

        [Command("partida")]
        public async Task<IUserMessage> RollMatchAsync()
        {
            var match = ValorantRoll.RollMatch();

            List<string> team1 = match.Team1.Select(a => a.Name).ToList();
            List<string> team2 = match.Team2.Select(a => a.Name).ToList();
            List<string> maps = match.Maps.Select(a => a.Name).ToList();


            return await ReplyAsync(@$"
Time 1: {team1.Aggregate((acc, next) => string.IsNullOrEmpty(acc) ? next : acc + ", " + next)}.
Time 2: {team2.Aggregate((acc, next) => string.IsNullOrEmpty(acc) ? next : acc + ", " + next)}.
Mapas: {maps.Aggregate((acc, next) => string.IsNullOrEmpty(acc) ? next : acc + ", " + next)}.
                ");
        }

        public async Task DeleteAgentsStickers()
        {
            var agentNameList = RiotValorantContent.Instance.GetAgents().Select(a => a.Name).ToList();
            var agentStickers = Context.Guild.Stickers.Where(s => agentNameList.Contains(s.Name)).ToList();

            foreach (var sticker in agentStickers)
            {
                await sticker.DeleteAsync();
            }
        }

        public async Task<SocketCustomSticker> GetAgentSticker(ValorantEntity agent)
        {
            await DeleteAgentsStickers();

            Discord.Image img = new Discord.Image(@$"{agent.IconPath}");
            var agentSticker = Context.Guild.Stickers.Where(s => s.Name == agent.Name).FirstOrDefault();
            if (agentSticker == null)
            {
                await Context.Guild.CreateStickerAsync(agent.Name, "Agente: " + agent.Name, new List<string> { "Valorant", "Agente" }, img);

                return Context.Guild.Stickers.Where(s => s.Name == agent.Name).First();
            }
            else
            {
                return agentSticker;
            }
        }

        public async Task<EmbedBuilder> GetAgentEmbed(ValorantEntity agent, string username = "")
        {
            Discord.Image img = new Discord.Image(@$"{agent.IconPath}");
            var agentSticker = await GetAgentSticker(agent);
            string urlFromSticker = agentSticker.GetStickerUrl();

            // byte agent_color_a = (byte)(Convert.ToUInt32(agent.ColorScheme?[0].Substring(0, 2), 16));
            byte agent_color_r = (byte)(Convert.ToUInt32(agent.ColorScheme?[0].Substring(2, 2), 16));
            byte agent_color_g = (byte)(Convert.ToUInt32(agent.ColorScheme?[0].Substring(4, 2), 16));
            byte agent_color_b = (byte)(Convert.ToUInt32(agent.ColorScheme?[0].Substring(6, 2), 16));

            var embed = new EmbedBuilder
            {
                Color = new Color(agent_color_r, agent_color_g, agent_color_b),
                ImageUrl = urlFromSticker
            };

            if (!string.IsNullOrEmpty(username))
            {
                embed.AddField("Player", $"{username}", true);
            }

            embed.AddField("Agente", $"{agent.Name}", true).AddField("Role", agent.Role, true);

            if (agent.Tags != null && agent.Tags.Count > 0)
            {
                string htmlTagList = $"{agent.Tags.ToList().Aggregate((acc, next) => string.IsNullOrEmpty(acc) ? next : acc + ", " + next)}";
                embed.AddField("Pontos Fortes", htmlTagList, true);
            }

            return embed;
        }

        public List<string> GetListUsersInContextVoiceChannel(SocketCommandContext context)
        {
            var userChannelInterface = (context.User as IGuildUser)?.VoiceChannel;
            var userChannel = userChannelInterface as Discord.WebSocket.SocketVoiceChannel;
            if (userChannel != null)
            {
                return userChannel
                        .ConnectedUsers
                        .Select(u => u != null ? u.Username : "")
                        .Where(str => !string.IsNullOrEmpty(str))
                        .ToList();
            }
            else
            {
                return new List<string> { };
            }
        }
    }
}

