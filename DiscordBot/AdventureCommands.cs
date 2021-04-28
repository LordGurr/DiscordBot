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

        private string WriteLine(string str)
        {
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
            if (bot.commandLine == null)
            {
                var g = Client.GetChannelAsync(827869624808374293);
                bot.commandLine = g.Result;
            }
            if (bot.commandLine != ctx.Channel)
            {
                await bot.commandLine.SendMessageAsync(str).ConfigureAwait(false);
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
                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = title,
                            Description = SendString,
                        });
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
                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = title,
                            Description = SendString,
                        });
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
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task SystemInfo(CommandContext ctx)
        {
            string SendString = string.Empty;
            SendString += WriteLine("Program namn: " + System.AppDomain.CurrentDomain.FriendlyName/*, ctx*/);
            SendString += WriteLine("Bot namn: " + Client.CurrentApplication.Name/*, ctx*/);
            SendString += WriteLine("D#+ version: " + Client.VersionString/*, ctx*/);
            SendString += WriteLine("Gateway version: " + Client.GatewayVersion/*, ctx*/);
            SendString += WriteLine("Windows version: " + Environment.OSVersion/*, ctx*/);
            SendString += WriteLine(".Net version: " + Environment.Version/*, ctx*/);
            ScreenShootingShit screenShit = new ScreenShootingShit();
            ScreenShootingShit.DisplayInfoCollection displays = screenShit.GetDisplays();
            for (int i = 0; i < displays.Count; i++)
            {
                SendString += WriteLine("Monitor " + (i + 1) + " har en upplösning på " + displays[i].ScreenWidth + " gånger " + displays[i].ScreenHeight + " pixlar"/*, ctx*/);
            }
            SendString += WriteLine("Dator namn: " + Environment.MachineName/*, ctx*/);
            SendString += WriteLine("Användarnamn: " + Environment.UserName/*, ctx*/);
            SendString += WriteLine("Dator organisation: " + Environment.UserDomainName/*, ctx*/);
            SendString += WriteLine("Fil mapp: " + Environment.CurrentDirectory/*, ctx*/);
            SendString += WriteLine("Kommando rad: " + "\"" + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + "\""/*, ctx*/);

            TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
            if (uptime.TotalDays >= 1)
            {
                SendString += WriteLine("Tid sen full nedstängning: " + uptime.Days + " dagar " + uptime.Hours + " timmar " + uptime.Minutes + " minuter"/*, ctx*/);
            }
            else if (uptime.TotalHours >= 1)
            {
                SendString += WriteLine("Tid sen full nedstängning: " + (int)uptime.TotalHours + " timmar " + uptime.Minutes + " minuter"/*, ctx*/);
            }
            else if (uptime.TotalMinutes >= 1)
            {
                SendString += WriteLine("Tid sen full nedstängning: " + (int)uptime.TotalMinutes + " minuter " + (int)uptime.Seconds + " sekunder "/*, ctx*/);
            }
            else
            {
                SendString += WriteLine("Tid sen full nedstängning: " + (int)uptime.TotalSeconds + " sekunder "/*, ctx*/);
            }

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
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task BotInfo(CommandContext ctx)
        {
            string SendString = string.Empty;
            SendString += WriteLine("Bot namn: " + Client.CurrentApplication.Name/*, ctx*/);
            //await WriteLine("Team name: " + Client.CurrentApplication.Team.Name/*, ctx*/);
            //var a = Client.CurrentApplication.Team.Members.ToArray();
            //for (int i = 0; i < a.Length; i++)
            //{
            //    await WriteLine("Member " + (i + 1) + ": " + Client.CurrentApplication.Team.Members/*, ctx*/);
            //}
            var b = Client.CurrentApplication.Owners.ToArray();
            for (int i = 0; i < b.Length; i++)
            {
                SendString += WriteLine("Owner " + (i + 1) + ": " + b[i].Username/*, ctx*/);
            }
            TimeSpan uptime = bot.sw.Elapsed;
            if (uptime.TotalDays >= 1)
            {
                SendString += WriteLine("Upptid: " + uptime.Days + " dagar " + uptime.Hours + " timmar " + uptime.Minutes + " minuter"/*, ctx*/);
            }
            else if (uptime.TotalHours >= 1)
            {
                SendString += WriteLine("Upptid: " + (int)uptime.TotalHours + " timmar " + uptime.Minutes + " minuter"/*, ctx*/);
            }
            else if (uptime.TotalMinutes >= 1)
            {
                SendString += WriteLine("Upptid: " + (int)uptime.TotalMinutes + " minuter " + (int)uptime.Seconds + " sekunder "/*, ctx*/);
            }
            else
            {
                SendString += WriteLine("Upptid: " + (int)uptime.TotalSeconds + " sekunder "/*, ctx*/);
            }
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
            int members = 0;
            for (int i = 0; i < kanalerna.Count; i++)
            {
                members += kanalerna[i].discordUsers.Count;
            }
            SendString += WriteLine("Har " + kanalerna.Count + " kanaler och " + members + " medlemmar");
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
            if (ctx.Channel.Id != bot.commandLine.Id)
            {
                await WriteLine("Sparade alla " + botCoinSaves.Count + " botcoin användares botcoin.", ctx);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("savemembers")]
        [DSharpPlus.CommandsNext.Attributes.Description("Saves members and channels users.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task SaveAllmembers(CommandContext ctx)
        {
            await bot.SaveMembers();
            if (ctx.Channel.Id != bot.commandLine.Id)
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
        public async Task DMa(CommandContext ctx, [RemainingText] string str)
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
        public async Task FileUpload(CommandContext ctx, params string[] filePath)
        {
            string actualFilePath = "";
            for (int i = 0; i < filePath.Length; i++)
            {
                actualFilePath += filePath[i];
                if (i + 1 < filePath.Length)
                {
                    actualFilePath += " ";
                }
            }
            await bot.UploadFile(actualFilePath, ctx.Channel);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("skärmdump")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("skärmbild", "screenshot")]
        [DSharpPlus.CommandsNext.Attributes.Description("Takes a screenshot.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task Screenshot(CommandContext ctx)
        {
            await bot.TakeScreenshotAndUpload(ctx);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("skärm")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("monitorbild", "monitorshot", "monitor")]
        [DSharpPlus.CommandsNext.Attributes.Description("Takes a screenshot.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task ScreenshotMonitor(CommandContext ctx, int index)
        {
            try
            {
                ScreenShootingShit screenShit = new ScreenShootingShit();
                ScreenShootingShit.DisplayInfoCollection displays = screenShit.GetDisplays();
                await bot.TakeScreenshotAndUploadApplication(ctx, displays[index - 1].hMonitor);
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
                int x = Console.CursorLeft;
                int y = Console.CursorTop;
                Console.SetCursorPosition(0, 0);
                await Task.Delay(1);
                Console.SetCursorPosition(x, y);
                await Task.Delay(5);
                await bot.TakeScreenshotAndUploadApplication(ctx, Process.GetCurrentProcess().MainWindowHandle);
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

        [DSharpPlus.CommandsNext.Attributes.Command("appsscreenshot")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("appsskärmbilder", "appsskärmdumpar")]
        [DSharpPlus.CommandsNext.Attributes.Description("Tar en skärmdump på alla program som är i gång.")]
        [DSharpPlus.CommandsNext.Attributes.RequireOwner]
        public async Task GetAppScreen(CommandContext ctx)
        {
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
                await CommandWriteLine("Stänger ner omdelbart på order av: " + ctx.Member.DisplayName + "(" + ctx.Member.Username + ")", ctx);
                if (bot.isAdding)
                {
                    await WriteLine("Väntar på att ladda in medlemmar ", ctx);
                    while (bot.isAdding)
                    {
                        await Task.Delay(5000);
                    }
                }
                await Client.DisconnectAsync();
                await Task.Delay(500);
                await SaveAllbotcoin(ctx);
                await SaveAllmembers(ctx);
                await bot.TakeScreenshotAndUploadApplication(ctx, Process.GetCurrentProcess().MainWindowHandle);
                Client.Dispose();
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
                await Client.DisconnectAsync();
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
                await Client.DisconnectAsync();
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
                await Client.UpdateStatusAsync(activity);
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
            int i = BotCoinIndex(ctx);
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
        }

        [DSharpPlus.CommandsNext.Attributes.Command("botcoinleaderboard")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("leaderboard", "botcoinleader", "botcoinboard")]
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
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Leaderboard",
                Description = SendString,
            });
            SendString = string.Empty;
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
            Image image = Image.FromFile(@"C:\Users\gustav.juul\Pictures\GaleBackup\ToadSpriteRightJump.png");
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
                await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
                return;
            }
            try
            {
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
            //Doesn't work yet    //List<DiscordAttachment> attachments = (List<DSharpPlus.Entities.DiscordAttachment>)ctx.Message.Attachments;
            //attachments.FindAll(x => x.GetType() == typeof(".png"))
            string SendString = "";
            try
            {
                //Image image;
                using (WebClient client = new WebClient())
                {
                    SaveImage(tempImagePng, System.Drawing.Imaging.ImageFormat.Png, "https://thispersondoesnotexist.com/image");
                    //image = Image.FromFile(tempImage);
                }

                await bot.UploadFile(tempImagePng, ctx.Channel);
                GiveBotCoin(ctx);
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
            }
            SendString = "";
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
                    SaveImage(tempImagePng, System.Drawing.Imaging.ImageFormat.Png, url);
                    await bot.UploadFile(tempImagePng, ctx.Channel);
                    GiveBotCoin(ctx);
                }
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("code")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("repo", "func", "void")]
        public async Task SendCode(CommandContext ctx, string commandName)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    //client.DownloadFile(new Uri(url), @"c:\temp\image35.png");
                    string code = client.DownloadString("https://raw.githubusercontent.com/LordGurr/DiscordBot/master/DiscordBot/Bot.cs");
                    //SaveImage(tempImagePng, System.Drawing.Imaging.ImageFormat.Png, url);
                    if (code.Contains("            [DSharpPlus.CommandsNext.Attributes.Command(\"" + commandName + "\")]"))
                    {
                        int index = code.IndexOf("            [DSharpPlus.CommandsNext.Attributes.Command(\"" + commandName + "\")]");
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
                            double result = Math.Ceiling((double)sendCode.Length / 2048.0);
                            int size = (int)((double)sendCode.Length / result);
                            int latest = 0;
                            for (int i = 0; i < result; i++)
                            {
                                string send = string.Empty;
                                if (i + 1 >= result)
                                {
                                    send = sendCode.Substring(latest, sendCode.Length - latest);
                                }
                                else
                                {
                                    send = sendCode.Substring(latest, size);
                                }
                                send = "```cs\n" + send + "\n```";//\n``` ```cs
                                                                  //sendCode += "\nResten av koden: https://github.com/LordGurr/DiscordBot/blob/master/DiscordBot/Bot.cs";
                                if (i + 1 >= result)
                                {
                                    send += "\n[Github länk till koden.](https://github.com/LordGurr/DiscordBot/blob/master/DiscordBot/Bot.cs)";
                                }

                                //await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                //{
                                //    Title = "Code snippet " + (i + 1),
                                //    Description = send,
                                //});
                                await ctx.Channel.SendMessageAsync(send).ConfigureAwait(false);
                                latest += size;
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
                var a = Client.CurrentApplication.Owners.ToArray();
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
                    await ctx.Channel.SendMessageAsync("Du måste vara admin för att använda detta kommand.").ConfigureAwait(false);
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
                var a = Client.CurrentApplication.Owners.ToArray();
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
                    await ctx.Channel.SendMessageAsync("Du måste vara admin för att använda detta kommand.").ConfigureAwait(false);
                    return;
                }
            }
            await ctx.Channel.SendMessageAsync("nickname behöver en string med namnet medlemmar ska döpas till.\nExempel: ?nickname kyckling").ConfigureAwait(false);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("math")]
        [DSharpPlus.CommandsNext.Attributes.Aliases("matte")]
        [DSharpPlus.CommandsNext.Attributes.Description("Löser simpla matematiska uttryck.\nFunkar inte med variabler och bara med heltal.\n kräver * före och efter parentes.\nExempel: ?math (2*2)^8")]
        public async Task MathExpress(CommandContext ctx)
        {
            string result = ctx.Message.Content.Replace("?math", "");
            if (!IsDigitsOnly(result, "()^*/+-.,"))
            {
                await ctx.Channel.SendMessageAsync("String was inputted incorectly").ConfigureAwait(false);
                return;
            }
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

        private string[] _operators = { "-", "+", "/", "*", "^" };

        private Func<double, double, double>[] _operations = {
        (a1, a2) => a1 - a2,
        (a1, a2) => a1 + a2,
        (a1, a2) => a1 / a2,
        (a1, a2) => a1 * a2,
        (a1, a2) => Math.Pow(a1, a2)
    };

        public double Eval(string expression)
        {
            List<string> tokens = getTokens(expression);
            Stack<double> operandStack = new Stack<double>();
            Stack<string> operatorStack = new Stack<string>();
            int tokenIndex = 0;

            while (tokenIndex < tokens.Count)
            {
                string token = tokens[tokenIndex];
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
                if (Array.IndexOf(_operators, token) >= 0)
                {
                    while (operatorStack.Count > 0 && Array.IndexOf(_operators, token) < Array.IndexOf(_operators, operatorStack.Peek()))
                    {
                        string op = operatorStack.Pop();
                        double arg2 = operandStack.Pop();
                        double arg1 = operandStack.Pop();
                        operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
                    }
                    operatorStack.Push(token);
                }
                else
                {
                    operandStack.Push(double.Parse(token));
                }
                tokenIndex += 1;
            }

            while (operatorStack.Count > 0)
            {
                string op = operatorStack.Pop();
                double arg2 = operandStack.Pop();
                double arg1 = operandStack.Pop();
                operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
            }
            return operandStack.Pop();
        }

        private string getSubExpression(List<string> tokens, ref int index)
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

        private List<string> getTokens(string expression)
        {
            string operators = "()^*/+-";
            List<string> tokens = new List<string>();
            StringBuilder sb = new StringBuilder();

            foreach (char c in expression.Replace(" ", string.Empty))
            {
                if (operators.IndexOf(c) >= 0)
                {
                    if ((sb.Length > 0))
                    {
                        tokens.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    tokens.Add(c.ToString());
                }
                else
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
        public async Task Adven(CommandContext ctx, [DSharpPlus.CommandsNext.Attributes.Description("Input which direction you wan't to move in. up, down, left, right. Example: ?adventure right")]string direction)
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
    }
}