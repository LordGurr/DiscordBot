using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using static DiscordBot.Bot;

namespace DiscordBot
{
    public class AdventureCommands : BaseCommandModule
    {
        /* Owner Commands */

        public static Bot bot;
        private DateTime shutdownTime;

        //private string SendString = "";

        private async Task WriteArrayEmbeed(string[] lines, CommandContext ctx, string title)
        {
            int latest = 0;
            int sent = 0;
            while (true)
            {
                string send = string.Empty;
                sent++;
                for (latest = latest; latest < lines.Length; latest++)
                {
                    string tempString = send;
                    tempString += lines[latest];
                    if (tempString.Length / 2000 >= 1.0)
                    {
                        break;
                    }
                    else
                    {
                        send = tempString + "\n";
                    }
                }
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = title + (sent == 0 && latest >= lines.Length ? "" : " part: " + sent),
                    Description = send,
                });
                if (latest >= lines.Length)
                {
                    break;
                }
            }
        }

        private async Task<List<DiscordUser>> AllUsersInMessage(string command, CommandContext ctx)
        {
            List<string> idlist = command.Split(' ', '\n').ToList();
            List<ulong> ids = new List<ulong>();
            for (int i = 0; i < idlist.Count; i++)
            {
                try
                {
                    if (!idlist[i].Contains('@') && IsDigitsOnly(idlist[i], string.Empty))
                    {
                        ids.Add(Convert.ToUInt64(idlist[i]));
                    }
                }
                catch (Exception)
                {
                    await ctx.Channel.SendMessageAsync(idlist[i] + " is not a valid ulong.");
                }
            }
            List<DiscordUser> usersMentioned = ctx.Message.MentionedUsers.ToList();
            for (int i = 0; i < ids.Count; i++)
            {
                try
                {
                    var user = bot.Client.GetUserAsync(ids[i]);
                    usersMentioned.Add(user.Result);
                    command = command.Replace(ids[i].ToString(), string.Empty);
                }
                catch (Exception e)
                {
                    await ctx.Channel.SendMessageAsync(idlist[i] + " is not a valid user id and sent the error: " + e.Message);
                }
            }
            return usersMentioned;
        }

        public static string TimespanToString(TimeSpan span)
        {
            if (span.TotalDays >= 7)
            {
                return span.Days / 7 + " veck" + (span.Days / 7 != 1 ? "or " : "a ") + span.Days % 7 + " dag" + (span.Days % 7 != 1 ? "ar " : " ") + span.Hours + " timm" + (span.Hours != 1 ? "ar " : "e ") + span.Minutes + " minut" + (span.Minutes != 1 ? "er" : "")/*, ctx*/;
            }
            else if (span.TotalDays >= 1)
            {
                return span.Days + " dag" + (span.Days != 1 ? "ar " : " ") + span.Hours + " timm" + (span.Hours != 1 ? "ar " : "e ") + span.Minutes + " minut" + (span.Minutes != 1 ? "er" : "")/*, ctx*/;
            }
            else if (span.TotalHours >= 1)
            {
                return span.Hours + " timm" + (span.Hours != 1 ? "ar " : "e ") + span.Minutes + " minut" + (span.Minutes != 1 ? "er" : "")/*, ctx*/;
            }
            else if (span.TotalMinutes >= 1)
            {
                return span.Minutes + " minut" + (span.Minutes != 1 ? "er " : " ") + (int)span.Seconds + " sekund" + (span.Seconds != 1 ? "er" : "")/*, ctx*/;
            }
            else
            {
                return (int)span.TotalSeconds + " sekund" + (span.Seconds != 1 ? "er" : "")/*, ctx*/;
            }
        }

        private static string TimespanToShortString(TimeSpan span)
        {
            if (span.TotalDays > 7)
            {
                return span.Days / 7 + ":" + span.Days % 7 + ":" + (span.Hours < 10 ? "0" : "") + span.Hours + ":" + (span.Minutes < 10 ? "0" : "") + span.Minutes/*, ctx*/;
            }
            else if (span.TotalDays >= 1)
            {
                return span.Days + ":" + (span.Hours < 10 ? "0" : "") + span.Hours + ":" + (span.Minutes < 10 ? "0" : "") + span.Minutes/*, ctx*/;
            }
            else
            {
                return span.Hours + ":" + (span.Minutes < 10 ? "0" : "") + span.Minutes/*, ctx*/;
            }
        }

        private static string TimespanToShortStringUnit(TimeSpan span)
        {
            if (span.TotalDays > 7)
            {
                return span.Days / 7 + "wk " + span.Days % 7 + "d " + (span.Hours < 10 ? "0" : "") + span.Hours + "hr " + (span.Minutes < 10 ? "0" : "") + span.Minutes + "m"/*, ctx*/;
            }
            else if (span.TotalDays >= 1)
            {
                return span.Days + "d " + (span.Hours < 10 ? "0" : "") + span.Hours + "hr " + (span.Minutes < 10 ? "0" : "") + span.Minutes + "m "/*, ctx*/;
            }
            else
            {
                return span.Hours + "hr " + (span.Minutes < 10 ? "0" : "") + span.Minutes + "m"/*, ctx*/;
            }
        }

        private string WriteLine(string str)
        {
            if (str == null || str == string.Empty)
            {
                return string.Empty;
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.White;
            //SendString += "\n" + str;
            return str + "\n";
        }

        private async Task WriteLine(string str, CommandContext ctx)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.White;
            await ctx.Channel.SendMessageAsync(str).ConfigureAwait(false);
            //if (SendString.Length > 0)
            //{
            //    SendString = "";
            //}
        }

        private async Task CommandWriteLine(string str, CommandContext ctx)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.White;
            await ctx.Channel.SendMessageAsync(str).ConfigureAwait(false);
            if (commandLine == null)
            {
                var g = bot.Client.GetChannelAsync(827869624808374293);
                commandLine = g.Result;
            }
            if (commandLine != ctx.Channel)
            {
                await commandLine.SendMessageAsync(str).ConfigureAwait(false);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("hierarchy")]
        [DSharpPlus.CommandsNext.Attributes.Description("Returns all members sorted by rank.")]
        public async Task WriteHierarchy(CommandContext ctx)
        {
            if (!bot.stopAll)
            {
                try
                {
                    int[] index = ChannelIndex(ctx);
                    if (index[0] >= 0)
                    {
                        List<DiscordMember> curMembers = new List<DiscordMember>();
                        for (int a = 0; a < kanalerna[index[0]].discordUsers.Count; a++)
                        {
                            curMembers.Add(kanalerna[index[0]].discordUsers[a].member);
                        }
                        curMembers = curMembers.OrderBy(o => o.Hierarchy).ToList();
                        curMembers.Reverse();
                        string SendString = string.Empty;
                        //await WriteLine("kanal " + (i + 1) + ": " + kanalerna[i].realDiscordChannel.Name);
                        if (kanalerna[index[0]].realDiscordChannel != null)
                        {
                            SendString += WriteLine(kanalerna[index[0]].realDiscordChannel.Name);
                        }
                        else
                        {
                            SendString += WriteLine("kanal");
                        }
                        string title = SendString;
                        SendString = string.Empty;
                        if (kanalerna[index[0]].discordUsers.Count < 1)
                        {
                            SendString += WriteLine(kanalerna[index[0]].membersToAdd.Length + " medlemmar som kommer läggas till så fort någon skriver något");
                        }
                        else
                        {
                            for (int a = 0; a < curMembers.Count; a++)
                            {
                                SendString += WriteLine("medlem " + (a + 1) + ": " + curMembers[a].Username + ". rank: " + curMembers[a].Hierarchy);
                                //await WriteLine(kanalerna[i].discordUsers[a].member.Username);
                            }
                        }
                        //await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        //{
                        //    Title = title,
                        //    Description = SendString,
                        //});
                        await WriteArrayEmbeed(SendString.Split('\n'), ctx, title);
                    }
                    else
                    {
                        await ctx.Channel.SendMessageAsync("this has to be sent in a channel").ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("bot shutting down").ConfigureAwait(false);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("members")]
        [DSharpPlus.CommandsNext.Attributes.Description("Returns all members stored.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task WriteMembers(CommandContext ctx)
        {
            if (!bot.stopAll)
            {
                try
                {
                    for (int i = 0; i < kanalerna.Count; i++)
                    {
                        string SendString = string.Empty;
                        //await WriteLine("kanal " + (i + 1) + ": " + kanalerna[i].realDiscordChannel.Name);
                        if (kanalerna[i].realDiscordChannel != null)
                        {
                            SendString += WriteLine(kanalerna[i].realDiscordChannel.Name);
                        }
                        else
                        {
                            SendString += WriteLine("kanal " + (i + 1));
                        }
                        string title = SendString;
                        SendString = string.Empty;
                        if (kanalerna[i].discordUsers.Count < 1)
                        {
                            SendString += WriteLine(kanalerna[i].membersToAdd.Length + " medlemmar som kommer läggas till så fort någon skriver något");
                        }
                        else
                        {
                            for (int a = 0; a < kanalerna[i].discordUsers.Count; a++)
                            {
                                SendString += WriteLine("medlem " + (a + 1) + ": " + kanalerna[i].discordUsers[a].member.Username);
                                //await WriteLine(kanalerna[i].discordUsers[a].member.Username);
                            }
                        }
                        //await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        //{
                        //    Title = title,
                        //    Description = SendString,
                        //});
                        await WriteArrayEmbeed(SendString.Split('\n'), ctx, title);
                    }
                }
                catch (Exception e)
                {
                    await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("bot shutting down").ConfigureAwait(false);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("commands")]
        [DSharpPlus.CommandsNext.Attributes.Description("Returns all commands stored.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task WriteCommands(CommandContext ctx)
        {
            if (!bot.stopAll)
            {
                try
                {
                    for (int i = 0; i < bot.commandNames.Count; i++)
                    {
                        string SendString = string.Empty;
                        //await WriteLine("kanal " + (i + 1) + ": " + kanalerna[i].realDiscordChannel.Name);
                        //SendString += WriteLine(bot.commandNames[i].Name);
                        string title = WriteLine(bot.commandNames[i].Name);
                        SendString = string.Empty;
                        if (bot.commandNames[i].Description != null && bot.commandNames[i].Description != string.Empty)
                        {
                            SendString += WriteLine(bot.commandNames[i].Description);
                        }
                        for (int a = 0; a < bot.commandNames[i].Overloads.Count; a++)
                        {
                            string temp = "overload " + (a + 1) + ": ";
                            for (int b = 0; b < bot.commandNames[i].Overloads[a].Arguments.Count; b++)
                            {
                                if (bot.commandNames[i].Overloads[a].Arguments[b].Description != null && bot.commandNames[i].Overloads[a].Arguments[b].Description != string.Empty)
                                {
                                    temp += " " + bot.commandNames[i].Overloads[a].Arguments[b].Description + "\n";
                                }
                            }
                            SendString += WriteLine(temp);
                        }
                        for (int a = 0; a < bot.commandNames[i].Aliases.Count; a++)
                        {
                            SendString += WriteLine("alias " + (a + 1) + ": " + bot.commandNames[i].Aliases[a]);
                            //await WriteLine(kanalerna[i].discordUsers[a].member.Username);
                        }
                        //await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        //{
                        //    Title = title,
                        //    Description = SendString,
                        //});
                        await WriteArrayEmbeed(SendString.Split('\n'), ctx, title);
                    }
                }
                catch (Exception e)
                {
                    await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("bot shutting down").ConfigureAwait(false);
            }
        }

        //Lägger till fler brackets för att fixa koden }}}}
        [DSharpPlus.CommandsNext.Attributes.Command("system")]
        [DSharpPlus.CommandsNext.Attributes.Description("Returns system info.")]
        //[DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task SystemInfo(CommandContext ctx)
        {
            string SendString = string.Empty;
            SendString += WriteLine("Program namn: " + System.AppDomain.CurrentDomain.FriendlyName/*, ctx*/);
            SendString += WriteLine("Bot namn: " + bot.Client.CurrentApplication.Name/*, ctx*/);
            SendString += WriteLine("D#+ version: " + bot.Client.VersionString/*, ctx*/);
            SendString += WriteLine("Gateway version: " + bot.Client.GatewayVersion/*, ctx*/);
            SendString += WriteLine("Operativ system: " + Environment.OSVersion/*, ctx*/);
            SendString += WriteLine(".Net version: " + Environment.Version/*, ctx*/);
            SendString += WriteLine("Bot version: " + botVersion/*, ctx*/);
#if DEBUG

            ScreenShootingShit screenShit = new ScreenShootingShit();
            ScreenShootingShit.DisplayInfoCollection displays = screenShit.GetDisplays();
            for (int i = 0; i < displays.Count; i++)
            {
                SendString += WriteLine("Monitor " + (i + 1) + " har en upplösning på " + displays[i].ScreenWidth + " gånger " + displays[i].ScreenHeight + " pixlar"/*, ctx*/);
            }
#else
            SendString += WriteLine("Monitor info funkar inte på raspberry");
#endif
            SendString += WriteLine("Dator namn: " + Environment.MachineName/*, ctx*/);
            SendString += WriteLine("Användarnamn: " + Environment.UserName/*, ctx*/);
            SendString += WriteLine("Dator organisation: " + Environment.UserDomainName/*, ctx*/);
            SendString += WriteLine("Fil mapp: " + Environment.CurrentDirectory/*, ctx*/);
            SendString += WriteLine("Kommando rad: " + "\"" + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + "\""/*, ctx*/);

            TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
            SendString += WriteLine("Tid sen full nedstängning: " + TimespanToString(uptime)/*, ctx*/);
            //if (uptime.TotalDays >= 1)
            //{
            //    SendString += WriteLine("Tid sen full nedstängning: " + uptime.Days + " dagar " + uptime.Hours + " timmar " + uptime.Minutes + " minuter"/*, ctx*/);
            //}
            //else if (uptime.TotalHours >= 1)
            //{
            //    SendString += WriteLine("Tid sen full nedstängning: " + (int)uptime.TotalHours + " timmar " + uptime.Minutes + " minuter"/*, ctx*/);
            //}
            //else if (uptime.TotalMinutes >= 1)
            //{
            //    SendString += WriteLine("Tid sen full nedstängning: " + (int)uptime.TotalMinutes + " minuter " + (int)uptime.Seconds + " sekunder "/*, ctx*/);
            //}
            //else
            //{
            //    SendString += WriteLine("Tid sen full nedstängning: " + (int)uptime.TotalSeconds + " sekunder "/*, ctx*/);
            //}

            SendString += WriteLine("Antal processor kärnor: " + Environment.ProcessorCount/*, ctx*/);
            SendString += WriteLine(Environment.Is64BitOperatingSystem ? "64 bitars operativ system" : "32 eller färre bitars operativ system"/*, ctx*/);
            SendString += WriteLine(Environment.Is64BitProcess ? "64 bitars program" : "32 bitars program"/*, ctx*/);
            //DiscordEmbedFooter footer = new DiscordEmbedFooter
            //{
            //    Text = "help"
            //};
            //new DiscordEmbedFooter
            //DiscordEmbedField field = new DiscordEmbedField
            //{
            //    Name = "help"
            //};
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "System info",
                Description = SendString,
            });
            //using (var fs = new FileStream("screenshotTemp.png", FileMode.Open, FileAccess.Read))
            //{
            //    var msg = await new DiscordMessageBuilder()
            //        .WithContent("Here is a really dumb file that I am testing with.")
            //        .WithFiles(new Dictionary<string, Stream>() { { "screenshotTemp.png", fs } })
            //        .WithEmbed(embed)
            //        //.WithContent(SendString)

            //        .SendAsync(ctx.Channel);
            //}
            SendString = string.Empty;
            //await WriteLine(SendString, ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("bot")]
        [DSharpPlus.CommandsNext.Attributes.Description("Returns system info.")]
        //[DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task BotInfo(CommandContext ctx)
        {
            string SendString = string.Empty;
            SendString += WriteLine("Bot namn: " + bot.Client.CurrentApplication.Name/*, ctx*/);
            SendString += WriteLine("Bot version: " + botVersion/*, ctx*/);
            //await WriteLine("Team name: " + Client.CurrentApplication.Team.Name/*, ctx*/);
            //var a = Client.CurrentApplication.Team.Members.ToArray();
            //for (int i = 0; i < a.Length; i++)
            //{
            //    await WriteLine("Member " + (i + 1) + ": " + Client.CurrentApplication.Team.Members/*, ctx*/);
            //}
            var b = bot.Client.CurrentApplication.Owners.ToArray();
            for (int i = 0; i < b.Length; i++)
            {
                SendString += WriteLine("Owner " + (i + 1) + ": " + b[i].Username/*, ctx*/);
            }
            TimeSpan uptime = bot.runTime.Elapsed;
            SendString += WriteLine("Upptid: " + TimespanToString(uptime)/*, ctx*/);
            //if (uptime.TotalDays >= 1)
            //{
            //    SendString += WriteLine("Upptid: " + uptime.Days + " dagar " + uptime.Hours + " timmar " + uptime.Minutes + " minuter"/*, ctx*/);
            //}
            //else if (uptime.TotalHours >= 1)
            //{
            //    SendString += WriteLine("Upptid: " + (int)uptime.TotalHours + " timmar " + uptime.Minutes + " minuter"/*, ctx*/);
            //}
            //else if (uptime.TotalMinutes >= 1)
            //{
            //    SendString += WriteLine("Upptid: " + (int)uptime.TotalMinutes + " minuter " + (int)uptime.Seconds + " sekunder "/*, ctx*/);
            //}
            //else
            //{
            //    SendString += WriteLine("Upptid: " + (int)uptime.TotalSeconds + " sekunder "/*, ctx*/);
            //}
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");//Set the User Agent to "request"

                    using (HttpResponseMessage response = client.GetAsync("https://api.github.com/repos/LordGurr/DiscordBot").Result)
                    {
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        string temp = "\"language\":\"";
                        int start = responseBody.IndexOf(temp);
                        if (start >= 0)
                        {
                            int end = responseBody.IndexOf("\",", start + 1 + temp.Length);
                            SendString += WriteLine("Programmerings språk: " + responseBody.Substring(start + temp.Length, end - start - 12));
                        }
                        temp = "\"created_at\":\"";
                        start = responseBody.IndexOf(temp);
                        if (start >= 0)
                        {
                            int end = responseBody.IndexOf("\"", start + 1 + temp.Length);
                            string send = responseBody.Substring(start + temp.Length, end - start - 14);
                            DateTime dateTime = Convert.ToDateTime(send);
                            SendString += WriteLine("Github repo skapades: " + dateTime.ToLongDateString());
                        }
                        temp = "\"updated_at\":\"";
                        start = responseBody.IndexOf(temp);
                        if (start >= 0)
                        {
                            int end = responseBody.IndexOf("\"", start + 1 + temp.Length);
                            string send = responseBody.Substring(start + temp.Length, end - start - 15);
                            DateTime dateTime = Convert.ToDateTime(send);
                            SendString += WriteLine("Senast uppdaterad: " + dateTime.ToLongDateString());
                        }
                    }
                    int index = 100;
                    string url = "LordGurr/DiscordBot";
                    string message = "\"message\":";
                    string downloaded = message;
                    for (int i = 0; i < 7; i++)
                    {
                        downloaded += downloaded;
                    }
                    while (AllIndexesOf(downloaded, message).Count >= index - 5)
                    {
                        index += 10;

                        using (HttpResponseMessage response = client.GetAsync("https://api.github.com/repos/" + url + "/commits?per_page=" + index).Result)
                        {
                            response.EnsureSuccessStatusCode();
                            downloaded = await response.Content.ReadAsStringAsync(); // LordGurr/DiscordBot
                        }
                    }
                    SendString += WriteLine("Har fått " + AllIndexesOf(downloaded, message).Count + " commits totalt");
                }
                catch (Exception e)
                {
                    SendString += WriteLine(e.Message);
                }
            }
            SendString += WriteLine("Har " + botCoinSaves.Count + " botcoin användare");
            SendString += WriteLine("Har " + simpPointSaves.Count + " simppoäng användare");
            int members = 0;
            for (int i = 0; i < kanalerna.Count; i++)
            {
                members += kanalerna[i].discordUsers.Count;
            }
            SendString += WriteLine("Har " + kanalerna.Count + " kanaler och " + members + " medlemmar");
            int totalGames = 0;
            for (int i = 0; i < bot.gameSaves.Count; i++)
            {
                for (int a = 0; a < bot.gameSaves[i].games.Count; a++)
                {
                    totalGames++;
                }
            }
            SendString += WriteLine("Har " + bot.gameSaves.Count + " gamesaves användare och " + totalGames + " spel sparade.");
            SendString += WriteLine("Har " + bot.membersChecking.Count + " användare som ska få online notiser");
            SendString += WriteLine("Sparade senast klockan " + bot.lastSave.ToShortTimeString() + ".");
            SendString += WriteLine("[Github repository](https://github.com/LordGurr/DiscordBot)");
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Bot info",
                Description = SendString,
            });
            SendString = string.Empty;
        }

        [DSharpPlus.CommandsNext.Attributes.Command("savebotcoin")]
        [DSharpPlus.CommandsNext.Attributes.Description("Saves botcoin users.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task SaveAllbotcoin(CommandContext ctx)
        {
            await bot.SaveBotCoin();
            if (ctx.Channel.Id != commandLine.Id)
            {
                await WriteLine("Sparade alla " + botCoinSaves.Count + " botcoin användares botcoin.", ctx);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("online")]
        [DSharpPlus.CommandsNext.Attributes.Description("Updates who is online.")]
        public async Task UpdateOnline(CommandContext ctx)
        {
            await bot.CheckOnline(ctx.Channel);
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("allcommands")]
        [DSharpPlus.CommandsNext.Attributes.Description("Saves botcoin users.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task AllCommands(CommandContext ctx)
        {
            await ctx.RespondAsync(this.ToString());
        }

        //[DSharpPlus.CommandsNext.Attributes.Command("isonline")]
        //[DSharpPlus.CommandsNext.Attributes.Description("Returns who is online.")]
        //public async Task IsOnline(CommandContext ctx)
        //{
        //    try
        //    {
        //        List<DiscordUser> usersMentioned = ctx.Message.MentionedUsers.ToList();
        //        string SendString = string.Empty;
        //        for (int i = 0; i < usersMentioned.Count; i++)
        //        {
        //            SendString += WriteLine(OnlineString(usersMentioned[i]));
        //        }
        //        if (SendString == string.Empty)
        //        {
        //            await ctx.Channel.SendMessageAsync("Please mention a user when sending this message");
        //        }
        //        else
        //        {
        //            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
        //            {
        //                Title = "Is online",
        //                Description = SendString,
        //            });
        //        }
        //        GiveBotCoin(ctx);
        //    }
        //    catch (Exception e)
        //    {
        //        await ctx.Channel.SendMessageAsync("Error: " + e.Message);
        //    }
        //}

        [DSharpPlus.CommandsNext.Attributes.Command("isonline")]
        [DSharpPlus.CommandsNext.Attributes.Description("Returns who is online.")]
        public async Task IsOnline(CommandContext ctx, [RemainingText] string idstring)
        {
            var task = AllUsersInMessage(idstring, ctx);
            await task;
            List<DiscordUser> usersMentioned = task.Result;
            string SendString = string.Empty;
            for (int i = 0; i < usersMentioned.Count; i++)
            {
                SendString += WriteLine(OnlineString(usersMentioned[i]));
            }
            if (SendString == string.Empty)
            {
                await ctx.Channel.SendMessageAsync("The ulongs weren't user id's or you didn't @ someone.");
            }
            else
            {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Is online",
                    Description = SendString,
                });
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("getactivity")]
        [DSharpPlus.CommandsNext.Attributes.Description("Returns who is online.")]
        public async Task GetActivity(CommandContext ctx, [RemainingText] string idstring)
        {
            var task = AllUsersInMessage(idstring, ctx);
            await task;
            List<DiscordUser> usersMentioned = task.Result;
            string SendString = string.Empty;
            for (int i = 0; i < usersMentioned.Count; i++)
            {
                SendString += WriteLine(ActivityString(usersMentioned[i]));
            }
            if (SendString == string.Empty)
            {
                await ctx.Channel.SendMessageAsync("The ulongs weren't user id's.");
            }
            else
            {
                //await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                //{
                //    Title = "Activities",
                //    Description = SendString,
                //});
                await WriteArrayEmbeed(SendString.Split('\n'), ctx, "Activities");
            }
        }

        private string OnlineString(DiscordUser discordUser)
        {
            try
            {
                if (discordUser.Presence != null)
                {
                    DiscordPresence presence = discordUser.Presence;
                    if (presence.Status != null)
                    {
                        if (presence.Status == UserStatus.Online)
                        {
                            return (discordUser.Username + " is online");
                        }
                        else if (presence.Status == UserStatus.Offline)
                        {
                            return (discordUser.Username + " is offline");
                        }
                        else if (presence.Status == UserStatus.Invisible)
                        {
                            return (discordUser.Username + " is trying to hide");
                        }
                        else if (presence.Status == UserStatus.Idle)
                        {
                            return (discordUser.Username + " is afk");
                        }
                    }
                    if (!presence.ClientStatus.Mobile.HasValue && !presence.ClientStatus.Desktop.HasValue && !presence.ClientStatus.Web.HasValue)
                    {
                        return discordUser.Username + " is not set";
                    }
                    if (presence.ClientStatus.Mobile.HasValue && presence.ClientStatus.Mobile.Value == UserStatus.Online || presence.ClientStatus.Desktop.HasValue && presence.ClientStatus.Desktop.Value == UserStatus.Online || presence.ClientStatus.Web.HasValue && presence.ClientStatus.Web.Value == UserStatus.Online)
                    {
                        //WriteLine(discordUser.Username + " is online");
                        //channel.SendMessageAsync(discordUser.Username + " is online").ConfigureAwait(false);
                        return discordUser.Username + " is online";
                    }
                    else if (presence.ClientStatus.Mobile.HasValue && presence.ClientStatus.Mobile.Value == UserStatus.Offline || presence.ClientStatus.Desktop.HasValue && presence.ClientStatus.Desktop.Value == UserStatus.Offline || presence.ClientStatus.Web.HasValue && presence.ClientStatus.Web.Value == UserStatus.Offline)
                    {
                        //WriteLine(discordUser.Username + " is offline");
                        //channel.SendMessageAsync(discordUser.Username + " is offline").ConfigureAwait(false);
                        return discordUser.Username + " is offline";
                    }
                    else if (presence.ClientStatus.Mobile.HasValue && presence.ClientStatus.Mobile.Value == UserStatus.DoNotDisturb || presence.ClientStatus.Desktop.HasValue && presence.ClientStatus.Desktop.Value == UserStatus.DoNotDisturb || presence.ClientStatus.Web.HasValue && presence.ClientStatus.Web.Value == UserStatus.DoNotDisturb)
                    {
                        //WriteLine(discordUser.Username + " is trying to hide");
                        //channel.SendMessageAsync(discordUser.Username + " is trying to hide").ConfigureAwait(false);
                        return discordUser.Username + " doesn't want to be disturbed";
                    }
                    else if (presence.ClientStatus.Mobile.HasValue && presence.ClientStatus.Mobile.Value == UserStatus.Invisible || presence.ClientStatus.Desktop.HasValue && presence.ClientStatus.Desktop.Value == UserStatus.Invisible || presence.ClientStatus.Web.HasValue && presence.ClientStatus.Web.Value == UserStatus.Invisible)
                    {
                        //WriteLine(discordUser.Username + " is trying to hide");
                        //channel.SendMessageAsync(discordUser.Username + " is trying to hide").ConfigureAwait(false);
                        return discordUser.Username + " is trying to hide";
                    }
                    else if (presence.ClientStatus.Mobile.HasValue && presence.ClientStatus.Mobile.Value == UserStatus.Idle || presence.ClientStatus.Desktop.HasValue && presence.ClientStatus.Desktop.Value == UserStatus.Idle || presence.ClientStatus.Web.HasValue && presence.ClientStatus.Web.Value == UserStatus.Idle)
                    {
                        //WriteLine(discordUser.Username + " is trying to hide");
                        //channel.SendMessageAsync(discordUser.Username + " is trying to hide").ConfigureAwait(false);
                        return discordUser.Username + " is afk";
                    }
                    else
                    {
                        //WriteLine(discordUser.Username + " is not set");
                        //channel.SendMessageAsync(discordUser.Username + " is not set").ConfigureAwait(false);
                        return discordUser.Username + " is not set";
                    }
                }
                else
                {
                    //await WriteLine(discordUser.Username + " is not set");
                    //await channel.SendMessageAsync(discordUser.Username + " is not set").ConfigureAwait(false);
                    return discordUser.Username + " is not set";
                }
            }
            catch (Exception e)
            {
                //await WriteLine(e.Message);
                return discordUser.Username + " errored: " + e.Message;
            }
        }

        private string ActivityString(DiscordUser discordUser)
        {
            try
            {
                string result = string.Empty;
                if (discordUser.Presence != null)
                {
                    DiscordPresence presence = discordUser.Presence;
                    if (presence.Activities.Count > 0)
                    {
                        if (presence.Activity != null)
                        {
                            for (int i = 0; i < presence.Activities.Count; i++)
                            {
                                result += presence.Activities[i].ActivityType.ToString() + " " + presence.Activities[i].Name + "\n";
                                if (presence.Activities[i].RichPresence != null)
                                {
                                    result += "Spel: " + presence.Activities[i].RichPresence.Application + " detaljer" + presence.Activities[i].RichPresence.Details;
                                }
                            }
                            if (result == string.Empty)
                            {
                                result = discordUser.Username + " is not doing anything";
                            }
                        }
                    }
                    else if (presence.Activity != null)
                    {
                        result += presence.Activity.ActivityType.ToString() + " " + presence.Activity.Name + "\n";
                        if (result == string.Empty)
                        {
                            result = " is not doing anything";
                        }
                    }
                    else
                    {
                        result = discordUser.Username + " is not set";
                    }
                }
                else
                {
                    //await WriteLine(discordUser.Username + " is not set");
                    //await channel.SendMessageAsync(discordUser.Username + " is not set").ConfigureAwait(false);
                    result = " is not set";
                }
                result = result.Insert(0, "\n" + discordUser.Username + " is: ");
                return result;
            }
            catch (Exception e)
            {
                //await WriteLine(e.Message);
                return discordUser.Username + " errored: " + e.Message + " callstack: " + e.StackTrace;
            }
        }

        private string OnlineString(DiscordMember discordUser)
        {
            try
            {
                if (discordUser.Presence != null)
                {
                    DiscordPresence presence = discordUser.Presence;
                    if (presence.Status != null)
                    {
                        if (presence.Status == UserStatus.Online)
                        {
                            return (discordUser.Username + " is online");
                        }
                        else if (presence.Status == UserStatus.Offline)
                        {
                            return (discordUser.Username + " is offline");
                        }
                        else if (presence.Status == UserStatus.Invisible)
                        {
                            return (discordUser.Username + " is trying to hide");
                        }
                        else if (presence.Status == UserStatus.Idle)
                        {
                            return (discordUser.Username + " is afk");
                        }
                    }
                    if (!presence.ClientStatus.Mobile.HasValue && !presence.ClientStatus.Desktop.HasValue && !presence.ClientStatus.Web.HasValue)
                    {
                        return discordUser.Username + " is not set";
                    }
                    if (presence.ClientStatus.Mobile.HasValue && presence.ClientStatus.Mobile.Value == UserStatus.Online || presence.ClientStatus.Desktop.HasValue && presence.ClientStatus.Desktop.Value == UserStatus.Online || presence.ClientStatus.Web.HasValue && presence.ClientStatus.Web.Value == UserStatus.Online)
                    {
                        //WriteLine(discordUser.Username + " is online");
                        //channel.SendMessageAsync(discordUser.Username + " is online").ConfigureAwait(false);
                        return discordUser.Username + " is online";
                    }
                    else if (presence.ClientStatus.Mobile.HasValue && presence.ClientStatus.Mobile.Value == UserStatus.Offline || presence.ClientStatus.Desktop.HasValue && presence.ClientStatus.Desktop.Value == UserStatus.Offline || presence.ClientStatus.Web.HasValue && presence.ClientStatus.Web.Value == UserStatus.Offline)
                    {
                        //WriteLine(discordUser.Username + " is offline");
                        //channel.SendMessageAsync(discordUser.Username + " is offline").ConfigureAwait(false);
                        return discordUser.Username + " is offline";
                    }
                    else if (presence.ClientStatus.Mobile.HasValue && presence.ClientStatus.Mobile.Value == UserStatus.DoNotDisturb || presence.ClientStatus.Desktop.HasValue && presence.ClientStatus.Desktop.Value == UserStatus.DoNotDisturb || presence.ClientStatus.Web.HasValue && presence.ClientStatus.Web.Value == UserStatus.DoNotDisturb)
                    {
                        //WriteLine(discordUser.Username + " is trying to hide");
                        //channel.SendMessageAsync(discordUser.Username + " is trying to hide").ConfigureAwait(false);
                        return discordUser.Username + " doesn't want to be disturbed";
                    }
                    else if (presence.ClientStatus.Mobile.HasValue && presence.ClientStatus.Mobile.Value == UserStatus.Invisible || presence.ClientStatus.Desktop.HasValue && presence.ClientStatus.Desktop.Value == UserStatus.Invisible || presence.ClientStatus.Web.HasValue && presence.ClientStatus.Web.Value == UserStatus.Invisible)
                    {
                        //WriteLine(discordUser.Username + " is trying to hide");
                        //channel.SendMessageAsync(discordUser.Username + " is trying to hide").ConfigureAwait(false);
                        return discordUser.Username + " is trying to hide";
                    }
                    else if (presence.ClientStatus.Mobile.HasValue && presence.ClientStatus.Mobile.Value == UserStatus.Idle || presence.ClientStatus.Desktop.HasValue && presence.ClientStatus.Desktop.Value == UserStatus.Idle || presence.ClientStatus.Web.HasValue && presence.ClientStatus.Web.Value == UserStatus.Idle)
                    {
                        //WriteLine(discordUser.Username + " is trying to hide");
                        //channel.SendMessageAsync(discordUser.Username + " is trying to hide").ConfigureAwait(false);
                        return discordUser.Username + " is afk";
                    }
                    else
                    {
                        //WriteLine(discordUser.Username + " is not set");
                        //channel.SendMessageAsync(discordUser.Username + " is not set").ConfigureAwait(false);
                        return discordUser.Username + " is not set";
                    }
                }
                else
                {
                    //await WriteLine(discordUser.Username + " is not set");
                    //await channel.SendMessageAsync(discordUser.Username + " is not set").ConfigureAwait(false);
                    return discordUser.Username + " is not set";
                }
            }
            catch (Exception e)
            {
                //await WriteLine(e.Message);
                return discordUser.Username + " errored: " + e.Message;
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("savemembers")]
        [DSharpPlus.CommandsNext.Attributes.Description("Saves members and channels users.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task SaveAllmembers(CommandContext ctx)
        {
            await bot.SaveMembers();
            if (ctx.Channel.Id != commandLine.Id)
            {
                int members = 0;
                for (int i = 0; i < kanalerna.Count; i++)
                {
                    members += kanalerna[i].discordUsers.Count;
                }
                await WriteLine("Har läst in " + kanalerna.Count + " kanaler och " + members + " medlemmar", ctx);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("pendingremind")]
        [DSharpPlus.CommandsNext.Attributes.Description("Returns the pending remindmes.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task PendingRemind(CommandContext ctx)
        {
            string SendString = string.Empty;
            for (int i = 0; i < bot.queuedRemindMes.Count; i++)
            {
                SendString += WriteLine(bot.queuedRemindMes[i].dateTime.ToShortTimeString() + " " + bot.queuedRemindMes[i].username);
            }
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Queued remindmes",
                Description = SendString,
            });
            SendString = string.Empty;
        }

        [DSharpPlus.CommandsNext.Attributes.Command("directmessage")]
        [DSharpPlus.CommandsNext.Attributes.Description("Messages fucking everyone.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task DirectMessage(CommandContext ctx, [RemainingText] string str)
        {
            List<DiscordMember> membersToSpam = new List<DiscordMember>();
            for (int i = 0; i < kanalerna.Count; i++)
            {
                for (int a = 0; a < kanalerna[i].discordUsers.Count; a++)
                {
                    if (!membersToSpam.Contains(kanalerna[i].discordUsers[a].member))
                    {
                        membersToSpam.Add(kanalerna[i].discordUsers[a].member);
                    }
                }
            }
            for (int i = 0; i < membersToSpam.Count; i++)
            {
                var c = membersToSpam[i].CreateDmChannelAsync();
                await c.Result.SendMessageAsync(str);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("upload")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("laddaup")]
        [DSharpPlus.CommandsNext.Attributes.Description("Laddar upp en specificerad fil.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task FileUpload(CommandContext ctx, [RemainingText] string filePath)
        {
            //string actualFilePath = "";
            //for (int i = 0; i < filePath.Length; i++)
            //{
            //    actualFilePath += filePath[i];
            //    if (i + 1 < filePath.Length)
            //    {
            //        actualFilePath += " ";
            //    }
            //}
            if (!filePath.Contains("config.json"))
            {
                await bot.UploadFile(filePath, ctx.Channel);
            }
            else
            {
                await ctx.RespondAsync("I won't upload any config files through discord");
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("uploadall")]
        [DSharpPlus.CommandsNext.Attributes.Description("Laddar upp en specificerad fil.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task FileUploadAll(CommandContext ctx)
        {
            try
            {
                await bot.UploadFile("gåtSvaren.txt", ctx.Channel); //  ?upload gåtSvaren.txt
                await bot.UploadFile("gåtor.txt", ctx.Channel); //  ?upload gåtor.txt
                await bot.UploadFile("nouns.txt", ctx.Channel); // ?upload nouns.txt
                await bot.UploadFile("citat.txt", ctx.Channel); // ?upload  citat.txt
                await bot.UploadFile("emotions.txt", ctx.Channel); // ?upload emotions.txt
                await bot.UploadFile("citatTemplate.txt", ctx.Channel); // ?upload citatTemplate.txt
                await bot.UploadFile("botCoinSave.txt", ctx.Channel); // ?upload botCoinSave.txt
                await bot.UploadFile("simppointsave.txt", ctx.Channel); // ?upload simppointsave.txt
                await bot.UploadFile("channels.txt", ctx.Channel); // ?upload channels.txt
                await bot.UploadFile("usergamesaves.txt", ctx.Channel); // ?upload usergamesaves.txt
                string[] tempArray = await File.ReadAllLinesAsync("channels.txt");
                for (int i = 0; i < tempArray.Length - 1; i++)
                {
                    await bot.UploadFile(tempArray[i], ctx.Channel);
                }
            }
            catch (Exception e)
            {
                await ctx.RespondAsync("COuldn't upload file: " + e.Message);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("skärmdump")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("skärmbild", "screenshot")]
        [DSharpPlus.CommandsNext.Attributes.Description("Takes a screenshot.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task Screenshot(CommandContext ctx)
        {
#if DEBUG
            await bot.TakeScreenshotAndUpload(ctx);
#else
            await ctx.RespondAsync("Du kan bara göra detta på windows");
#endif
        }

        [DSharpPlus.CommandsNext.Attributes.Command("skärm")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("monitorbild", "monitorshot", "monitor")]
        [DSharpPlus.CommandsNext.Attributes.Description("Takes a screenshot.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task ScreenshotMonitor(CommandContext ctx, int index)
        {
            try
            {
#if DEBUG
                ScreenShootingShit screenShit = new ScreenShootingShit();
                ScreenShootingShit.DisplayInfoCollection displays = screenShit.GetDisplays();
                await bot.TakeScreenshotAndUploadApplication(ctx, displays[index - 1].hMonitor);
#else
                await ctx.RespondAsync("Because i might be on raspberry i won't do this");
#endif
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("app")]
        [DSharpPlus.CommandsNext.Attributes.Description("Takes a screenshot off application.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task ScreenshotApp(CommandContext ctx)
        {
            try
            {
                // Move window to show as much as possible
#if DEBUG
                int x = Console.CursorLeft;
                int y = Console.CursorTop;
                Console.SetCursorPosition(0, 0);
                await Task.Delay(1);
                Console.SetCursorPosition(x, y);
                await Task.Delay(5);
                await bot.TakeScreenshotAndUploadApplication(ctx, Process.GetCurrentProcess().MainWindowHandle);
#else
                await ctx.RespondAsync("No screenshots on raspberry");
#endif
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("getapps")]
        [DSharpPlus.CommandsNext.Attributes.Description("Skriver ut alla program som är i gång.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task GetApp(CommandContext ctx)
        {
            try
            {
                string SendString = string.Empty;
                foreach (KeyValuePair<IntPtr, string> window in OpenWindowGetter.GetOpenWindows())
                {
                    IntPtr handle = window.Key;

                    string title = window.Value;

                    SendString += WriteLine(title/* + ", (" + handle + ")"*/);
                }
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Applications info",
                    Description = SendString,
                });
                SendString = string.Empty;
            }
            catch (Exception e)
            {
                await ctx.RespondAsync(e.Message);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("appsscreenshot")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("appsskärmbilder", "appsskärmdumpar")]
        [DSharpPlus.CommandsNext.Attributes.Description("Tar en skärmdump på alla program som är i gång.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task GetAppScreen(CommandContext ctx)
        {
#if DEBUG
            string SendString = string.Empty;
            foreach (KeyValuePair<IntPtr, string> window in OpenWindowGetter.GetOpenWindows())
            {
                IntPtr handle = window.Key;

                string title = window.Value;

                await CommandWriteLine(title /*+ ", (" + handle + ")"*/, ctx);
                try
                {
                    await bot.TakeScreenshotAndUploadApplication(ctx, handle);
                }
                catch (Exception e)
                {
                    await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
                }
            }
#else
            await ctx.RespondAsync("Endast på windows som sagt");
#endif
        }

        //[DSharpPlus.CommandsNext.Attributes.Command("commandline")]
        //[DSharpPlus.CommandsNext.Attributes.Description("Takes a screenshot.")]
        //[DSharpPlus.CommandsNext.Attributes.RequireOwner]
        //public async Task ExecCommand(CommandContext ctx, [RemainingText] string strCmdLine)
        //{
        //    // Start the child process.
        //    try
        //    {
        //        Process p = new Process();
        //        // Redirect the output stream of the child process.
        //        p.StartInfo.UseShellExecute = false;
        //        p.StartInfo.RedirectStandardOutput = true;
        //        p.StartInfo.FileName = strCmdLine;
        //        //p.StartInfo.Arguments = strCmdLine;
        //        p.Start();

        //        // Do not wait for the child process to exit before
        //        // reading to the end of its redirected stream.
        //        // p.WaitForExit();
        //        // Read the output stream first and then wait.
        //        string output = p.StandardOutput.ReadToEnd();
        //        await ctx.Channel.SendMessageAsync(output).ConfigureAwait(false);
        //        p.WaitForExit();
        //        await bot.TakeScreenshotAndUpload(ctx);
        //    }
        //    catch (Exception e)
        //    {
        //        await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
        //    }
        //}

        [DSharpPlus.CommandsNext.Attributes.Command("fuck")]
        [DSharpPlus.CommandsNext.Attributes.Description("Tells people to fuck off.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task Fuck(CommandContext ctx)
        {
            string temp;
            if (ctx.Message.MentionedUsers.Count > 0)
            {
                temp = "";
                for (int i = 0; i < ctx.Message.MentionedUsers.Count; i++)
                {
                    temp += ctx.Message.MentionedUsers[i].Mention;
                    if (i + 2 == ctx.Message.MentionedUsers.Count && ctx.Message.MentionedUsers.Count > 1)
                    {
                        temp += " and ";
                    }
                    else if (ctx.Message.MentionedUsers.Count > 1)
                    {
                        temp += ", ";
                    }
                }
            }
            else
            {
                temp = ctx.Message.Content.ToLower().Replace("?fuck ", "");
                temp = temp.ToLower().Replace("?fuck", "");
            }
            //await ctx.Channel.SendMessageAsync("Fuck " + ctx.User.Mention).ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync("Fuck you " + temp).ConfigureAwait(false);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("shutdown")]
        [DSharpPlus.CommandsNext.Attributes.Description("Shutdown immediately.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task Shutdown(CommandContext ctx)
        {
            if (!bot.shutdown)
            {
                bot.restart = false;
                bot.shutdown = true;
                bot.stopAll = true;
                //TimeSpan temp = DateTime.Now - lastSave;
                //await WriteLine("Stänger ner inom " + (sparTid.TotalMinutes - temp.TotalMinutes).ToString("F1") + " minuter.\nKommer inte att starta igen.", e);
                await CommandWriteLine("Stänger ner omedelbart på order av: " + ctx.Member.DisplayName + "(" + ctx.Member.Username + ")", ctx);
                if (bot.isAdding)
                {
                    await WriteLine("Väntar på att ladda in medlemmar ", ctx);
                    while (bot.isAdding)
                    {
                        await Task.Delay(5000);
                    }
                }
                await bot.Client.DisconnectAsync();
                await Task.Delay(500);
                await SaveAllbotcoin(ctx);
                await SaveAllmembers(ctx);
                await SaveGameTime(ctx);
#if DEBUG
                await bot.TakeScreenshotAndUploadApplication(ctx, Process.GetCurrentProcess().MainWindowHandle);
#else
                await WriteLine("Låtsas som om detta är en skärmdump", ctx);
#endif
                bot.Client.Dispose();
                await Task.Delay(500);
                Environment.Exit(0);
            }
            else
            {
                if (shutdownTime != null)
                {
                    await WriteLine("Avstängning redan planerad klockan: " + shutdownTime.ToShortTimeString() + " och restart är sätt till " + bot.restart + ".", ctx);
                }
                else
                {
                    await WriteLine("En avstängning är redan bestämd och restart är sätt till " + bot.restart + ".", ctx);
                }
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("reboot")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("restart")]
        [DSharpPlus.CommandsNext.Attributes.Description("Reboots safely.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task Reboot(CommandContext ctx)
        {
            if (!bot.shutdown)
            {
                bot.restart = true;
                bot.shutdown = true;
                TimeSpan temp = DateTime.Now - bot.lastSave;
                await CommandWriteLine("Startar om inom " + (bot.sparTid.TotalMinutes - temp.TotalMinutes).ToString("F1") + " minuter på order av: " + ctx.Member.DisplayName + "(" + ctx.Member.Username + ")" + "\nReboot är satt till " + bot.restart + ".", ctx);
                shutdownTime = DateTime.Now.AddMinutes(bot.sparTid.TotalMinutes - temp.TotalMinutes);
                await Activity(ctx, 2, "Rebooting " + shutdownTime.ToShortTimeString());
                await bot.Client.DisconnectAsync();
                //await TakeScreenshotAndUpload(e);
            }
            else
            {
                if (shutdownTime != null)
                {
                    await WriteLine("Avstängning redan planerad klockan: " + shutdownTime.ToShortTimeString() + " och restart är sätt till " + bot.restart + ".", ctx);
                }
                else
                {
                    await WriteLine("En avstängning är redan bestämd och restart är sätt till " + bot.restart + ".", ctx);
                }
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("safeshutdown")]
        [DSharpPlus.CommandsNext.Attributes.Description("Shuts down safely.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task Safeshutdown(CommandContext ctx)
        {
            if (!bot.shutdown)
            {
                bot.restart = false;
                bot.shutdown = true;
                TimeSpan temp = DateTime.Now - bot.lastSave;
                await CommandWriteLine("Stänger ner inom " + (bot.sparTid.TotalMinutes - temp.TotalMinutes).ToString("F1") + " minuter på order av: " + ctx.Member.DisplayName + "(" + ctx.Member.Username + ")" + "\nKommer inte att starta igen.", ctx);
                shutdownTime = DateTime.Now.AddMinutes(bot.sparTid.TotalMinutes - temp.TotalMinutes);
                await Activity(ctx, 2, "Shutdown " + shutdownTime.ToShortTimeString());
                await bot.Client.DisconnectAsync();
                //await TakeScreenshotAndUpload(e);
            }
            else
            {
                if (shutdownTime != null)
                {
                    await WriteLine("Avstängning redan planerad klockan: " + shutdownTime.ToShortTimeString() + " och restart är sätt till " + bot.restart + ".", ctx);
                }
                else
                {
                    await WriteLine("En avstängning är redan bestämd och restart är sätt till " + bot.restart + ".", ctx);
                }
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("sudo")]
        [DSharpPlus.CommandsNext.Attributes.Description("Executes a command as another user.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task Sudo(CommandContext ctx, [DSharpPlus.CommandsNext.Attributes.Description("Member to execute as.")] DiscordMember member, [RemainingText, DSharpPlus.CommandsNext.Attributes.Description("Command text to execute.")] string command)
        {
            // note the [RemainingText] attribute on the argument.
            // it will capture all the text passed to the command

            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            // get the command service, we need this for
            // sudo purposes
            var cmds = ctx.CommandsNext;

            // retrieve the command and its arguments from the given string
            var cmd = cmds.FindCommand(command, out var customArgs);
            //for (int i = 1; i < command.Length; i++)
            //{
            //    customArgs += command[i] + " ";
            //}
            // create a fake CommandContext
            var fakeContext = cmds.CreateFakeContext(member, ctx.Channel, command, ctx.Prefix, cmd, customArgs);

            // and perform the sudo
            await cmds.ExecuteCommandAsync(fakeContext);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("sudo")]
        [DSharpPlus.CommandsNext.Attributes.Description("Executes a command as another user.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task Sudo(CommandContext ctx, [RemainingText, DSharpPlus.CommandsNext.Attributes.Description("Command text to execute.")] string command)
        {
            List<string> idlist = command.Split(' ', '\n').ToList();
            List<ulong> ids = new List<ulong>();
            for (int i = 0; i < idlist.Count; i++)
            {
                try
                {
                    if (!idlist[i].Contains('@') && IsDigitsOnly(idlist[i], string.Empty))
                    {
                        ids.Add(Convert.ToUInt64(idlist[i]));
                    }
                }
                catch (Exception)
                {
                    await ctx.Channel.SendMessageAsync(idlist[i] + " is not a valid ulong.");
                }
            }
            List<DiscordUser> usersMentioned = ctx.Message.MentionedUsers.ToList();
            for (int i = 0; i < ids.Count; i++)
            {
                try
                {
                    var user = bot.Client.GetUserAsync(ids[i]);
                    usersMentioned.Add(user.Result);
                    command = command.Replace(ids[i].ToString(), string.Empty);
                }
                catch (Exception e)
                {
                    await ctx.Channel.SendMessageAsync(idlist[i] + " is not a valid user id and sent the error: " + e.Message);
                }
            }
            // note the [RemainingText] attribute on the argument.
            // it will capture all the text passed to the command

            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            // get the command service, we need this for
            // sudo purposes
            var cmds = ctx.CommandsNext;
            // retrieve the command and its arguments from the given string
            var cmd = cmds.FindCommand(command, out var customArgs);
            //for (int i = 1; i < command.Length; i++)
            //{
            //    customArgs += command[i] + " ";
            //}
            // create a fake CommandContext
            for (int i = 0; i < usersMentioned.Count; i++)
            {
                var fakeContext = cmds.CreateFakeContext(usersMentioned[i], ctx.Channel, command, ctx.Prefix, cmd, customArgs);
                await cmds.ExecuteCommandAsync(fakeContext);
            }
            if (usersMentioned.Count < 1)
            {
                await ctx.RespondAsync("Didn't get any users");
            }

            // and perform the sudo
        }

        [DSharpPlus.CommandsNext.Attributes.Command("activity")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("aktivitet")]
        [DSharpPlus.CommandsNext.Attributes.Description("Sets game bot is playing.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task Activity(CommandContext ctx, int activityType, [RemainingText] string input)
        {
            //string input = "";
            //// Real Shit
            //for (int i = 0; i < inputs.Length; i++)
            //{
            //    input += inputs[i] + " ";
            //}
            var activity = new DiscordActivity
            {
                Name = input,
                ActivityType = (ActivityType)activityType,
            };
            try
            {
                await bot.Client.UpdateStatusAsync(activity);
                if (activity.ActivityType == ActivityType.Custom)
                {
                    await ctx.RespondAsync(activity.CustomStatus.Name + "  " + activity.CustomStatus.Emoji);
                }
                else
                {
                    await ctx.RespondAsync("Aktivitet satt till: " + activity.ActivityType.ToString() + " " + activity.Name);
                }
            }
            catch (Exception e)
            {
                await ctx.RespondAsync("Kan inte uppdatera status: " + e.Message);
            }
        }

        /* Owner Commands */

        /* Music Commands */

        /*private int SongID = 1;
        private MusicPlayer player;

        [DSharpPlus.CommandsNext.Attributes.Command("join")]
        [DSharpPlus.CommandsNext.Attributes.RequireBotPermissions(Permissions.UseVoice)]
        public async Task Join(CommandContext ctx)
        {
            //Initialize music player
            player = new MusicPlayer(ctx);

            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                await ctx.RespondAsync("Already connected in this guild.");
                throw new InvalidOperationException("Already connected in this guild.");
            }

            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync("You need to be in a voice channel.");
                throw new InvalidOperationException("You need to be in a voice channel.");
            }

            vnc = await vnext.ConnectAsync(chn);
            await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:")); //👌
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }

            vnc.Disconnect();
            await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:")); //👌
        }

        [Command("play"), DSharpPlus.CommandsNext.Attributes.Description("Plays an audio file.")]
        public async Task Play(CommandContext ctx, [RemainingText, DSharpPlus.CommandsNext.Attributes.Description("Full path to the file to play.")] string filename)
        {
            // check whether VNext is enabled
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            // check whether we aren't already connected
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                // already connected
                await Join(ctx); //If we aren't already joined in the guid then join it
                                 //await ctx.RespondAsync("Not connected in this guild.");
                                 //return;
            }

            //Add songs
            if (!string.IsNullOrWhiteSpace(filename)) //If we only type !play
            {
                Song song = new Song(filename);
                player.AddMusic(song);
            }

            //Start Playing
            if (player.qs.Count <= 0)
            {
                await ctx.RespondAsync("Add a song first");
            }
            await player.Play();
        }*/

        /* Music Commands */

        [DSharpPlus.CommandsNext.Attributes.Command("ping")]
        [DSharpPlus.CommandsNext.Attributes.Description("Pingar dig.")]
        [DSharpPlus.CommandsNext.Attributes.Hidden]
        public async Task Ping(CommandContext ctx)
        {
            var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");
            await ctx.RespondAsync($"{emoji} Pong! Ping: {ctx.Client.Ping}ms");
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("riddle")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("gåta")]
        [DSharpPlus.CommandsNext.Attributes.Description("Returnerar en gåta.")]
        public async Task Ridle(CommandContext ctx)
        {
            //gåtor = new string[9] { "Fattiga har mig, rika behöver mig. Äter du mig dör du. Vad är jag?", "Vad blir blötare ju mer det torkar?", "Den som tillverkar mig behöver mig inte, den som köper mig använder mig inte, den som använder mig kan varken se eller känna mig. Vad är jag?", "Om du har mig, vill du dela med dig av mig. Om du delar med dig av mig, har du mig inte. Vad är jag?", "Vilka katter mår bäst i hög värme?", "Går från hus till hus men kommer aldrig in?", "Har hatt och fot men saknar huvud och sko?", "Vad har huvud men inget öga?", "Vad har öga men kan inte se?" };
            //svar = new string[9] { "Ingenting", "Handduken", "En kista", "En hemlighet", "Lussekatterna", "Vägen", "Svampen", "Spiken", "Nålen" };
            int random = rng.Next(gåtor.Length);
            await ctx.Channel.SendMessageAsync(gåtor[random]).ConfigureAwait(false);
            DateTime startTid = DateTime.Now;
            TimeSpan deltaTid = DateTime.Now - startTid;
            await ctx.Channel.TriggerTypingAsync();
            while (deltaTid.TotalMilliseconds < 5000)
            {
                deltaTid = DateTime.Now - startTid;
            }
            await ctx.Channel.SendMessageAsync(random < svar.Length ? svar[random] : "Error. Svar saknas på: " + random).ConfigureAwait(false);

            //Console.WriteLine("\n\n" + ctx.Guild.PreferredLocale);
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("quote")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("citat")]
        [DSharpPlus.CommandsNext.Attributes.Description("Returns a quote.")]
        public async Task Citat(CommandContext ctx)
        {
            int random = rng.Next(quote.Length);
            await ctx.Channel.SendMessageAsync(quote[random]).ConfigureAwait(false);
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("genequote")]
        [DSharpPlus.CommandsNext.Attributes.Description("Returns a generated quote.")]
        public async Task GenerateCitat(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(GenerateQuote()).ConfigureAwait(false);
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("botcoin")]
        [DSharpPlus.CommandsNext.Attributes.Description("Signs you up for botcoin and tells you how many you have.")]
        public async Task BotCoin(CommandContext ctx)
        {
            int i = OnlyBotCoinIndex(ctx);
            if (i > -1)
            {
                if (botCoinSaves[i].userName == null)
                {
                    botCoinSaves[i].userName = ctx.Message.Author.Username;
                }
                await ctx.Channel.SendMessageAsync("Du är redan uppskriven för botcoin och har " + botCoinSaves[i].antalBotCoin + " botcoins.").ConfigureAwait(false);
                return;
            }
            botCoinSaves.Add(new BotCoinSaveData(ctx.Message.Author.Id, rng.Next(0, 10), DateTime.Now.AddMinutes(-5), ctx.Message.Author.Username));
            await ctx.Channel.SendMessageAsync("Du är nu uppskriven för botcoin och har: " + botCoinSaves[botCoinSaves.Count - 1].antalBotCoin + " botcoins.").ConfigureAwait(false);
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("simppoint")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("simpoäng")]
        [DSharpPlus.CommandsNext.Attributes.Description("Signs you up for simppoint and tells you how many you have.")]
        public async Task SimpPoint(CommandContext ctx)
        {
            int i = SimpPointIndex(ctx);
            if (i > -1)
            {
                if (simpPointSaves[i].userName == null)
                {
                    simpPointSaves[i].userName = ctx.Message.Author.Username;
                }
                await ctx.Channel.SendMessageAsync(ctx.Message.Author.Username + " är redan uppskriven för simpPoint och har " + simpPointSaves[i].antalSimpPoint + " simppoäng.").ConfigureAwait(false);
                return;
            }
            simpPointSaves.Add(new SimpPointSaveData(ctx.Message.Author.Id, rng.Next(0, 10), DateTime.Now.AddMinutes(-5), ctx.Message.Author.Username));
            await ctx.Channel.SendMessageAsync(ctx.Message.Author.Username + " är nu uppskriven för simppoäng och har: " + simpPointSaves[simpPointSaves.Count - 1].antalSimpPoint + " simppoäng.").ConfigureAwait(false);
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("giveSimpPoint")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("sippoäng")]
        [DSharpPlus.CommandsNext.Attributes.Description("Signs you up for simppoint and tells you how many you have.")]
        public async Task SimpPoint(CommandContext ctx, [RemainingText] string input)
        {
            var task = AllUsersInMessage(input, ctx);
            await task;
            List<DiscordUser> usersInMessage = task.Result;
            for (int i = 0; i < usersInMessage.Count; i++)
            {
                if (i > 3)
                {
                    await ctx.RespondAsync("Du kan bara ge fyra personer simppoäng åt gången. Fy " + ctx.Message.Author.Username);
                    break;
                }
                if (usersInMessage[i].Id != ctx.Message.Author.Id)
                {
                    int a = SimpPointIndex(ctx);
                    if (a > -1)
                    {
                        int points = GiveSimpPoint(usersInMessage[i]);
                        if (points < 0)
                        {
                            await ctx.RespondAsync(usersInMessage[i].Username + " fick simppoäng för mindre än tolv sekunder sen och måste vänta");
                        }
                        await ctx.RespondAsync(ctx.Message.Author.Username + " gav " + usersInMessage[i].Username + " " + points + " simppoäng.");
                    }
                    else
                    {
                        await Sudo(ctx, "simppoint " + usersInMessage[i].Mention);
                        a = SimpPointIndex(ctx);
                        if (a > -1)
                        {
                            int points = GiveSimpPoint(usersInMessage[i]);
                            if (points < 0)
                            {
                                await ctx.RespondAsync(usersInMessage[i].Username + " fick simppoäng för mindre än tolv sekunder sen och måste vänta");
                            }
                            await ctx.RespondAsync(ctx.Message.Author.Username + " gav " + usersInMessage[i].Username + " " + points + " simppoäng.");
                        }
                    }
                }
                else
                {
                    await ctx.RespondAsync("Fy på dig " + ctx.Message.Author.Username + ". Du kan inte ge dig själv poäng");
                }
            }
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("gametime")]
        [DSharpPlus.CommandsNext.Attributes.Description("Signs you up for saving the amount of time you spend in games.")]
        public async Task GameTime(CommandContext ctx)
        {
            int i = bot.GameTimeIndex(ctx);
            if (i > -1)
            {
                await ctx.Channel.SendMessageAsync(ctx.User.Username + " är redan uppskriven för gametime och har " + bot.gameSaves[i].games.Count + " spel som vars tid räknas.").ConfigureAwait(false);

                return;
            }
            if (!ctx.User.IsBot)
            {
                bot.gameSaves.Add(new UserGameSave(ctx.Message.Author.Id));
                await ctx.Channel.SendMessageAsync(ctx.User.Username + " är nu uppskriven för gametime och så fort du börjar spela ett spel borde det registreras.").ConfigureAwait(false);
            }
            else
            {
                await ctx.Channel.SendMessageAsync(ctx.User.Username + " får inte skriva upp sig för gametime då du är en bot").ConfigureAwait(false);
            }
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("updategamesaves")]
        [DSharpPlus.CommandsNext.Attributes.Description("Signs you up for saving the amount of time you spend in games.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task UpdateGameTime(CommandContext ctx)
        {
            await bot.UpdateGameSaves();
        }

        [DSharpPlus.CommandsNext.Attributes.Command("savegamesaves")]
        [DSharpPlus.CommandsNext.Attributes.Description("Signs you up for saving the amount of time you spend in games.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task SaveGameTime(CommandContext ctx)
        {
            await bot.SaveGameSaves();
        }

        [DSharpPlus.CommandsNext.Attributes.Command("savesimppoints")]
        [DSharpPlus.CommandsNext.Attributes.Description("Signs you up for saving the amount of time you spend in games.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task SaveSimpPoints(CommandContext ctx)
        {
            await bot.SaveSimpPoint();
        }

        [DSharpPlus.CommandsNext.Attributes.Command("lastsave")]
        [DSharpPlus.CommandsNext.Attributes.Description("Signs you up for saving the amount of time you spend in games.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task LastSave(CommandContext ctx)
        {
            await ctx.RespondAsync("Last time saved was: " + bot.lastSave.ToShortTimeString());
        }

        [DSharpPlus.CommandsNext.Attributes.Command("timetillsave")]
        [DSharpPlus.CommandsNext.Attributes.Description("Signs you up for saving the amount of time you spend in games.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task TimeTillSave(CommandContext ctx)
        {
            await ctx.RespondAsync("Time till save is: " + TimespanToString((bot.lastSave.Add(bot.sparTid) - DateTime.Now)));
        }

        [DSharpPlus.CommandsNext.Attributes.Command("botcoinleaderboard")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("botcoinleader", "botcoinboard")]
        [DSharpPlus.CommandsNext.Attributes.Description("Signs you up for botcoin and tells you how many you have.")]
        public async Task BotCoinLeaderBoard(CommandContext ctx)
        {
            int i = BotCoinIndex(ctx);
            if (i > -1)
            {
                if (botCoinSaves[i].userName == null)
                {
                    botCoinSaves[i].userName = ctx.Message.Author.Username;
                }
            }
            List<BotCoinSaveData> temp = new List<BotCoinSaveData>();
            temp.InsertRange(0, botCoinSaves);
            temp = temp.OrderBy(a => a.antalBotCoin).ToList();
            temp.Reverse();
            string SendString = string.Empty;
            for (int a = 0; a < temp.Count; a++)
            {
                if (temp[a].userName == null)
                {
                    SendString += WriteLine("Botcoin: " + temp[a].antalBotCoin + " ");
                }
                else
                {
                    SendString += WriteLine("Botcoin: " + temp[a].antalBotCoin + " " + temp[a].userName);
                }
            }
            //await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            //{
            //    Title = "Leaderboard",
            //    Description = SendString,
            //});
            await WriteArrayEmbeed(SendString.Split('\n'), ctx, "Leaderboard");
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("simppointleaderboard")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("simppointleader", "simpboard", "simpleaderboard", "simpleader")]
        [DSharpPlus.CommandsNext.Attributes.Description("Signs you up for botcoin and tells you how many you have.")]
        public async Task SimpPointLeaderBoard(CommandContext ctx)
        {
            int i = BotCoinIndex(ctx);
            if (i > -1)
            {
                if (simpPointSaves[i].userName == null)
                {
                    simpPointSaves[i].userName = ctx.Message.Author.Username;
                }
            }
            List<SimpPointSaveData> temp = new List<SimpPointSaveData>();
            temp.InsertRange(0, simpPointSaves);
            temp = temp.OrderByDescending(a => a.antalSimpPoint).ToList();
            string SendString = string.Empty;
            for (int a = 0; a < temp.Count; a++)
            {
                if (temp[a].userName == null)
                {
                    SendString += WriteLine("Simppoints: " + temp[a].antalSimpPoint + " ");
                }
                else
                {
                    SendString += WriteLine("Simppoints: " + temp[a].antalSimpPoint + " " + temp[a].userName);
                }
            }
            //await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            //{
            //    Title = "Leaderboard",
            //    Description = SendString,
            //});
            await WriteArrayEmbeed(SendString.Split('\n'), ctx, "Leaderboard");
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("fullleaderboard")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("fullgameleaderboard", "fullgameleader", "fullgameboard", "allleader", "fullleader")]
        [DSharpPlus.CommandsNext.Attributes.Description("Lists everyone who is in gamesaves list ordered by playtime.")]
        public async Task FullGameLeaderboard(CommandContext ctx)
        {
            string SendString = string.Empty;
            List<UserGameSave> tempSave = new List<UserGameSave>();
            tempSave.AddRange(bot.gameSaves);
            for (int a = 0; a < tempSave.Count; a++)
            {
                tempSave[a].SetGames(tempSave[a].games.OrderByDescending(o => o.timeSpentPlaying.TotalHours).ToList());
            }
            tempSave = tempSave.OrderByDescending(o => o.games.Count > 0 ? o.games[0].timeSpentPlaying.TotalHours : 0).ToList();
            for (int a = 0; a < tempSave.Count; a++)
            {
                SendString += WriteLine(tempSave[a].user.Username + " " + tempSave[a].games.Count + " games.");
                for (int b = 0; b < tempSave[a].games.Count; b++)
                {
                    //SendString += WriteLine(tempSave[a].games[b].gameName + " has been played for " + TimespanToString(tempSave[a].games[b].timeSpentPlaying) + " by " + tempSave[a].user.Username);
                    SendString += WriteLine(TimespanToShortString(tempSave[a].games[b].timeSpentPlaying) + " " + tempSave[a].games[b].gameName);
                }
                if (tempSave[a].games.Count > 0)
                {
                    SendString += WriteLine(" ");
                }
            }
            //await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            //{
            //    Title = "Game time leaderboard",
            //    Description = SendString,
            //});
            await WriteArrayEmbeed(SendString.Split('\n'), ctx, "Game time leaderboard");
            GiveBotCoin(ctx);
        }

        //[DSharpPlus.CommandsNext.Attributes.Command("leaderboard")]
        //[DSharpPlus.CommandsNext.Attributes.Aliases("gameleader", "gameboard", "leader", "board")]
        //[DSharpPlus.CommandsNext.Attributes.Description("Signs you up for botcoin and tells you how many you have.")]
        public async Task GameLeaderboard(CommandContext ctx)
        {
            try
            {
                string SendString = string.Empty;
                List<(GameTimeSave, string)> tempSave = new List<(GameTimeSave, string)>();
                for (int i = 0; i < bot.gameSaves.Count; i++)
                {
                    for (int a = 0; a < bot.gameSaves[i].games.Count; a++)
                    {
                        tempSave.Add((bot.gameSaves[i].games[a], bot.gameSaves[i].user.Username));
                    }
                }
                //tempSave.AddRange(bot.gameSaves);
                tempSave = tempSave.OrderByDescending(o => o.Item1.timeSpentPlaying.TotalHours).ToList();
                for (int a = 0; a < tempSave.Count; a++)
                {
                    //SendString += WriteLine(tempSave[a].games[b].gameName + " has been played for " + TimespanToString(tempSave[a].games[b].timeSpentPlaying) + " by " + tempSave[a].user.Username);
                    SendString += WriteLine(TimespanToShortString(tempSave[a].Item1.timeSpentPlaying) + " " + tempSave[a].Item1.gameName + " " + tempSave[a].Item2);

                    //SendString += WriteLine(" ");
                }
                //await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                //{
                //    Title = "Game time leaderboard",
                //    Description = SendString,
                //});
                await WriteArrayEmbeed(SendString.Split('\n'), ctx, "Game time leaderboard");
                GiveBotCoin(ctx);
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message + "\n callstack: " + e.StackTrace);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("leaderboard")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("gameleader", "gameboard", "leader", "board")]
        [DSharpPlus.CommandsNext.Attributes.Description("Signs you up for botcoin and tells you how many you have.")]
        public async Task GameLeaderboard(CommandContext ctx, [RemainingText] string gamename)
        {
            if (gamename != null && gamename != string.Empty)
            {
                string SendString = string.Empty;
                List<(GameTimeSave, string)> tempSave = new List<(GameTimeSave, string)>();
                for (int i = 0; i < bot.gameSaves.Count; i++)
                {
                    for (int a = 0; a < bot.gameSaves[i].games.Count; a++)
                    {
                        if (bot.gameSaves[i].games[a].gameName.ToLower() == gamename.ToLower())
                        {
                            tempSave.Add((bot.gameSaves[i].games[a], bot.gameSaves[i].user.Username));
                        }
                    }
                }
                //tempSave.AddRange(bot.gameSaves);
                if (tempSave.Count > 0)
                {
                    tempSave = tempSave.OrderByDescending(o => o.Item1.timeSpentPlaying.TotalHours).ToList();
                    for (int a = 0; a < tempSave.Count; a++)
                    {
                        //SendString += WriteLine(tempSave[a].games[b].gameName + " has been played for " + TimespanToString(tempSave[a].games[b].timeSpentPlaying) + " by " + tempSave[a].user.Username);
                        SendString += WriteLine(TimespanToShortString(tempSave[a].Item1.timeSpentPlaying) + " " + tempSave[a].Item1.gameName + " " + tempSave[a].Item2);

                        //SendString += WriteLine(" ");
                    }
                    //await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    //{
                    //    Title = "Game time leaderboard",
                    //    Description = SendString,
                    //});
                    await WriteArrayEmbeed(SendString.Split('\n'), ctx, "Game time leaderboard");
                    GiveBotCoin(ctx);
                }
                else
                {
                    await ctx.RespondAsync("There were no savegames found called: " + gamename);
                    //await GameLeaderboard(ctx);
                }
            }
            else
            {
                //await ctx.RespondAsync("There were no savegames found called: " + gamename);
                await GameLeaderboard(ctx);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("leaderboardusersort")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("gameleaderusersort", "gameboardusersort", "leaderusersort", "boardusersort", "leaderboarduser", "gameleaderuser", "gameboarduser", "leaderuser", "boarduser")]
        [DSharpPlus.CommandsNext.Attributes.Description("Signs you up for botcoin and tells you how many you have.")]
        public async Task GameLeaderboardUserSort(CommandContext ctx)
        {
            string SendString = string.Empty;
            List<UserGameSave> tempSave = new List<UserGameSave>();
            tempSave.AddRange(bot.gameSaves);
            for (int a = 0; a < tempSave.Count; a++)
            {
                tempSave[a].SetGames(tempSave[a].games.OrderByDescending(o => o.timeSpentPlaying.TotalHours).ToList());
            }
            tempSave = tempSave.OrderByDescending(o => o.games.Count > 0 ? o.games[0].timeSpentPlaying.TotalHours : 0).ToList();
            for (int a = 0; a < tempSave.Count; a++)
            {
                if (tempSave[a].games.Count > 0)
                {
                    SendString += WriteLine(tempSave[a].user.Username + " " + tempSave[a].games.Count + " games.");
                    for (int b = 0; b < tempSave[a].games.Count; b++)
                    {
                        //SendString += WriteLine(tempSave[a].games[b].gameName + " has been played for " + TimespanToString(tempSave[a].games[b].timeSpentPlaying) + " by " + tempSave[a].user.Username);
                        SendString += WriteLine(TimespanToShortString(tempSave[a].games[b].timeSpentPlaying) + " " + tempSave[a].games[b].gameName);
                    }
                    SendString += WriteLine(" ");
                }
            }
            //await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            //{
            //    Title = "Game time leaderboard",
            //    Description = SendString,
            //});
            await WriteArrayEmbeed(SendString.Split('\n'), ctx, "Game time leaderboard");
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("onlineleaderboard")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("onlinegameleader", "onlinegameboard", "onlineleader", "onlineboard")]
        [DSharpPlus.CommandsNext.Attributes.Description("Signs you up for botcoin and tells you how many you have.")]
        public async Task OnlineGameLeaderboard(CommandContext ctx)
        {
            string SendString = string.Empty;
            List<(GameTimeSave, string)> tempSave = new List<(GameTimeSave, string)>();
            for (int i = 0; i < bot.gameSaves.Count; i++)
            {
                if (bot.IsplayingInGameSaves(bot.gameSaves[i]))
                {
                    for (int a = 0; a < bot.gameSaves[i].games.Count; a++)
                    {
                        if (bot.IsplayingInGameSaves(bot.gameSaves[i], bot.gameSaves[i].games[a]))
                        {
                            tempSave.Add((bot.gameSaves[i].games[a], bot.gameSaves[i].user.Username));
                        }
                    }
                }
            }
            //tempSave.AddRange(bot.gameSaves);
            tempSave = tempSave.OrderByDescending(o => o.Item1.timeSpentPlaying.TotalHours).ToList();
            for (int a = 0; a < tempSave.Count; a++)
            {
                //SendString += WriteLine(tempSave[a].games[b].gameName + " has been played for " + TimespanToString(tempSave[a].games[b].timeSpentPlaying) + " by " + tempSave[a].user.Username);
                SendString += WriteLine(TimespanToShortString(tempSave[a].Item1.timeSpentPlaying) + " " + tempSave[a].Item1.gameName + " " + tempSave[a].Item2);

                //SendString += WriteLine(" ");
            }
            if (SendString == null || SendString == string.Empty)
            {
                SendString += "No one is playing anything at the moment.";
            }
            //await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            //{
            //    Title = "Game time leaderboard",
            //    Description = SendString,
            //});
            await WriteArrayEmbeed(SendString.Split('\n'), ctx, "Game time leaderboard");
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("remindme")]
        [DSharpPlus.CommandsNext.Attributes.Description("Reminds you at specified time")]
        public async Task Remind(CommandContext ctx, [DSharpPlus.CommandsNext.Attributes.Description("amount of time to wait in integers")]int time, [DSharpPlus.CommandsNext.Attributes.Description("Your measurement of time.")]string measurement, [DSharpPlus.CommandsNext.Attributes.Description("Sends message at specified time. Optional")]params string[] message)
        {
            DateTime current = DateTime.Now;
            DateTime remindTime = DateTime.Now;
            try
            {
                if (measurement.ToLower() == "minutes" || measurement.ToLower() == "minute" || measurement.ToLower() == "minut" || measurement.ToLower() == "minuter")
                {
                    remindTime = remindTime.AddMinutes(time);
                }
                else if (measurement.ToLower() == "hours" || measurement.ToLower() == "hour" || measurement.ToLower() == "timme" || measurement.ToLower() == "timmar")
                {
                    remindTime = remindTime.AddHours(time);
                }
                else if (measurement.ToLower() == "seconds" || measurement.ToLower() == "second" || measurement.ToLower() == "sekund" || measurement.ToLower() == "sekunder")
                {
                    remindTime = remindTime.AddSeconds(time);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("You inputted the time measurement wrong.").ConfigureAwait(false);
                    return;
                }
                TimeSpan timeSpan = remindTime - current;
                await ctx.Channel.SendMessageAsync("Ok, will remind you at " + remindTime.ToShortTimeString() + ".").ConfigureAwait(false);
                bot.queuedRemindMes.Add(new RemindmeSave(remindTime, ctx.Message.Author.Username));
                await Task.Delay(Convert.ToInt32(timeSpan.TotalMilliseconds));
                int index = bot.queuedRemindMes.FindIndex(a => a.dateTime == remindTime);
                if (index >= 0)
                {
                    bot.queuedRemindMes.RemoveAt(index);
                }
                string temp = "";
                for (int i = 0; i < message.Length; i++)
                {
                    temp += message[i] + " ";
                }
                if (temp != " " && temp != "")
                {
                    await ctx.Channel.SendMessageAsync(ctx.User.Mention + " You told me to remind you now. Your message: " + temp + ".").ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync(ctx.User.Mention + " You told me to remind you now.").ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                int index = bot.queuedRemindMes.FindIndex(a => a.dateTime == remindTime);
                if (index >= 0)
                {
                    bot.queuedRemindMes.RemoveAt(index);
                }
                await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
            }
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("remindme")]
        [DSharpPlus.CommandsNext.Attributes.Description("Reminds you at specified time")]
        public async Task Remind(CommandContext ctx, [DSharpPlus.CommandsNext.Attributes.Description("amount of time to wait in integers")]int time, [DSharpPlus.CommandsNext.Attributes.Description("Your measurement of time.")]string measurement)
        {
            DateTime current = DateTime.Now;
            DateTime remindTime = DateTime.Now;
            try
            {
                if (measurement.ToLower() == "minutes" || measurement.ToLower() == "minute" || measurement.ToLower() == "minut" || measurement.ToLower() == "minuter")
                {
                    remindTime = remindTime.AddMinutes(time);
                }
                else if (measurement.ToLower() == "hours" || measurement.ToLower() == "hour" || measurement.ToLower() == "timme" || measurement.ToLower() == "timmar")
                {
                    remindTime = remindTime.AddHours(time);
                }
                else if (measurement.ToLower() == "seconds" || measurement.ToLower() == "second" || measurement.ToLower() == "sekund" || measurement.ToLower() == "sekunder")
                {
                    remindTime = remindTime.AddSeconds(time);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("You inputted the time measurement wrong.").ConfigureAwait(false);
                    return;
                }
                TimeSpan timeSpan = remindTime - current;
                await ctx.Channel.SendMessageAsync("Ok, will remind you at " + remindTime.ToShortTimeString() + ".").ConfigureAwait(false);
                bot.queuedRemindMes.Add(new RemindmeSave(remindTime, ctx.Message.Author.Username));
                await Task.Delay(Convert.ToInt32(timeSpan.TotalMilliseconds));
                int index = bot.queuedRemindMes.FindIndex(a => a.dateTime == remindTime);
                if (index >= 0)
                {
                    bot.queuedRemindMes.RemoveAt(index);
                }
                await ctx.Channel.SendMessageAsync(ctx.User.Mention + " You told me to remind you now.").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                int index = bot.queuedRemindMes.FindIndex(a => a.dateTime == remindTime);
                if (index >= 0)
                {
                    bot.queuedRemindMes.RemoveAt(index);
                }
                await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
            }
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("remindme")]
        [DSharpPlus.CommandsNext.Attributes.Description("Reminds you at specified time")]
        public async Task Remind(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("remindme has to have a time and measurement inputted to function.\nExample: ?remindme 30 minutes").ConfigureAwait(false);
        }

        public void SaveImage(string filename, System.Drawing.Imaging.ImageFormat format, string imageUrl)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(imageUrl);
            Bitmap bitmap; bitmap = new Bitmap(stream);

            if (bitmap != null)
            {
                bitmap.Save(filename, format);
            }

            stream.Flush();
            stream.Close();
            client.Dispose();
        }

        [DSharpPlus.CommandsNext.Attributes.Command("image2string")]
        [DSharpPlus.CommandsNext.Attributes.Description("Converts an image to a string.\nIf no image provided sends toad pics.")]
        public async Task Image2string(CommandContext ctx)
        {
            //Doesn't work yet    //List<DiscordAttachment> attachments = (List<DSharpPlus.Entities.DiscordAttachment>)ctx.Message.Attachments;
            //attachments.FindAll(x => x.GetType() == typeof(".png"))
            string SendString = "";
            Image image = Image.FromFile(tempImagePng);
            try
            {
                if (ctx.Message.Attachments.Count > 0)
                {
                    string url = ctx.Message.Attachments.FirstOrDefault().Url;
                    SaveImage(tempImagePng, System.Drawing.Imaging.ImageFormat.Png, url);
                    image = Image.FromFile(tempImagePng);
                }
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message + " callstack " + e.StackTrace).ConfigureAwait(false);
                return;
            }
            try
            {
                //image ??= Image.FromFile(@"C:\Users\gustav.juul\Pictures\GaleBackup\ToadSpriteRightJump.png");
                FrameDimension dimension = new FrameDimension(image.FrameDimensionsList[0]);

                int frameCount = image.GetFrameCount(dimension);

                int left = Console.WindowLeft, top = Console.WindowTop;

                //char[] chars = { '#', '#', '@', '%', '=', '+', 'º', ':', '-', '.' };
                string[] chars = { "█", "▓", "▒", "░" };// "   "
                image.SelectActiveFrame(dimension, 0);
                for (int h = 0; h < image.Height; h++)
                {
                    string temp = "";
                    for (int w = 0; w < image.Width; w++)
                    {
                        Color cl = ((Bitmap)image).GetPixel(w, h);
                        int gray = (cl.R + cl.G + cl.B) / 3;
                        int index = (gray * (chars.Length - 1)) / 255;
                        if (cl.Name == "0")
                        {
                            index = chars.Length - 1;
                        }
                        //if (index >= chars.Length)
                        //{
                        //    temp += string.Empty + (empty) + (empty);
                        //}
                        //else if (chars[index] == ".")
                        //{
                        //    temp += string.Empty + (chars[index]) + (chars[index]) + (chars[index]) + (chars[index]) + (chars[index]) + (chars[index]) + (chars[index]) + (chars[index]);
                        //}
                        temp += string.Empty + (chars[index]) + (chars[index]);
                    }

                    SendString += WriteLine(temp);
                }
                await ctx.Channel.SendMessageAsync(SendString).ConfigureAwait(false);
                GiveBotCoin(ctx);
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
            }
            SendString = "";
        }

        [DSharpPlus.CommandsNext.Attributes.Command("face")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("thispersondoesnotexist", "tpdne", "human")]
        [DSharpPlus.CommandsNext.Attributes.Description("Sends a generated picture from [tpdne](https://thispersondoesnotexist.com)")]
        public async Task ThisPersonDoesNotExist(CommandContext ctx)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile("https://thispersondoesnotexist.com/image", tempImagePng);
                }
                await bot.UploadFile(tempImagePng, ctx.Channel);
                GiveBotCoin(ctx);
            }
            catch (Exception ex)
            {
                await ctx.Channel.SendMessageAsync(ex.Message).ConfigureAwait(false);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("inspirobot")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("inspiroquote", "inspiro")]
        [DSharpPlus.CommandsNext.Attributes.Description("Sends a generated quote from [inspirobot](https://inspirobot.me/)")]
        public async Task InspiroCitat(CommandContext ctx)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    //client.DownloadFile(new Uri(url), @"c:\temp\image35.png");
                    string url = client.DownloadString("https://inspirobot.me/api?generate=true");
                    client.DownloadFile(url, tempImagePng);
                }
                await bot.UploadFile(tempImagePng, ctx.Channel);
                GiveBotCoin(ctx);
            }
            catch (Exception ex)
            {
                await ctx.Channel.SendMessageAsync(ex.Message + " callstack " + ex.StackTrace).ConfigureAwait(false);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("code")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("repo")]
        public async Task SendCode(CommandContext ctx, string commandName)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    //client.DownloadFile(new Uri(url), @"c:\temp\image35.png");
                    string code = client.DownloadString("https://raw.githubusercontent.com/LordGurr/DiscordBot/master/DiscordBot/AdventureCommands.cs");
                    //SaveImage(tempImagePng, System.Drawing.Imaging.ImageFormat.Png, url);
                    if (code.Contains("        [DSharpPlus.CommandsNext.Attributes.Command(\"" + commandName + "\")]"))
                    {
                        int index = code.IndexOf("        [DSharpPlus.CommandsNext.Attributes.Command(\"" + commandName + "\")]");
                        int brackets = 1;
                        int next = code.IndexOf("{", index) + 1;
                        string temp = code.Substring(index, next - index);
                        while (brackets > 0)
                        {
                            if (code.IndexOf("{", next) < code.IndexOf("}", next))
                            {
                                next = code.IndexOf("{", next) + 1;
                                brackets++;
                            }
                            else
                            {
                                next = code.IndexOf("}", next) + 1;
                                brackets--;
                            }
                            temp = code.Substring(index, next - index);
                        }
                        string sendCode = code.Substring(index, next - index);
                        string original = sendCode;
                        string[] linesToSend = sendCode.Split('\n');
                        int spacesToRemove = 0;
                        for (int i = 0; i < linesToSend[0].Length; i++)
                        {
                            if (linesToSend[0][i] == ' ')
                            {
                                spacesToRemove++;
                            }
                        }
                        sendCode = string.Empty;
                        for (int i = 0; i < linesToSend.Length; i++)
                        {
                            try
                            {
                                if (spacesToRemove < linesToSend[i].Length)
                                {
                                    linesToSend[i] = linesToSend[i].Remove(0, spacesToRemove);
                                    sendCode += linesToSend[i] + "\n";
                                }
                            }
                            catch (Exception e)
                            {
                                throw new ArgumentException(e.Message + " string " + i + ": " + linesToSend[i]);
                            }
                        }

                        if ((double)sendCode.Length / 1880 > 1.0)
                        {
                            double result = Math.Ceiling((double)sendCode.Length / 18880.0);
                            int size = (int)((double)sendCode.Length / result);
                            int latest = 0;
                            while (true)
                            {
                                string send = string.Empty;
                                //if (i + 1 >= result)
                                //{
                                //    send = sendCode.Substring(latest, sendCode.Length - latest);
                                //}
                                //else
                                //{
                                //    send = sendCode.Substring(latest, size);
                                //}
                                for (latest = latest; latest < linesToSend.Length; latest++)
                                {
                                    string tempString = send;
                                    tempString += linesToSend[latest];
                                    if (tempString.Length / 2000 >= 1.0)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        send = tempString;
                                    }
                                }
                                send = "```cs\n" + send + "\n```";
                                //sendCode += "\nResten av koden: https://github.com/LordGurr/DiscordBot/blob/master/DiscordBot/Bot.cs";
                                if (latest >= linesToSend.Length)
                                {
                                    send += "\n[Github länk till koden.](https://github.com/LordGurr/DiscordBot/blob/master/DiscordBot/Bot.cs)";
                                }

                                //await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                //{
                                //    Title = "Code snippet " + (i + 1),
                                //    Description = send,
                                //});
                                await ctx.Channel.SendMessageAsync(send).ConfigureAwait(false);
                                if (latest >= linesToSend.Length)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            sendCode = "```cs\n" + sendCode + "\n```"; //``` ```cs
                                                                       //sendCode += "\nResten av koden: https://github.com/LordGurr/DiscordBot/blob/master/DiscordBot/Bot.cs";
                            sendCode += "\n[Github länk till koden.](https://github.com/LordGurr/DiscordBot/blob/master/DiscordBot/Bot.cs)";

                            //await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                            //{
                            //    Title = "Code snippet",
                            //    Description = sendCode,
                            //});

                            await ctx.Channel.SendMessageAsync(sendCode).ConfigureAwait(false);
                        }
                        GiveBotCoin(ctx);
                    }
                    else
                    {
                        await ctx.Channel.SendMessageAsync(commandName + " doesn't seem to exist.").ConfigureAwait(false);
                    }
                    //await bot.UploadFile(tempImagePng, ctx.Channel);
                }
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
            }
        }

        private bool döperOm = false;

        //Lägger till fler brackets för att fixa koden }}
        [DSharpPlus.CommandsNext.Attributes.Command("nickname")]
        [DSharpPlus.CommandsNext.Attributes.Description("Döper om alla på servern.")]
        [DSharpPlus.CommandsNext.Attributes.RequireBotPermissions(Permissions.ManageNicknames)]
        public async Task Nickname(CommandContext ctx, [DSharpPlus.CommandsNext.Attributes.Description("Vad de ska döpas till")]string name)
        {
            if (ctx.Channel.Name == null)
            {
                await ctx.Channel.SendMessageAsync("Det här måste skickas i en server.").ConfigureAwait(false);
                return;
            }
            if (ctx.Member.Hierarchy < int.MaxValue)
            {
                var a = bot.Client.CurrentApplication.Owners.ToArray();
                bool found = false;
                for (int i = 0; i < a.Length; i++)
                {
                    if (ctx.Member.Id == a[i].Id)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    await ctx.Channel.SendMessageAsync("Du måste vara admin för att använda detta kommando.").ConfigureAwait(false);
                    return;
                }
            }
            if (!bot.stopAll)
            {
                if (!döperOm)
                {
                    döperOm = true;
                    try
                    {
                        List<DiscordMember> members = new List<DiscordMember>();
                        if (ctx.Message.MentionedUsers.Count > 0)
                        {
                            var ids = ctx.Message.MentionedUsers.ToList();
                            int[] kanalIndex = ChannelIndex(ctx);
                            if (kanalIndex[0] >= 0)
                            {
                                for (int i = 0; i < kanalerna[kanalIndex[0]].discordUsers.Count; i++)
                                {
                                    if (ids.Any(a => a.Id == kanalerna[kanalIndex[0]].discordUsers[i].user))
                                    {
                                        members.Add(kanalerna[kanalIndex[0]].discordUsers[i].member);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //List<DiscordMember> temp = ctx.Guild.Members.Values.ToList();
                            //var guildStuff = ctx.Guild.GetAllMembersAsync();

                            //temp.AddRange(guildStuff.Result.ToList());
                            //DiscordMemberConverter converter = new DiscordMemberConverter();
                            int[] kanalIndex = ChannelIndex(ctx);
                            if (kanalIndex[0] >= 0)
                            {
                                for (int i = 0; i < kanalerna[kanalIndex[0]].discordUsers.Count; i++)
                                {
                                    members.Add(kanalerna[kanalIndex[0]].discordUsers[i].member);
                                }
                                //    //for (int i = 0; i < temp.Count; i++)
                                //    //{
                                //    //    if (!kanalerna[kanalIndex[0]].discordUsers.Any(a => a.user == temp[i].Id))
                                //    //    {
                                //    //        kanalerna[kanalIndex[0]].discordUsers.Add(new DiscordMemberSaveData(temp[i]));
                                //    //    }
                                //    //}
                                //    for (int i = 0; i < kanalerna[kanalIndex[0]].discordUsers.Count; i++)
                                //    {
                                //        bool found = false;
                                //        for (int a = 0; a < members.Count; a++)
                                //        {
                                //            if (members[a] == kanalerna[kanalIndex[0]].discordUsers[i].member)
                                //            {
                                //                found = true;
                                //                break;
                                //            }
                                //        }
                                //        if (!found)
                                //        {
                                //            members.Add(kanalerna[kanalIndex[0]].discordUsers[i].member);
                                //        }
                                //    }
                                //}
                                //for (int i = 0; i < temp.Count; i++)
                                //{
                                //    if (!members.Contains(temp[i]))
                                //    {
                                //        members.Add(temp[i]);
                                //    }
                            }
                        }
                        int membersModified = 0;
                        int membersNotAbleToModify = 0;
                        for (int i = 0; i < members.Count; i++)
                        {
                            try
                            {
                                await members[i].ModifyAsync(u => u.Nickname = name);
                                membersModified++;
                            }
                            catch (Exception e)
                            {
                                await WriteLine("Försökte ändra smeknamn: " + e.Message + " på " + members[i].Username, ctx);
                                membersNotAbleToModify++;
                            }
                        }
                        if (membersNotAbleToModify > 0)
                        {
                            await ctx.Channel.SendMessageAsync(membersModified + " medlemmars smeknamn har ändrats till " + name + ".\n" + membersNotAbleToModify + " av medlammarna kunde inte döpas om.").ConfigureAwait(false);
                        }
                        else
                        {
                            await ctx.Channel.SendMessageAsync(membersModified + " medlemmars smeknamn har ändrats till " + name + ".").ConfigureAwait(false);
                        }
                        döperOm = false;
                    }
                    catch (Exception e)
                    {
                        await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
                        döperOm = false;
                    }
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("Döper redan om. Skicka senare.").ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("bot shutting down.").ConfigureAwait(false);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("nickname")]
        [DSharpPlus.CommandsNext.Attributes.Description("Döper om alla på servern.")]
        [DSharpPlus.CommandsNext.Attributes.RequireBotPermissions(Permissions.ManageNicknames)]
        public async Task Nickname(CommandContext ctx)
        {
            if (ctx.Channel.Name == null)
            {
                await ctx.Channel.SendMessageAsync("Det här måste skickas i en server.").ConfigureAwait(false);
                return;
            }
            if (ctx.Member.Hierarchy < int.MaxValue)
            {
                var a = bot.Client.CurrentApplication.Owners.ToArray();
                bool found = false;
                for (int i = 0; i < a.Length; i++)
                {
                    if (ctx.Member.Id == a[i].Id)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    await ctx.Channel.SendMessageAsync("Du måste vara admin för att använda detta kommando.").ConfigureAwait(false);
                    return;
                }
            }
            await ctx.Channel.SendMessageAsync("nickname behöver en string med namnet medlemmar ska döpas till.\nExempel: ?nickname kyckling").ConfigureAwait(false);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("math")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("matte")]
        [DSharpPlus.CommandsNext.Attributes.Description("Löser simpla matematiska uttryck.\nFunkar inte med variabler och bara med heltal.\n kräver * före och efter parentes.\nExempel: ?math (2*2)^8")]
        public async Task MathExpress(CommandContext ctx, [RemainingText] string result)
        {
            //string result = ctx.Message.Content.Replace("?math", "");
            try
            {
                double answer = Eval(result);
                await ctx.Channel.SendMessageAsync("Result: " + answer.ToString()).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
            }
            GiveBotCoin(ctx);
        }

        private const double champ = 0.123456789101112;
        private const double conway = 1.303577269034296;
        private const double phi = 1.618033988749894;
        public static float Rad2Deg = 360 / ((float)Math.PI * 2);
        public static float Deg2Rad = ((float)Math.PI * 2) / 360;

        private static string[] _operators = { "-", "+", "/", "*", "^", "%" };

        private static string[] _singleOperators = { "!" };

        private static string[] mathConst = { "e", "pi", "tau", "champerowne", "champ", "conway", "phi", "rad2deg", "deg2rad" };

        private static double[] actMathConst = { Math.E, Math.PI, Math.PI * 2, champ, champ, conway, phi, Rad2Deg, Deg2Rad };

        private static string[] preParenthesis = { "sin^-1", "cos^-1", "tan^-1", "sin", "cos", "tan", "abs", "sqr", "log", "round", "ln" };

        private static Func<double, double, double>[] _operations = {
        (a1, a2) => a1 - a2,
        (a1, a2) => a1 + a2,
        (a1, a2) => a1 / a2,
        (a1, a2) => a1 * a2,
        (a1, a2) => Math.Pow(a1, a2),
        (a1, a2) => a1 % a2,
        //(a1, a2) => factorialDouble(a1),
    };

        private static Func<double, double>[] preParenthesisOperation = {
        (a1) => Math.Asin(a1)*Rad2Deg,
        (a1) => Math.Acos(a1)*Rad2Deg,
        (a1) => Math.Atan(a1)*Rad2Deg,
        (a1) => Math.Sin(a1*Deg2Rad),
        (a1) => Math.Cos(a1*Deg2Rad),
        (a1) => Math.Tan(a1*Deg2Rad),
        (a1) => Math.Abs(a1),
        (a1) => Math.Sqrt(a1),
        (a1) => Math.Log10(a1),
        (a1) => Math.Round(a1, MidpointRounding.ToPositiveInfinity),
        (a1) => Math.Log(a1),
        };

        private static Func<double, double>[] _singleOperations = {
        (a1) => factorialDouble(a1),
        //(a1, a2) => factorialDouble(a1),
        };

        public static double Eval(string expression)
        {
            expression = expression.Replace('.', ',');
            List<string> tokens = getTokens(expression);
            Stack<double> operandStack = new Stack<double>();
            Stack<string> operatorStack = new Stack<string>();
            int tokenIndex = 0;
            MakeEverySecondOperator(tokens);

            while (tokenIndex < tokens.Count)
            {
                string token = tokens[tokenIndex];
                bool usedPreParen = false;
                if (preParenthesis.Any(a => a == token))
                {
                    if (tokens[tokenIndex + 1] != "(")
                    {
                        if (tokens[tokenIndex + 1] == "*")
                        {
                            tokens.RemoveAt(tokenIndex + 1);
                            if (tokens[tokenIndex + 1] != "(")
                            {
                                throw new ArgumentException("A pre parenthesis operation has to be followed by a parenthesis.");
                            }
                        }
                        else
                        {
                            throw new ArgumentException("A pre parenthesis operation has to be followed by a parenthesis.");
                        }
                    }
                    int index = preParenthesis.ToList().FindIndex(a => a == token);
                    tokenIndex++;
                    string subExpr = getSubExpression(tokens, ref tokenIndex);
                    operandStack.Push(preParenthesisOperation[Array.IndexOf(preParenthesis, token)](Eval(subExpr)));
                    continue;
                }
                if (token == "(")
                {
                    string subExpr = getSubExpression(tokens, ref tokenIndex);
                    operandStack.Push(Eval(subExpr));
                    continue;
                }
                if (token == ")")
                {
                    throw new ArgumentException("Mis-matched parentheses in expression");
                }
                //If this is an operator
                if (Array.IndexOf(_operators, token) >= 0 || Array.IndexOf(_singleOperators, token) >= 0)
                {
                    while (operatorStack.Count > 0 && Array.IndexOf(_operators, token) < Array.IndexOf(_operators, operatorStack.Peek()))
                    {
                        string op = operatorStack.Pop();
                        double arg2 = operandStack.Pop();
                        if (_singleOperators.Any(a => a == op))
                        {
                            operandStack.Push(_singleOperations[Array.IndexOf(_singleOperators, op)](arg2));
                        }
                        else
                        {
                            if (operandStack.Count > 0 || op != "-")
                            {
                                double arg1 = operandStack.Pop();
                                operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
                            }
                            else
                            {
                                operandStack.Push(-arg2);
                            }
                        }
                    }
                    operatorStack.Push(token);
                }
                else
                {
                    if (IsDigitsOnly(token, ","))
                    {
                        operandStack.Push(Convert.ToDouble(token));
                    }
                    else
                    {
                        operandStack.Push(ChooseConst(token));
                    }
                }
                tokenIndex += 1;
            }

            while (operatorStack.Count > 0)
            {
                string op = operatorStack.Pop();
                double arg2 = operandStack.Pop();
                if (_singleOperators.Any(a => a == op))
                {
                    operandStack.Push(_singleOperations[Array.IndexOf(_singleOperators, op)](arg2));
                }
                else
                {
                    if (operandStack.Count > 0 || op != "-")
                    {
                        double arg1 = operandStack.Pop();
                        operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
                    }
                    else
                    {
                        operandStack.Push(-arg2);
                    }
                }
            }
            return operandStack.Pop();
        }

        private static string getSubExpression(List<string> tokens, ref int index)
        {
            StringBuilder subExpr = new StringBuilder();
            int parenlevels = 1;
            index += 1;
            while (index < tokens.Count && parenlevels > 0)
            {
                string token = tokens[index];
                if (tokens[index] == "(")
                {
                    parenlevels += 1;
                }

                if (tokens[index] == ")")
                {
                    parenlevels -= 1;
                }

                if (parenlevels > 0)
                {
                    subExpr.Append(token);
                }

                index += 1;
            }

            if ((parenlevels > 0))
            {
                throw new ArgumentException("Mis-matched parentheses in expression");
            }
            return subExpr.ToString();
        }

        private static List<string> getTokens(string expression)
        {
            List<string> allOperators = _operators.ToList();
            allOperators.AddRange(_singleOperators);
            string operators = string.Join("", allOperators.ToArray()); //"()^*/+-%";
            List<string> tokens = new List<string>();
            StringBuilder sb = new StringBuilder();
            string newExpress = expression.Replace(" ", string.Empty);
            for (int i = 0; i < newExpress.Length; i++)
            {
                char c = newExpress[i];
                if (operators.IndexOf(c) >= 0)
                {
                    if ((sb.Length > 0))
                    {
                        tokens.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    tokens.Add(c.ToString());
                    continue;
                }
                else if (c == '(' && i > 0 && IsDigitsOnly(newExpress[i - 1].ToString(), string.Empty) || c == ')' && i + 1 < newExpress.Length && IsDigitsOnly(newExpress[i + 1].ToString(), string.Empty))
                {
                    if (c == '(' && i > 0 && IsDigitsOnly(newExpress[i - 1].ToString(), string.Empty))
                    {
                        if ((sb.Length > 0))
                        {
                            tokens.Add(sb.ToString());
                            sb.Length = 0;
                        }
                        tokens.Add("*");
                        tokens.Add(c.ToString());
                    }
                    else
                    {
                        if ((sb.Length > 0))
                        {
                            tokens.Add(sb.ToString());
                            sb.Length = 0;
                        }
                        tokens.Add(c.ToString());
                        tokens.Add("*");
                    }
                    continue;
                }
                if (c == '(' || c == ')')
                {
                    if ((sb.Length > 0))
                    {
                        tokens.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    if (i > 0 && newExpress[i - 1] == ')' && c == '(')
                    {
                        tokens.Add("*");
                    }
                    tokens.Add(c.ToString());
                }
                else if (!IsDigitsOnly(c.ToString(), string.Empty) && IsFirstLetterOfConstant(c, newExpress, i))
                {
                    if ((sb.Length > 0))
                    {
                        tokens.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    int constantsLength = ConstantLengthFrom(c, newExpress, i);
                    tokens.Add(newExpress.Substring(i, constantsLength));
                    i += constantsLength - 1;
                }
                else if (operators.IndexOf(c) < 0)
                {
                    sb.Append(c);
                }
            }

            if ((sb.Length > 0))
            {
                tokens.Add(sb.ToString());
            }
            return tokens;
        }

        private static double ChooseConst(string str)
        {
            int index = mathConst.ToList().FindIndex(a => a.Contains(str.ToLower()));
            if (index >= 0)
            {
                //if (str.ToLower() == "e")
                //{
                //    return Math.E;
                //}
                //else if (str.ToLower() == "pi")
                //{
                //    return Math.PI;
                //}
                //else if (str.ToLower() == "tau")
                //{
                //    return 2 * Math.PI;
                //}
                //else if (str.ToLower().Contains("champ"))
                //{
                //    return champ;
                //}
                //else if (str.ToLower().Contains("conway"))
                //{
                //    return conway;
                //}
                for (int i = index; i < mathConst.Length; i++)
                {
                    if (str.ToLower() == mathConst[i])
                    {
                        return actMathConst[i];
                    }
                }
            }
            throw new Exception("No mathematical constant matches: " + str);
        }

        private static int ConstantLengthFrom(char c, string input, int index)
        {
            for (int i = 0; i < mathConst.Length; i++)
            {
                if (c == mathConst[i][0] && input.Substring(index).ToLower().Contains(mathConst[i]))
                {
                    return mathConst[i].Length;
                }
            }
            for (int i = 0; i < preParenthesis.Length; i++)
            {
                if (c == preParenthesis[i][0] && input.Substring(index).ToLower().Contains(preParenthesis[i]))
                {
                    return preParenthesis[i].Length;
                }
            }
            return 0;
        }

        private static bool IsFirstLetterOfConstant(char c, string input, int index)
        {
            for (int i = 0; i < mathConst.Length; i++)
            {
                if (c == mathConst[i][0] && input.Substring(index).ToLower().Contains(mathConst[i]))
                {
                    return true;
                }
            }
            for (int i = 0; i < preParenthesis.Length; i++)
            {
                if (c == preParenthesis[i][0] && input.Substring(index).ToLower().Contains(preParenthesis[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private static void MakeEverySecondOperator(List<string> tokens)
        {
            List<string> allOperators = _operators.ToList();
            allOperators.AddRange(_singleOperators);
            //allOperators.Add("(");
            //allOperators.Add(")");
            string operators = string.Join("", allOperators.ToArray()); //"()^*/+-%";
            for (int i = 0; i < tokens.Count; i++)
            {
                if (i > 0)
                {
                    if ((i < 1 || operators.IndexOf(tokens[i - 1]) < 0) && (i + 1 >= tokens.Count && tokens[i - 1] != "(" && tokens[i] == "(" ? !preParenthesis.Any(a => a == tokens[i - 1]) : true || tokens[i - 1] != "(" && (tokens[i] == "(" ? !preParenthesis.Any(a => a == tokens[i - 1]) : true) && operators.IndexOf(tokens[i]) < 0 && (i + 1 < tokens.Count ? operators.IndexOf(tokens[i + 1]) < 0 && tokens[i + 1] != ")" : true)) && tokens[i - 1] != "(" && tokens[i] != "(" && tokens[i] != ")" && (i + 1 < tokens.Count ? tokens[i + 1] != ")" && operators.IndexOf(tokens[i + 1]) < 0 : true) && operators.IndexOf(tokens[i]) < 0)
                    {
                        //if (tokens[i - 1] != "(" && tokens[i] != "(" && operators.IndexOf(tokens[i]) < 0)
                        //{
                        //    if (i + 1 < tokens.Count)
                        //    {
                        //        if (tokens[i + 1] != ")" && operators.IndexOf(tokens[i + 1]) < 0)
                        //            tokens.Insert(i, "*");
                        //    }
                        //    else
                        //    {
                        //        tokens.Insert(i, "*");
                        //    }
                        //}
                        tokens.Insert(i, "*");
                    }
                }
                //return tokens;
            }
        }

        private static double factorialDouble(double d)
        {
            if (d == 0.0)
            {
                return 1.0;
            }

            double abs = Math.Abs(d);
            double decimalen = abs - Math.Floor(abs);
            double result = 1.0;

            for (double i = Math.Floor(abs); i > decimalen; --i)
            {
                result *= (i + decimalen);
            }
            if (d < 0.0)
            {
                result = -result;
            }

            return result;
        }

        [DSharpPlus.CommandsNext.Attributes.Command("rockpaperscissor")]
        [DSharpPlus.CommandsNext.Attributes.Description("Returnerar sten, sax eller påse på slumpmässigt sätt.")]
        public async Task StenSax(CommandContext ctx)
        {
            int answer = rng.Next(3);
            string[] svar = new string[3] { "Sten", "Sax", "Påse" };
            await ctx.Channel.SendMessageAsync(svar[answer]).ConfigureAwait(false);
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("flipcoin")]
        [DSharpPlus.CommandsNext.Attributes.Description("Returnerar krona eller klave på slumpmässigt sätt.")]
        public async Task Coin(CommandContext ctx, string guess)
        {
            int answer = rng.Next(2);
            string[] svar = new string[2] { "Klave", "Krona" };
            await ctx.Channel.SendMessageAsync(svar[answer]).ConfigureAwait(false);
            if (guess.ToLower() == svar[answer].ToLower())
            {
                int added = AddBotCoins(ctx, true);
                await ctx.Channel.SendMessageAsync("Du gissade rätt och fick " + added + " botcoin.").ConfigureAwait(false);
            }
            else
            {
                await ctx.Channel.SendMessageAsync("Du gissade fel...").ConfigureAwait(false);
            }
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("flipCoin")]
        [DSharpPlus.CommandsNext.Attributes.Description("Returnerar krona eller klave på slumpmässigt sätt.")]
        public async Task Coin(CommandContext ctx)
        {
            int answer = rng.Next(2);
            string[] svar = new string[2] { "Klave", "Krona" };
            await ctx.Channel.SendMessageAsync(svar[answer]).ConfigureAwait(false);
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("adventure")]
        [DSharpPlus.CommandsNext.Attributes.Description("Let's you play a simple minigame.")]
        public async Task Adven(CommandContext ctx, [DSharpPlus.CommandsNext.Attributes.Description("Input which direction you want to move in. up, down, left, right. Example: ?adventure right")]string direction)
        {
            int playerIndex = -1;
            for (int i = 0; i < saveData.Count; i++)
            {
                if (ctx.User.Id == saveData[i].user)
                {
                    playerIndex = i;
                    break;
                }
            }
            if (playerIndex < 0)
            {
                List<string> fyrkanten = new List<string>();
                int[] position = new int[2];
                int[] boxend = new int[2];
                saveData.Add(new AdventureSaveData(ctx.User.Id, fyrkanten, position, boxend));
                playerIndex = saveData.Count - 1;
            }
            else if (direction == "up")
            {
                await Up(ctx);
                return;
            }
            else if (direction == "down")
            {
                await Down(ctx);
                return;
            }
            else if (direction == "right")
            {
                await Right(ctx);
                return;
            }
            else if (direction == "left")
            {
                await Left(ctx);
                return;
            }
            await ctx.Channel.SendMessageAsync(Spelet(1, " · ", saveData[playerIndex])).ConfigureAwait(false);
            //Console.WriteLine("\n\n" + ctx.User.Locale);
            GiveBotCoin(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("adventure")]
        [DSharpPlus.CommandsNext.Attributes.Description("Let's you play a simple minigame.")]
        public async Task Adven(CommandContext ctx)
        {
            int playerIndex = -1;
            for (int i = 0; i < saveData.Count; i++)
            {
                if (ctx.User.Id == saveData[i].user)
                {
                    playerIndex = i;
                    break;
                }
            }
            if (playerIndex < 0)
            {
                List<string> fyrkanten = new List<string>();
                int[] position = new int[2];
                int[] boxend = new int[2];
                saveData.Add(new AdventureSaveData(ctx.User.Id, fyrkanten, position, boxend));
                playerIndex = saveData.Count - 1;
            }

            await ctx.Channel.SendMessageAsync(Spelet(1, " · ", saveData[playerIndex])).ConfigureAwait(false);

            //Console.WriteLine("\n\n" + ctx.User.Locale);
        }

        //[DSharpPlus.CommandsNext.Attributes.Command("up")]
        public async Task Up(CommandContext ctx)
        {
            int playerIndex = -1;
            for (int i = 0; i < saveData.Count; i++)
            {
                if (ctx.User.Id == saveData[i].user)
                {
                    playerIndex = i;
                    break;
                }
            }
            if (playerIndex < 0 || saveData[playerIndex].position[0] == 0)
            {
                await ctx.Channel.SendMessageAsync("Please start game using ?adventure first").ConfigureAwait(false);
                return;
            }
            else if (saveData[playerIndex].position[1] - 1 > 0)
            {
                saveData[playerIndex].position[1]--;
            }
            if (saveData[playerIndex].position[0] > 0)
            {
                await ctx.Channel.SendMessageAsync(Spelet(1, " · ", saveData[playerIndex])).ConfigureAwait(false);
            }
        }

        //AdventureSaveData MoveUp(CommandContext ctx)
        // {
        //     int playerIndex = -1;
        //     for (int i = 0; i < saveData.Count; i++)
        //     {
        //         if (ctx.User.Id == saveData[i].user)
        //         {
        //             playerIndex = i;
        //             break;
        //         }
        //     }
        //     if (playerIndex < 0 || saveData[playerIndex].position[0] == 0)
        //     {
        //          ctx.Channel.SendMessageAsync("Please start game using ?adventure first").ConfigureAwait(false);
        //         return saveData[playerIndex];
        //     }
        //     else if (saveData[playerIndex].position[1] - 1 > 0)
        //     {
        //         saveData[playerIndex].position[1]--;
        //     }
        //     return saveData[playerIndex];
        // }

        //[DSharpPlus.CommandsNext.Attributes.Command("down")]
        public async Task Down(CommandContext ctx)
        {
            int playerIndex = -1;
            for (int i = 0; i < saveData.Count; i++)
            {
                if (ctx.User.Id == saveData[i].user)
                {
                    playerIndex = i;
                    break;
                }
            }
            if (playerIndex < 0 || saveData[playerIndex].position[0] == 0)
            {
                await ctx.Channel.SendMessageAsync("Please start game using ?adventure first").ConfigureAwait(false);
                return;
            }
            else if (saveData[playerIndex].position[1] + 1 < saveData[playerIndex].boxend[1])
            {
                saveData[playerIndex].position[1]++;
            }

            if (saveData[playerIndex].position[0] > 0)
            {
                await ctx.Channel.SendMessageAsync(Spelet(1, " · ", saveData[playerIndex])).ConfigureAwait(false);
            }
        }

        //[DSharpPlus.CommandsNext.Attributes.Command("right")]
        public async Task Right(CommandContext ctx)
        {
            int playerIndex = -1;
            for (int i = 0; i < saveData.Count; i++)
            {
                if (ctx.User.Id == saveData[i].user)
                {
                    playerIndex = i;
                    break;
                }
            }
            if (playerIndex < 0 || saveData[playerIndex].position[0] == 0)
            {
                await ctx.Channel.SendMessageAsync("Please start game using ?adventure first").ConfigureAwait(false);
                return;
            }
            else if (saveData[playerIndex].position[0] + 5 < saveData[playerIndex].fyrkanten[saveData[playerIndex].position[1]].Length)
            {
                saveData[playerIndex].position[0] += 3;
            }
            else
            {
                saveData[playerIndex].position[0] = saveData[playerIndex].fyrkanten[saveData[playerIndex].position[1]].Length - 2;
            }
            if (saveData[playerIndex].position[0] > 0)
            {
                await ctx.Channel.SendMessageAsync(Spelet(1, " · ", saveData[playerIndex])).ConfigureAwait(false);
            }
        }

        //[DSharpPlus.CommandsNext.Attributes.Command("left")]
        public async Task Left(CommandContext ctx)
        {
            int playerIndex = -1;
            for (int i = 0; i < saveData.Count; i++)
            {
                if (ctx.User.Id == saveData[i].user)
                {
                    playerIndex = i;
                    break;
                }
            }
            if (playerIndex < 0 || saveData[playerIndex].position[0] == 0)
            {
                await ctx.Channel.SendMessageAsync("Please start game using ?adventure first").ConfigureAwait(false);
                return;
            }
            else if (saveData[playerIndex].position[0] - 3 > 0)
            {
                saveData[playerIndex].position[0] -= 3;
            }
            if (saveData[playerIndex].position[0] > 0)
            {
                await ctx.Channel.SendMessageAsync(Spelet(1, " · ", saveData[playerIndex])).ConfigureAwait(false);
            }
        }

        private string Spelet(int Form, string Ground, AdventureSaveData save) //Ritar en box.)
        {
            string returnString = "";
            if (save.position[0] == 0)
            {
                //boxend[0] = EndPointX;
                //boxend[1] = EndPointY;
                save.position[0] = rng.Next(1, save.boxend[0]);
                save.position[1] = rng.Next(1, save.boxend[1]);
                //int offset = 0;

                for (int a = 0; a < save.boxend[1] + 1; a++)
                {
                    save.fyrkanten.Add("");
                    for (int i = 0; i < save.boxend[0] + 1; i++)
                    {
                        if (a >= 0 && i >= 0)
                        {
                            //Console.SetCursorPosition(i, a);
                            if (i == 0 || i == save.boxend[0] || a == 0 || a == save.boxend[1])
                            {
                                if (Form == 1) //Om form är 1 ska den rita den rita den här varje gång
                                {
                                    if (a > 0 && a < save.boxend[1] && i > 0)
                                    {
                                        for (int b = 0; b < save.boxend[0] / 15; b++)
                                        {
                                            save.fyrkanten[a] += (Ground);
                                            //returnString += (Ground);
                                        }
                                    }
                                    save.fyrkanten[a] += ("█");
                                    //returnString += ("█");
                                }
                                else if (Form == 2) //Annars använd de här fina karaktärerna så att det ser snyggare ut
                                {
                                    if (a > 0 && a < save.boxend[1] && i > 0)
                                    {
                                        for (int b = 0; b < save.boxend[0] / 15; b++)
                                        {
                                            save.fyrkanten[a] += (Ground);
                                            //returnString += (Ground);
                                        }
                                    }
                                    if (i == 0 && a == 0)
                                    {
                                        save.fyrkanten[a] += ("┌");
                                        //returnString += ("┌");
                                    }
                                    else if (i == save.boxend[0] && a == 0)
                                    {
                                        save.fyrkanten[a] += ("┐");
                                        //returnString += ("┐");
                                    }
                                    else if (i == 0 && a == save.boxend[1])
                                    {
                                        save.fyrkanten[a] += ("└");
                                        //returnString += ("└");
                                    }
                                    else if (i == save.boxend[0] && a == save.boxend[1])
                                    {
                                        save.fyrkanten[a] += ("┘");
                                        //returnString += ("┘");
                                    }
                                    else if (a == 0 || a == save.boxend[1])
                                    {
                                        save.fyrkanten[a] += ("─");
                                        //returnString += ("─");
                                    }
                                    else
                                    {
                                        save.fyrkanten[a] += ("│");
                                        //returnString += ("│");
                                    }
                                }
                            }
                            else //Annars ritar den marken
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                save.fyrkanten[a] += (Ground);
                                //returnString += (Ground);
                                //if (offset > 3)
                                //{
                                //    returnString += " ";
                                //    offset = 0;
                                //}
                                Console.ForegroundColor = ConsoleColor.White;
                                //offset++;
                            }
                        }
                    }

                    //returnString += "\n";
                }
                save.fyrkanten[0] += "";
            }

            WritePlayer(Ground, save);
            for (int i = 0; i < save.fyrkanten.Count; i++)
            {
                returnString += save.fyrkanten[i] + "\n";
            }
            Console.WriteLine(returnString);
            return returnString;
        }

        private void WritePlayer(string Ground, AdventureSaveData save)
        {
            if (save.fyrkanten.Count > 1)
            {
                int[] previousPos = new int[2] { -1, -1 };
                for (int i = 0; i < save.fyrkanten.Count; i++)
                {
                    previousPos[0] = save.fyrkanten[i].IndexOf('X');
                    if (previousPos[0] > 0)
                    {
                        string removeTemp = save.fyrkanten[i];
                        removeTemp = removeTemp.Remove(previousPos[0], 1);
                        if (i != save.position[1])
                        {
                            for (int a = 0; a < 1; a++)
                            {
                                removeTemp = removeTemp.Insert(previousPos[0], Ground);
                            }
                        }
                        save.fyrkanten[i] = removeTemp;
                        previousPos[1] = i;
                        previousPos[0] += 3;
                        break;
                    }
                }
                string temp = save.fyrkanten[save.position[1]];
                if (previousPos[1] != save.position[1])
                {
                    temp = temp.Remove(save.position[0] > 6 ? save.position[0] - 4 : save.boxend[0] - 2, 3);
                }
                temp = temp.Insert(save.position[0] + (save.boxend[0] / 15), "X");
                Console.WriteLine(temp);
                save.fyrkanten[save.position[1]] = temp;
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("anim")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("animation")]
        [DSharpPlus.CommandsNext.Attributes.Description("Gör en sexig text animation")]
        public async Task Anim(CommandContext ctx, [RemainingText] string input)
        {
            if (input != null && input != string.Empty)
            {
                string[] sendStrings = SplitMessage(TextAnimation(input), 2000);
                for (int i = 0; i < sendStrings.Length; i++)
                {
                    await ctx.Channel.SendMessageAsync(sendStrings[i]).ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("Please send a string after command.");
            }
            GiveBotCoin(ctx);
        }

        private static string[] SplitMessage(string[] input, int maxLength)
        {
            string temp = string.Empty;
            List<string> returnList = new List<string>();
            int index = 0;
            while (index < input.Length)
            {
                if (temp.Length + input[index].Length < maxLength)
                {
                    temp += input[index] + "\n";
                    index++;
                }
                else
                {
                    returnList.Add(temp);
                    temp = string.Empty;
                }
            }
            if (temp != string.Empty)
            {
                returnList.Add(temp);
            }
            return returnList.ToArray();
        }

        private static string[] TextAnimation(string input)
        {
            MyConsole myConsole = new MyConsole();
            for (int i = 0; i < 20; i++)
            {
                myConsole.WriteLine(input);
            }
            int curPos = 0;
            for (int a = 0; a < 2; a++)
            {
                //Move to the Right
                Move(ref curPos, 20, input, false, myConsole);

                //Move to the left
                Move(ref curPos, 20, input, true, myConsole);
            }
            char[] characters = input.ToCharArray();
            int maxPos = (int)Math.Round(3.75 * characters.Length - 1, MidpointRounding.ToZero);
            int[] pos = new int[characters.Length];
            for (int i = 0; i < pos.Length; i++)
            {
                pos[i] = i * 3;
            }
            // Cha cha to the right
            for (int i = 0; i < 7.5 * characters.Length; i++)
            {
                for (int a = 0; a < characters.Length; a++)
                {
                    int spaces = pos[a] - characters.Length + i - characters.Length * 2;
                    spaces = spaces < a ? a : spaces;
                    spaces = spaces + characters.Length - a - 1 >= maxPos ? maxPos - (characters.Length - a) : spaces;
                    myConsole.SetCursorPosition(spaces, myConsole.CursorTop);
                    myConsole.Write(characters[a]);
                }
                myConsole.WriteLine();
            }
            // Cha cha to the left
            for (int i = (int)Math.Round(7.5 * characters.Length - 1, MidpointRounding.ToZero); i > -1; i--)
            {
                for (int a = 0; a < characters.Length; a++)
                {
                    int spaces = pos[a] - characters.Length + i - characters.Length * 2;
                    if (spaces < a)
                    {
                    }
                    spaces = spaces < a ? a : spaces;
                    if (spaces >= maxPos)
                    {
                    }
                    spaces = spaces + characters.Length - a - 1 >= maxPos ? maxPos - (characters.Length - a) : spaces;
                    myConsole.SetCursorPosition(spaces, myConsole.CursorTop);
                    myConsole.Write(characters[a]);
                }
                myConsole.WriteLine();
            }
            // Move to the right
            curPos = 0;
            Move(ref curPos, 15, input, false, myConsole);

            // Wiggle in the middle
            string[] splitted = SplitTheString(input, 3);
            int[] positions = new int[3];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = curPos;
                for (int a = 0; a < i; a++)
                {
                    positions[i] += splitted[a].Length;
                }
            }
            for (int i = 0; i < 10; i++)
            {
                for (int a = 0; a < splitted.Length; a++)
                {
                    if (a == 0)
                    {
                        positions[a]--;
                    }
                    else if (a == 2)
                    {
                        positions[a]++;
                    }
                    myConsole.SetCursorPosition(positions[a], myConsole.CursorTop);
                    myConsole.Write(splitted[a]);
                }
                myConsole.WriteLine();
            }
            bool movingLeft = false;
            int original = positions[1];
            for (int i = 0; i < 30; i++)
            {
                if (positions[1] > positions[2] - splitted[1].Length - 4)
                {
                    movingLeft = true;
                }
                else if (positions[1] < positions[0] + splitted[0].Length + 4)
                {
                    movingLeft = false;
                }
                positions[1] += movingLeft ? -1 : 1;
                for (int a = 0; a < splitted.Length; a++)
                {
                    myConsole.SetCursorPosition(positions[a], myConsole.CursorTop);
                    myConsole.Write(splitted[a]);
                }
                myConsole.WriteLine();
            }
            for (int i = 0; i < 10; i++)
            {
                for (int a = 0; a < splitted.Length; a++)
                {
                    if (a == 0)
                    {
                        positions[a]++;
                    }
                    else if (a == 2)
                    {
                        positions[a]--;
                    }
                    else if (a == 1 && positions[a] != original)
                    {
                        if (original < positions[a])
                        {
                            positions[a]--;
                        }
                        else
                        {
                            positions[a]++;
                        }
                    }
                    myConsole.SetCursorPosition(positions[a], myConsole.CursorTop);
                    myConsole.Write(splitted[a]);
                }
                myConsole.WriteLine();
            }
            // Move to the left
            myConsole.SetCursorPosition(curPos, myConsole.CursorTop);
            myConsole.WriteLine(input);
            Move(ref curPos, 15, input, true, myConsole);

            // Offsets!
            string offsetted = input;
            int timesToRun = GetNearestMultiple(20, input.Length);
            int offset = 1 * (rng.Next(0, 2) == 0 ? 1 : -1);
            for (int i = 0; i < timesToRun; i++)
            {
                offsetted = Offset(offsetted, offset);
                myConsole.WriteLine(offsetted);
            }
            // Move to the right
            Move(ref curPos, 15, input, false, myConsole);
            // Twist around
            splitted = SplitTheString(input, 2);
            positions = new int[2];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = curPos;
                for (int a = 0; a < i; a++)
                {
                    positions[i] += splitted[a].Length;
                }
            }
            movingLeft = true;
            int[] originalPos = new int[positions.Length];
            for (int i = 0; i < originalPos.Length; i++)
            {
                originalPos[i] = positions[i];
            }
            for (int i = 0; i < 50; i++)
            {
                if (positions[0] < 1)
                {
                    movingLeft = false;
                }
                else if (positions[1] < 1)
                {
                    movingLeft = true;
                }
                for (int a = 0; a < splitted.Length; a++)
                {
                    if (a == 0)
                    {
                        positions[a] += movingLeft ? -1 : 1;
                    }
                    else if (a == 1)
                    {
                        positions[a] += movingLeft ? 1 : -1;
                    }
                    myConsole.SetCursorPosition(positions[a], myConsole.CursorTop);
                    myConsole.Write(splitted[a]);
                }
                myConsole.WriteLine();
            }
            for (int i = 0; i < 30; i++)
            {
                for (int a = 0; a < splitted.Length; a++)
                {
                    if (positions[a] != originalPos[a])
                    {
                        positions[a] += positions[a] < originalPos[a] ? 1 : -1;
                    }
                    myConsole.SetCursorPosition(positions[a], myConsole.CursorTop);
                    myConsole.Write(splitted[a]);
                }
                myConsole.WriteLine();
            }
            // Move to the left
            Move(ref curPos, 15, input, true, myConsole);
            for (int i = 0; i < 15; i++)
            {
                myConsole.WriteLine(input);
            }
            return myConsole.GetStringArray();
        }

        private static void Move(ref int curPos, int steps, string input, bool left, MyConsole myConsole)
        {
            if (left)
            {
                for (int i = 0; i < steps; i++)
                {
                    curPos--;
                    myConsole.SetCursorPosition(curPos, myConsole.CursorTop);
                    myConsole.WriteLine(input);
                }
            }
            else
            {
                for (int i = 0; i < steps; i++)
                {
                    curPos++;
                    myConsole.SetCursorPosition(curPos, myConsole.CursorTop);
                    myConsole.WriteLine(input);
                }
            }
        }

        private static string Offset(string str, int offset)
        {
            if (offset > 0)
            {
                string end = str.Substring(str.Length - offset);
                str = str.Remove(str.Length - offset);
                str = str.Insert(0, end);
            }
            else
            {
                offset *= -1;
                string start = str.Substring(0, offset);
                str = str.Remove(0, offset);
                str = str.Insert(str.Length, start);
            }
            return str;
        }

        private static string[] SplitTheString(string input, int parts)
        {
            string[] split = input.Split();
            if (split.Length != parts)
            {
                split = ChunksUpto(input, input.Length / parts).ToArray();
                if (split.Length > parts)
                {
                    string[] temp = new string[parts];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        temp[i] = split[i];
                    }
                    for (int i = parts; i < split.Length; i++)
                    {
                        temp[temp.Length - 1] += split[i];
                    }
                    split = temp;
                }
            }
            else
            {
                for (int i = 0; i < split.Length; i++)
                {
                    split[i] = split[i].Insert(0, " ");
                }
            }
            return split;
        }

        //private static IEnumerable<string> Split(string str, int chunkSize)
        //{
        //    return Enumerable.Range(0, str.Length / chunkSize)
        //        .Select(i => str.Substring(i * chunkSize, chunkSize));
        //}

        private static IEnumerable<string> ChunksUpto(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }

        private static int GetNearestMultiple(int value, int factor) // use get nearest to
        {
            return (int)Math.Round(
                              (value / (double)factor),
                              MidpointRounding.AwayFromZero
                          ) * factor;
        }

        private class MyConsole
        {
            private static List<string> everyThing = new List<string>();
            private static int[] position = new int[2];

            public MyConsole()
            {
                everyThing = new List<string>();
                position = new int[2];
            }

            public void WriteLine(string str)
            {
                UpdateCursor();
                if (everyThing[position[1]].Length < str.Length + position[0])
                {
                    everyThing[position[1]] = everyThing[position[1]].PadRight(position[0] + str.Length);
                }
                foreach (char c in str)
                {
                    everyThing[position[1]] = ReplaceAt(everyThing[position[1]], position[0], c);
                    position[0]++;
                }
                position[1]++;
                position[0] = 0;
                UpdateCursor();
            }

            public void WriteLine()
            {
                UpdateCursor();
                position[1]++;
                position[0] = 0;
                UpdateCursor();
            }

            private void UpdateCursor()
            {
                if (everyThing.Count < 1)
                {
                    everyThing.Add(new string(""));
                }

                if (position[1] > everyThing.Count - 1)
                {
                    int temp = everyThing.Count - 1;
                    for (int i = temp; i < position[1]; i++)
                    {
                        everyThing.Add(new string(""));
                    }
                }
                if (position[0] > 0 && everyThing[position[1]].Length - 1 < position[0])
                {
                    everyThing[position[1]] = everyThing[position[1]].PadRight(position[0]);
                }
            }

            public void Write(string str)
            {
                UpdateCursor();
                if (everyThing.Count < 1)
                {
                    everyThing.Add(new string(""));
                }
                if (everyThing[position[1]].Length < str.Length + position[0])
                {
                    everyThing[position[1]] = everyThing[position[1]].PadRight(position[0] + str.Length);
                }
                foreach (char c in str)
                {
                    everyThing[position[1]] = ReplaceAt(everyThing[position[1]], position[0], c);
                    position[0]++;
                }
                UpdateCursor();
            }

            public void Write(char c)
            {
                UpdateCursor();
                if (everyThing.Count < 1)
                {
                    everyThing.Add(new string(""));
                }
                if (everyThing[position[1]].Length < 1 + position[0])
                {
                    everyThing[position[1]] = everyThing[position[1]].PadRight(position[0] + 1);
                }
                everyThing[position[1]] = ReplaceAt(everyThing[position[1]], position[0], c);
                position[0]++;
                UpdateCursor();
            }

            public void SetCursorPosition(int x, int y)
            {
                position[0] = x;
                position[1] = y;
                UpdateCursor();
            }

            private string ReplaceAt(string input, int index, char newChar)
            {
                if (input == null)
                {
                    throw new ArgumentNullException("input");
                }
                char[] chars = input.ToCharArray();
                chars[index] = newChar;
                return new string(chars);
            }

            public int CursorTop
            {
                get
                {
                    return position[1];
                }
            }

            public int CursorLeft
            {
                get
                {
                    return position[0];
                }
            }

            public string GetString()
            {
                string SendString = string.Empty;
                for (int i = 0; i < everyThing.Count; i++)
                {
                    SendString += everyThing[i] + "\n";
                }
                return SendString;
            }

            public string[] GetStringArray()
            {
                return everyThing.ToArray();
            }
        }
    }
}