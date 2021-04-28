using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using HWND = System.IntPtr;
using System.Xml.Serialization;
using System.Threading;

//using DSharpPlus.VoiceNext;
//using System.Management;   //This namespace is used to work with WMI classes. For using this namespace add reference of System.Management.dll .

namespace DiscordBot
{
    public class Bot
    {
        public static DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        public static Random rng = new Random();
        public static string[] gåtor;
        public static string[] svar;
        public static List<AdventureSaveData> saveData = new List<AdventureSaveData>();
        public static string[] quote;
        public static string[] quoteTemplates;// s = substantiv, k = känsla, t = tidsadverb
        public static string[] nouns;
        public static string[] emotions;

        public static List<BotCoinSaveData> botCoinSaves = new List<BotCoinSaveData>();
        public static List<ChannelSaveData> kanalerna = new List<ChannelSaveData>();

        public DiscordChannel commandLine;

        public bool shutdown = false;
        public bool restart = true;
        public DateTime lastSave;
        public TimeSpan sparTid;
        public Stopwatch sw;
        public bool stopAll = false;

        public const string tempImagePng = "screenshotTemp.png";

        public List<RemindmeSave> queuedRemindMes = new List<RemindmeSave>();

        //public VoiceNextExtension Voice { get; set; } //To play music

        private async Task WriteLine(string str)
        {
            //Utskrivet.Add(str);
            if (commandLine == null)
            {
                var g = Client.GetChannelAsync(827869624808374293);
                commandLine = g.Result;
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.White;
            await commandLine.SendMessageAsync(str).ConfigureAwait(false);
        }

        private async Task WriteLine(string str, DiscordChannel e)
        {
            //Utskrivet.Add(str);
            if (commandLine == null)
            {
                var g = Client.GetChannelAsync(827869624808374293);
                commandLine = g.Result;
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.White;
            await commandLine.SendMessageAsync(str).ConfigureAwait(false);
            if (e != commandLine)
            {
                await e.SendMessageAsync(str).ConfigureAwait(false);
            }
        }

        public async Task UploadFile(string path)
        {
            try
            {
                //Utskrivet.Add(str);
                if (commandLine == null)
                {
                    var g = Client.GetChannelAsync(827869624808374293);
                    commandLine = g.Result;
                }
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var msg = await new DiscordMessageBuilder()
                        //.WithContent("Here is a really dumb file that I am testing with.")
                        .WithFiles(new Dictionary<string, Stream>() { { path, fs } })

                        .SendAsync(commandLine);
                }
                //await WriteLine("Uppladdat: " + path + " till kanalen: " + commandLine.Name + ".");
            }
            catch (Exception e)
            {
                await WriteLine(e.Message);
            }
        }

        public async Task UploadFile(string path, DiscordChannel channel)
        {
            try
            {
                //Utskrivet.Add(str);
                if (commandLine == null)
                {
                    var g = Client.GetChannelAsync(827869624808374293);
                    commandLine = g.Result;
                }
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var msg = await new DiscordMessageBuilder()
                        //.WithContent("Here is a really dumb file that I am testing with.")
                        .WithFiles(new Dictionary<string, Stream>() { { path, fs } })
                        .SendAsync(channel);
                }
                //if (channel != commandLine)
                //{
                //    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                //    {
                //        var msg = await new DiscordMessageBuilder()
                //            //.WithContent("Here is a really dumb file that I am testing with.")
                //            .WithFiles(new Dictionary<string, Stream>() { { path, fs } })
                //            .SendAsync(commandLine);
                //    }
                //}
                //await WriteLine("Uppladdat: " + path + " till kanalen: " + channel.Name + ".", channel);
            }
            catch (Exception e)
            {
                await WriteLine(e.Message, channel);
            }
        }

        public async Task TakeScreenshotAndUpload(MessageCreateEventArgs ctx)
        {
            try
            {
                ScreenCapture screenCapture = new ScreenCapture();
                Image img = screenCapture.CaptureScreen();
                screenCapture.CaptureScreenToFile(tempImagePng, System.Drawing.Imaging.ImageFormat.Png, img);

                if (ctx == null)
                {
                    await UploadFile(tempImagePng);
                }
                else if (ctx.Channel.Id != commandLine.Id)
                {
                    await UploadFile(tempImagePng, ctx.Channel);
                }
                else
                {
                    await UploadFile(tempImagePng);
                }
            }
            catch (Exception e)
            {
                await WriteLine(e.Message, ctx.Channel);
            }
        }

        public async Task TakeScreenshotAndUpload(CommandContext ctx)
        {
            try
            {
                //await UploadFile("screenshotTemp.png");
                ScreenCapture screenCapture = new ScreenCapture();
                Image img = screenCapture.CaptureScreen();
                screenCapture.CaptureScreenToFile(tempImagePng, System.Drawing.Imaging.ImageFormat.Png, img);

                if (ctx == null)
                {
                    await UploadFile(tempImagePng);
                }
                else if (ctx.Channel.Id != commandLine.Id)
                {
                    await UploadFile(tempImagePng, ctx.Channel);
                }
                else
                {
                    await UploadFile(tempImagePng);
                }
            }
            catch (Exception e)
            {
                await WriteLine(e.Message, ctx.Channel);
            }
        }

        public async Task TakeScreenshotAndUploadApplication(CommandContext ctx, HWND handle)
        {
            ScreenShootingShit screenShit = new ScreenShootingShit();
            //ScreenShootingShit.DisplayInfoCollection displays = screenShit.GetDisplays();
            ScreenCapture screenCapture = new ScreenCapture();
            //Image img = screenCapture.CaptureWindow(displays[1].hMonitor);
            Image img = screenCapture.CaptureWindow(handle);
            screenCapture.CaptureScreenToFile(tempImagePng, System.Drawing.Imaging.ImageFormat.Png, img);
            //await UploadFile("screenshotTemp.png");
            if (OpenWindowGetter.IsWindowVisible(handle))
            {
                if (ctx == null || ctx.Channel.Id == commandLine.Id)
                {
                    await UploadFile(tempImagePng);
                }
                else if (ctx.Channel.Id != commandLine.Id)
                {
                    await UploadFile(tempImagePng, ctx.Channel);
                }
            }
            else
            {
                await ctx.RespondAsync("Windown is not visible").ConfigureAwait(false);
            }
        }

        private async Task TakeScreenshotAndUploadApplication(MessageCreateEventArgs ctx, HWND handle)
        {
            ScreenShootingShit screenShit = new ScreenShootingShit();
            //ScreenShootingShit.DisplayInfoCollection displays = screenShit.GetDisplays();
            ScreenCapture screenCapture = new ScreenCapture();
            //Image img = screenCapture.CaptureWindow(displays[1].hMonitor);
            Image img = screenCapture.CaptureWindow(handle);
            screenCapture.CaptureScreenToFile(tempImagePng, System.Drawing.Imaging.ImageFormat.Png, img);
            //await UploadFile("screenshotTemp.png");
            if (OpenWindowGetter.IsWindowVisible(handle))
            {
                if (ctx == null || ctx.Channel.Id == commandLine.Id)
                {
                    await UploadFile(tempImagePng);
                }
                else if (ctx.Channel.Id != commandLine.Id)
                {
                    await UploadFile(tempImagePng, ctx.Channel);
                }
            }
            else
            {
                await ctx.Message.RespondAsync("Windown is not visible").ConfigureAwait(false);
            }
        }

        public async Task SaveMembers()
        {
            //BinaryFormatter formatter = new BinaryFormatter();
            for (int i = 0; i < kanalerna.Count; i++)
            {
                string fileName = kanalerna[i].discordChannel.ToString() + ".txt";
                if (!File.Exists(fileName))
                {
                    var temp = File.Create(fileName);
                    temp.Close();
                }
                try
                {
                    //using (FileStream stream = File.OpenWrite(fileName))
                    //{
                    //    formatter.Serialize(stream, kanalerna[i]);
                    //}
                    if (kanalerna[i].finished)
                    {
                        using (TextWriter tw = new StreamWriter(fileName))
                        {
                            for (int a = 0; a < kanalerna[i].discordUsers.Count; a++)
                            {
                                tw.WriteLine(kanalerna[i].discordUsers[a].user.ToString());
                            }
                        }
                    }
                    else
                    {
                        using (TextWriter tw = new StreamWriter(fileName))
                        {
                            for (int a = 0; a < kanalerna[i].membersToAdd.Length; a++)
                            {
                                tw.WriteLine(kanalerna[i].membersToAdd[a]);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    await WriteLine(e.Message);
                }
            }
            using (TextWriter tw = new StreamWriter("channels.txt"))
            {
                for (int i = 0; i < kanalerna.Count; i++)
                {
                    tw.WriteLine(kanalerna[i].discordChannel.ToString() + ".txt");
                }
                tw.WriteLine(kanalerna.Count + " stycken kanaler");
            }
            int members = 0;
            for (int i = 0; i < kanalerna.Count; i++)
            {
                members += kanalerna[i].discordUsers.Count;
            }
            await WriteLine("Sparade " + kanalerna.Count + " kanaler och " + members + " medlemmar");
        }

        private static ChannelSaveData Load(string FileName)
        {
            using (var stream = System.IO.File.OpenRead(FileName))
            {
                var serializer = new XmlSerializer(typeof(ChannelSaveData));
                return serializer.Deserialize(stream) as ChannelSaveData;
            }
        }

        private async Task ReadAllMemebers()
        {
            string[] tempArray = await File.ReadAllLinesAsync("channels.txt");
            for (int i = 0; i < tempArray.Length - 1; i++)
            {
                try
                {
                    string[] members = File.ReadAllLines(tempArray[i]);
                    kanalerna.Add(new ChannelSaveData(Convert.ToUInt64(tempArray[i].Replace(".txt", ""))));
                    var g = Client.GetChannelAsync(kanalerna[kanalerna.Count - 1].discordChannel);
                    kanalerna[kanalerna.Count - 1].realDiscordChannel = g.Result;
                    kanalerna[kanalerna.Count - 1].membersToAdd = members;

                    //for (int a = 0; a < kanalerna[kanalerna.Count - 1].membersToAdd.Length; a++)
                    //{
                    //    var getMem = kanalerna[kanalerna.Count - 1].realDiscordChannel.Guild.GetMemberAsync(Convert.ToUInt64(kanalerna[kanalerna.Count - 1].membersToAdd[a]));
                    //    if (!kanalerna[kanalerna.Count - 1].discordUsers.Any(o => o.user == getMem.Result.Id))
                    //    {
                    //        kanalerna[kanalerna.Count - 1].discordUsers.Add(new DiscordMemberSaveData(getMem.Result));
                    //    }
                    //}
                    //kanalerna[kanalerna.Count - 1].membersToAdd = new string[kanalerna[kanalerna.Count - 1].membersToAdd.Length];
                    //kanalerna[kanalerna.Count - 1].finished = true;
                }
                catch (Exception e)
                {
                    await WriteLine(e.Message);
                }
            }
        }

        private void AddMembers(MessageCreateEventArgs e)
        {
            if (e.Channel.Name != null)
            {
                int[] channel = ChannelIndex(e);
                if (channel[1] < 0)
                {
                    if (channel[0] < 0)
                    {
                        kanalerna.Add(new ChannelSaveData(e.Channel));
                        channel[0] = kanalerna.Count - 1;
                    }
                    else if (kanalerna[channel[0]].realDiscordChannel == null)
                    {
                        kanalerna[channel[0]].realDiscordChannel = e.Channel;
                    }

                    //List<DiscordMember> temp = e.Message.Channel.Users.ToList();
                    //List<int> indexesFound = new List<int>();
                    //for (int i = 0; i < temp.Count; i++)
                    //{
                    //    for (int a = 0; a < kanalerna[channel[0]].discordUsers.Count; a++)
                    //    {
                    //        if (kanalerna[channel[0]].discordUsers[a].member == temp[i])
                    //        {
                    //            indexesFound.Add(i);
                    //        }
                    //    }
                    //}
                    //for (int i = 0; i < temp.Count; i++)
                    //{
                    //    if (!indexesFound.Contains(i))
                    //    {
                    //        kanalerna[channel[0]].discordUsers.Add(new DiscordMemberSaveData(temp[i]));
                    //    }
                    //}

                    //List<DiscordChannel> channels = e.Guild.Channels.Values.ToList();
                    //for (int i = 0; i < channels.Count; i++)
                    //{
                    //    ChannelSaveData curChannel = kanalerna.Find(a => a.discordChannel == channels[i].Id);

                    //    if (curChannel != null)
                    //    {
                    //        List<DiscordMember> curUsers = channels[i].Users.ToList();
                    //        for (int a = 0; a < curUsers.Count; a++)
                    //        {
                    //            if (!curChannel.discordUsers.Any(o => o.user == curUsers[a].Id))
                    //            {
                    //                curChannel.discordUsers.Add(new DiscordMemberSaveData(curUsers[a]));
                    //            }
                    //        }
                    //        if (!curChannel.finished)
                    //        {
                    //            for (int a = 0; a < curChannel.membersToAdd.Length; a++)
                    //            {
                    //                var getMem = e.Guild.GetMemberAsync(Convert.ToUInt64(curChannel.membersToAdd[a]));
                    //                if (!curChannel.discordUsers.Any(o => o.user == getMem.Result.Id))
                    //                {
                    //                    curChannel.discordUsers.Add(new DiscordMemberSaveData(getMem.Result));
                    //                }
                    //            }
                    //            curChannel.membersToAdd = new string[curChannel.membersToAdd.Length];
                    //            curChannel.finished = true;
                    //            break;
                    //        }
                    //    }
                    //}
                }
            }
        }

        public bool isAdding = false;

        private async Task LoadChannels(MessageCreateEventArgs e)
        {
            if (!isAdding)
            {
                isAdding = true;
                try
                {
                    //for (int i = 0; i < kanalerna.Count; i++)
                    //{
                    //ChannelSaveData curChannel = kanalerna[i];
                    List<DiscordChannel> theChannels = e.Guild.Channels.Values.ToList();
                    ulong guildID = e.Guild.Id;
                    List<ChannelSaveData> channelsInGuild = kanalerna.FindAll(a => a.realDiscordChannel.GuildId == guildID).ToList();
                    if (!channelsInGuild.All(a => a.finished))
                    {
                        List<string> membersGoingToAdd = new List<string>();
                        for (int a = 0; a < channelsInGuild.Count; a++)
                        {
                            for (int b = 0; b < channelsInGuild[a].membersToAdd.Length; b++)
                            {
                                if (!membersGoingToAdd.Contains(channelsInGuild[a].membersToAdd[b]))
                                {
                                    membersGoingToAdd.Add(channelsInGuild[a].membersToAdd[b]);
                                }
                            }
                        }
                        List<DiscordMemberSaveData> allTheMembersToAddToSave = new List<DiscordMemberSaveData>();

                        for (int a = 0; a < membersGoingToAdd.Count; a++)
                        {
                            var getMem = e.Guild.GetMemberAsync(Convert.ToUInt64(membersGoingToAdd[a]));
                            allTheMembersToAddToSave.Add(new DiscordMemberSaveData(getMem.Result));
                        }

                        for (int a = 0; a < channelsInGuild.Count; a++)
                        {
                            //channelsInGuild[a].discordUsers = allTheMembersToAddToSave;
                            for (int b = 0; b < allTheMembersToAddToSave.Count; b++)
                            {
                                if (!channelsInGuild[a].discordUsers.Any(o => o.user == allTheMembersToAddToSave[b].user))
                                {
                                    channelsInGuild[a].discordUsers.Add(allTheMembersToAddToSave[b]);
                                }
                            }
                            channelsInGuild[a].membersToAdd = new string[0];
                            channelsInGuild[a].finished = true;
                        }
                        //if (!curChannel.finished && theChannels.Any(a => a.Id == curChannel.discordChannel))
                        //{
                        //    try
                        //    {
                        //        for (int a = 0; a < curChannel.membersToAdd.Length; a++)
                        //        {
                        //            var getMem = e.Guild.GetMemberAsync(Convert.ToUInt64(curChannel.membersToAdd[a]));
                        //            if (!curChannel.discordUsers.Any(o => o.user == getMem.Result.Id))
                        //            {
                        //                curChannel.discordUsers.Add(new DiscordMemberSaveData(getMem.Result));
                        //            }
                        //        }
                        //        curChannel.membersToAdd = new string[curChannel.membersToAdd.Length];
                        //        curChannel.finished = true;
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        await WriteLine(ex.Message);
                        //    }
                        //}
                        //}
                        if (allTheMembersToAddToSave.Count > 0)
                        {
                            await WriteLine("Läste in " + allTheMembersToAddToSave.Count + " medlemmar till servern " + e.Guild.Name);
                        }
                        if (kanalerna.All(a => a.finished))
                        {
                            int members = 0;
                            for (int i = 0; i < kanalerna.Count; i++)
                            {
                                members += kanalerna[i].discordUsers.Count;
                            }
                            await WriteLine("Alla " + members + " medlemmar har blivt inlästa.");
                        }
                    }
                    isAdding = false;
                }
                catch (Exception ex)
                {
                    isAdding = false;
                    await WriteLine(ex.Message);
                }
            }
        }

        private async Task LoadAllTheChannels()
        {
            List<ulong> guildIds = new List<ulong>();
            List<DiscordChannel> channelsToMessage = new List<DiscordChannel>();
            for (int i = 0; i < kanalerna.Count; i++)
            {
                if (!guildIds.Any(a => a == kanalerna[i].realDiscordChannel.GuildId) && !kanalerna[i].finished)
                {
                    guildIds.Add(kanalerna[i].realDiscordChannel.GuildId);
                    channelsToMessage.Add(kanalerna[i].realDiscordChannel);
                }
            }
            for (int i = 0; i < channelsToMessage.Count; i++)
            {
                if (isAdding)
                {
                    await WriteLine("Waiting for adder in server: " + channelsToMessage[i].Guild.Name);
                    while (isAdding)
                    {
                        await Task.Delay(5000);
                    }
                }
                await Task.Delay(2000);
                await channelsToMessage[i].SendMessageAsync("Boten är uppkopplad och redo för kommandon.").ConfigureAwait(false);
                if (isAdding)
                {
                    await WriteLine("Waiting for adder in server: " + channelsToMessage[i].Guild.Name);
                    while (isAdding)
                    {
                        await Task.Delay(5000);
                    }
                }
            }
            await WriteLine("Det borde stå att alla medlemmar blivit inlästa.");
        }

        public async Task RunAsync()
        {
            AdventureCommands.bot = this;
            sw = Stopwatch.StartNew();
            Environment.CurrentDirectory = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Replace("\\" + System.AppDomain.CurrentDomain.FriendlyName + ".exe", string.Empty);
            restart = true;
            var json = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            DiscordConfiguration config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
            };
            Client = new DiscordClient(config);
            //ReadyEventArgs e = Client.Ready;
            ///  Client.Ready += OnClientReady;
            //OnClientReady(ReadyEventArgs e);
            CommandsNextConfiguration commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                IgnoreExtraArguments = true,
            };
            sparTid = DateTime.Now.AddMinutes(5) - DateTime.Now;
            lastSave = DateTime.Now;
            Client.MessageCreated += async (s, e) =>
           {
               try
               {
                   if (!stopAll)
                   {
                       AddMembers(e);
                       if (!kanalerna.All(a => a.finished) && !isAdding)
                       {
                           Thread t = new Thread(async () => await LoadChannels(e));
                           t.Start();
                       }
                   }
               }
               catch (Exception ex)
               {
                   await WriteLine(e.Message + "\n" + ex.Message);
               }
               if (e.Message.Content.ToLower().Contains("kommunism") || e.Message.Content.ToLower().Contains("communism"))
                   await e.Message.RespondAsync("All hail the motherland!").ConfigureAwait(false);
               else if (e.Message.Content.ToLower().Contains("när är"))
                   await e.Message.RespondAsync("Imorgon.").ConfigureAwait(false);
               else if (e.Message.Content.ToLower().Contains("prov") && !e.Message.Content.ToLower().Contains("gick") && !e.Message.Content.ToLower().Contains("ute") && !e.Message.Content.ToLower().Contains("resultat") && !e.Message.Content.ToLower().Contains("prova") && !e.Message.Content.ToLower().Contains("proved") && !e.Message.Content.ToLower().Contains("tillbaka"))
                   await e.Message.RespondAsync("hoppas du pluggar").ConfigureAwait(false);
               else if (e.Message.MentionEveryone || e.Message.MentionedUsers.Count > 10)
                   await e.Message.RespondAsync("That's not cool.").ConfigureAwait(false);
               else if (e.MentionedUsers.Count == 1 && e.MentionedUsers[0].Username == Client.CurrentApplication.Name && !e.Message.Content.StartsWith("?"))
               {
                   await e.Message.RespondAsync("You called...\nSkriv ?help för att se vad jag kan göra.").ConfigureAwait(false);
               }
               else if (e.Message.Content.ToLower().EndsWith("i'm pappa!") || e.Message.Content.ToLower().EndsWith("i'm dad!") || e.Message.Content.ToLower().StartsWith("hi") && e.Message.Content.ToLower().EndsWith("!") && e.Author.Id == 503720029456695306/* Dad bot's id */)
                   await e.Message.RespondAsync("Hej " + e.Author.Username + "...").ConfigureAwait(false);
           };
            await Reload();
            async Task Reload()
            {
                DateTime dateTime = DateTime.Now;
                //List<string> Utskrivet = new List<string>();

                //Utskrivet.Add("Finns på " + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                //Console.WriteLine(Utskrivet[Utskrivet.Count - 1]);
                rng = new Random();
                saveData = new List<AdventureSaveData>();
                botCoinSaves = new List<BotCoinSaveData>();
                kanalerna = new List<ChannelSaveData>();
                await WriteLine("Heter " + System.AppDomain.CurrentDomain.FriendlyName);
                await WriteLine("D#+ version " + Client.VersionString);

                //Console.WriteLine(Utskrivet[Utskrivet.Count - 1]);
                gåtor = await File.ReadAllLinesAsync("gåtor.txt");
                svar = await File.ReadAllLinesAsync("gåtSvaren.txt");
                await WriteLine("Har läst in " + gåtor.Length + " gåtor och " + svar.Length + " svar på dem");
                //Console.WriteLine(Utskrivet[Utskrivet.Count - 1]);
                quote = await File.ReadAllLinesAsync("citat.txt");
                for (int i = 0; i < quote.Length; i++)
                {
                    quote[i] = quote[i].Replace("\\n", "\n");
                }
                await WriteLine("Har läst in " + quote.Length + " citat");
                //Console.WriteLine(Utskrivet[Utskrivet.Count - 1]);
                quoteTemplates = await File.ReadAllLinesAsync("citatTemplate.txt");
                await WriteLine("Har läst in " + quoteTemplates.Length + " citat mallar");
                nouns = await File.ReadAllLinesAsync("nouns.txt");
                await WriteLine("Har läst in " + nouns.Length + " substantiv");
                emotions = await File.ReadAllLinesAsync("emotions.txt");
                await WriteLine("Har läst in " + emotions.Length + " känslor");
                await ReadBotCoin();
                await WriteLine("Har läst in " + botCoinSaves.Count + " användares botcoins");
                await ReadAllMemebers();
                int members = 0;
                for (int i = 0; i < kanalerna.Count; i++)
                {
                    members += kanalerna[i].discordUsers.Count;
                }
                await WriteLine("Har läst in " + kanalerna.Count + " kanaler och " + members + " medlemmar");
                TimeSpan timeSpan = DateTime.Now - dateTime;
                double totalMilliseconds = Convert.ToInt32(timeSpan.TotalMilliseconds * 100);
                totalMilliseconds /= 100;
                await WriteLine("Tog " + totalMilliseconds + " millisekunder att läsa in");
                await Task.Delay(500);
                MessageCreateEventArgs message;
                message = null;
                try
                {
                    await TakeScreenshotAndUploadApplication(message, Process.GetCurrentProcess().MainWindowHandle);
                }
                catch (Exception e)
                {
                    await WriteLine(e.Message);
                    await TakeScreenshotAndUpload(message);
                }
            }

            Client.Resumed += async (e, a) =>
            {
                await WriteLine("Tillbaka kopplad efter frånkoppling.");
            };
            Client.Ready += async (e, a) =>
            {
                //await WriteLine("Boten är uppkopplad och redo för kommandon.");
                Thread t = new Thread(async () => await LoadAllTheChannels());
                t.Start();
            };
            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<AdventureCommands>();
            //Voice = Client.UseVoiceNext(); //To play msucic

            await Client.ConnectAsync();
            //var c = Client.GetUserAsync(460713383017185292);
            //var d = Client.Guilds;
            //var e = Client.CurrentApplication.Owners;
            //var f = Client.PrivateChannels;
            //var g = Client.GetChannelAsync(827869624808374293);
            //await g.Result.SendMessageAsync("ping");
            //var k = g.Result.;
            //DiscordMember[] members = k.Result[0].Channel.Users.ToArray();

            //Client.
            while (!shutdown)
            {
                await Task.Delay(Convert.ToInt32(sparTid.TotalMilliseconds));
                lastSave = DateTime.Now;
                if (!stopAll)
                {
                    await SaveBotCoin();
                    await SaveMembers();
                }
            }
            await WriteLine("Shutting down.");
            if (restart)
            {
                await WriteLine("Will start again after reboot.");
            }
            if (isAdding)
            {
                await WriteLine("Väntar på att ladda in medlemmar ");
                while (isAdding)
                {
                    await Task.Delay(5000);
                }
            }
            MessageCreateEventArgs message;
            message = null;
            await TakeScreenshotAndUploadApplication(message, Process.GetCurrentProcess().MainWindowHandle);
            await Client.DisconnectAsync();
            Client.Dispose();
        }

        public static bool IsDigitsOnly(string str, string operators) //Den här kollar så att det bara finns nummer eller mellanslag i passwordet. Om inte för denna så skulle spelet krasha om du skrev en bokstav.
        {
            bool containsNumber = false;
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                {
                    if (c != ' ')
                    {
                        bool found = false;
                        foreach (char character in operators)
                        {
                            if (c == character)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            return false;
                        }
                    }
                }
                else if (!containsNumber && c > '0' && c < '9')
                {
                    containsNumber = true;
                }
            }
            if (!containsNumber)
            {
                return false;
            }
            return true;
        }

        public async Task SaveBotCoin()
        {
            using (TextWriter tw = new StreamWriter("botCoinSave.txt"))
            {
                for (int i = 0; i < botCoinSaves.Count; i++)
                {
                    if (botCoinSaves[i].userName == null)
                    {
                        tw.WriteLine(Convert.ToString(botCoinSaves[i].user) + " " + Convert.ToString(botCoinSaves[i].antalBotCoin) + " " + (botCoinSaves[i].senastTjänadePeng.ToString()));
                    }
                    else
                    {
                        tw.WriteLine(Convert.ToString(botCoinSaves[i].user) + " " + Convert.ToString(botCoinSaves[i].antalBotCoin) + " " + (botCoinSaves[i].senastTjänadePeng.ToString()) + " " + botCoinSaves[i].userName);
                    }
                }
            }
            await WriteLine("Sparade alla " + botCoinSaves.Count + " botcoin användares botcoin.");
        }

        private async Task ReadBotCoin()
        {
            botCoinSaves = new List<BotCoinSaveData>();
            string[] tempArray = await File.ReadAllLinesAsync("botCoinSave.txt");

            for (int i = 0; i < tempArray.Length; i++)
            {
                //if (IsDigitsOnly(tempArray[i], ":-"))
                //{
                try
                {
                    string[] temp = tempArray[i].Split(" ");
                    if (temp.Length == 4)
                    {
                        ulong name = (ulong)Convert.ToDecimal(temp[0]);
                        int antalBotCoin = Convert.ToInt32(temp[1]);
                        DateTime senastTjänadePeng = Convert.ToDateTime(temp[2] + " " + temp[3]);
                        if (!botCoinSaves.Any(a => a.user == name))
                        {
                            botCoinSaves.Add(new BotCoinSaveData(name, antalBotCoin, senastTjänadePeng));
                        }
                    }
                    else if (temp.Length == 5)
                    {
                        ulong name = (ulong)Convert.ToDecimal(temp[0]);
                        int antalBotCoin = Convert.ToInt32(temp[1]);
                        DateTime senastTjänadePeng = Convert.ToDateTime(temp[2] + " " + temp[3]);
                        string userName = temp[4];
                        if (!botCoinSaves.Any(a => a.user == name))
                        {
                            botCoinSaves.Add(new BotCoinSaveData(name, antalBotCoin, senastTjänadePeng, userName));
                        }
                    }
                }
                catch (Exception e)
                {
                    await WriteLine(e.Message);
                }
                //}
            }
            //tw.WriteLine(Convert.ToString(botCoinSaves[i].user) + " " + Convert.ToString(botCoinSaves[i].antalBotCoin) + " " + (botCoinSaves[i].senastTjänadePeng.ToString()));
        }

        public static void GiveBotCoin(CommandContext ctx)
        {
            int i = BotCoinIndex(ctx);
            if (i > -1)
            {
                if (botCoinSaves[i].userName == null)
                {
                    botCoinSaves[i].userName = ctx.Message.Author.Username;
                }
                TimeSpan timeSpan = DateTime.Now - botCoinSaves[i].senastTjänadePeng;
                if (timeSpan.TotalMinutes > 1)
                {
                    int max = Convert.ToInt32(Math.Clamp(timeSpan.TotalMinutes, 0, 15));
                    int earnedCoins = Convert.ToInt32(Math.Floor(Math.Abs(rng.NextDouble() - rng.NextDouble()) * (1 + max - 0) + 2));
                    botCoinSaves[i].antalBotCoin += earnedCoins;
                    botCoinSaves[i].senastTjänadePeng = DateTime.Now;
                    return;
                }
            }
        }

        public static int AddBotCoins(CommandContext ctx, bool vann)
        {
            int extraPeng = 0;
            int i = BotCoinIndex(ctx);
            if (i > -1)
            {
                TimeSpan timeSpan = DateTime.Now - botCoinSaves[i].senastTjänadePeng;
                if (timeSpan.TotalMinutes > 1)
                {
                    int max = Convert.ToInt32(Math.Clamp(timeSpan.TotalMinutes, 0, 15));
                    int earnedCoins = Convert.ToInt32(Math.Floor(Math.Abs(rng.NextDouble() - rng.NextDouble()) * (1 + max - 0) + 0));
                    extraPeng = (int)(earnedCoins * rng.NextDouble());
                    botCoinSaves[i].antalBotCoin += earnedCoins;
                    botCoinSaves[i].senastTjänadePeng = DateTime.Now;
                    return extraPeng;
                }
            }
            return extraPeng;
        }

        public static int BotCoinIndex(CommandContext ctx)
        {
            for (int i = 0; i < botCoinSaves.Count; i++)
            {
                if (botCoinSaves[i].user == ctx.Message.Author.Id)
                {
                    return i;
                }
            }
            string command = "botcoin";
            var cmds = ctx.CommandsNext;

            // retrieve the command and its arguments from the given string
            var cmd = cmds.FindCommand(command, out var customArgs);
            //for (int i = 1; i < command.Length; i++)
            //{
            //    customArgs += command[i] + " ";
            //}
            // create a fake CommandContext
            var fakeContext = cmds.CreateFakeContext(ctx.Member, ctx.Channel, command, ctx.Prefix, cmd, customArgs);

            // and perform the sudo
            cmds.ExecuteCommandAsync(fakeContext);
            return -1;
        }

        private static int[] ChannelIndex(MessageCreateEventArgs ctx)
        {
            for (int i = 0; i < kanalerna.Count; i++)
            {
                if (kanalerna[i].discordChannel == ctx.Channel.Id)
                {
                    for (int a = 0; a < kanalerna[i].discordUsers.Count; a++)
                    {
                        if (kanalerna[i].discordUsers[a].user == ctx.Message.Author.Id)
                        {
                            return new int[2] { i, a };
                        }
                    }
                    return new int[2] { i, -1 };
                }
            }
            return new int[2] { -1, -1 };
        }

        public static int[] ChannelIndex(CommandContext ctx)
        {
            for (int i = 0; i < kanalerna.Count; i++)
            {
                if (kanalerna[i].discordChannel == ctx.Channel.Id)
                {
                    for (int a = 0; a < kanalerna[i].discordUsers.Count; a++)
                    {
                        if (kanalerna[i].discordUsers[a].user == ctx.Message.Author.Id)
                        {
                            return new int[2] { i, a };
                        }
                    }
                    return new int[2] { i, -1 };
                }
            }
            return new int[2] { -1, -1 };
        }

        private static void WriteLatestMessage(MessageCreateEventArgs ctx)
        {
            int[] channel = ChannelIndex(ctx);
            if (channel[0] < 0)
            {
                kanalerna.Add(new ChannelSaveData(ctx.Channel));
                channel[0] = kanalerna.Count - 1;
            }
            if (channel[1] < 0)
            {
                kanalerna[channel[0]].discordUsers.Add(new DiscordMemberSaveData(ctx.Author.Id));
                channel[1] = kanalerna[channel[0]].discordUsers.Count - 1;
            }
            kanalerna[channel[0]].discordUsers[channel[1]].AddLatestMessage(ctx.Message.Content);
        }

        private bool LatestMessageSpam(MessageCreateEventArgs ctx)
        {
            int[] channel = ChannelIndex(ctx);
            if (channel[0] > -1 && channel[1] > -1)
            {
                return kanalerna[channel[0]].discordUsers[channel[1]].IsSpam(10, ctx.Message.Content, 2);
            }
            return false;
        }

        public static string GenerateQuote()
        {
            string quote = "";
            //int random = rng.Next(quoteTemplates.Length);
            string[] uppsplitad = quoteTemplates[rng.Next(quoteTemplates.Length)].Split("*");
            string noun = "";
            string emotion = "";
            for (int i = 0; i < uppsplitad.Length; i++)
            {
                if (uppsplitad[i] == "sp")
                {
                    if (noun == string.Empty)
                    {
                        noun = nouns[rng.Next(nouns.Length)];
                        noun = GuessPlural(noun);
                    }
                    uppsplitad[i] = noun;
                }
                else if (uppsplitad[i] == "s")
                {
                    if (noun == string.Empty)
                    {
                        noun = nouns[rng.Next(nouns.Length)];
                    }
                    uppsplitad[i] = noun;
                }
                else if (uppsplitad[i] == "k")
                {
                    if (emotion == string.Empty)
                    {
                        emotion = emotions[rng.Next(emotions.Length)];
                    }
                    uppsplitad[i] = emotion;
                }

                quote += uppsplitad[i];
                if (i == 0 && !char.IsUpper(uppsplitad[0][0]))
                {
                    uppsplitad[i] = FirstCharToUpper(uppsplitad[0]);
                }
            }

            quote += "\n- Gustavs bot.";
            return quote;
        }

        private static string GuessPlural(string str)
        {
            string[] singularNoun = new string[6] { "s", "ss", "sh", "ch", "x", "z" };
            for (int i = 0; i < singularNoun.Length; i++)
            {
                if (str.EndsWith(singularNoun[i]))
                {
                    str += "es";
                    return str;
                }
            }

            string[] fNoun = new string[2] { "f", "fe" };
            for (int i = 0; i < fNoun.Length; i++)
            {
                if (str.EndsWith(fNoun[i]))
                {
                    StringBuilder sb = new StringBuilder(str);
                    int a = str.LastIndexOf("f");
                    sb[a] = 'v';
                    str = sb.ToString();
                    if (a + 1 < str.Length)
                    {
                        str += "e";
                    }
                    str += "s";
                    return str;
                }
            }

            if (str.EndsWith("y"))
            {
                bool found = false;
                char[] vocals = new char[9] { 'a', 'e', 'i', 'o', 'u', 'y', 'å', 'ä', 'ö' };
                for (int i = 0; i < vocals.Length; i++)
                {
                    if (str[str.Length - 2] == vocals[i])
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    StringBuilder sb = new StringBuilder(str);
                    sb[sb.Length - 1] = 'i';
                    str = sb.ToString();
                    str += "es";
                    return str;
                }
            }

            if (str.EndsWith("o"))
            {
                str += "es";
                return str;
            }
            if (str.EndsWith("us"))
            {
                int usPos = str.LastIndexOf("us");
                str.Remove(usPos);
                str += "i";
                return str;
            }
            if (str.EndsWith("is"))
            {
                int isPos = str.LastIndexOf("is");
                str.Remove(isPos);
                str += "es";
                return str;
            }
            if (str.EndsWith("on"))
            {
                int isPos = str.LastIndexOf("on");
                str.Remove(isPos);
                str += "a";
                return str;
            }

            str += "s";
            return str;
        }

        public static string FirstCharToUpper(string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input.First().ToString().ToUpper() + input.Substring(1)
        };

        public static List<int> AllIndexesOf(string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }

        public class RemindmeSave
        {
            public DateTime dateTime { private set; get; }
            public string username { private set; get; }

            public RemindmeSave(DateTime _dateTime, string _username)
            {
                username = _username;
                dateTime = _dateTime;
            }
        }

        public class AdventureSaveData
        {
            public ulong user;
            public List<string> fyrkanten = new List<string>();
            public int[] position = new int[2];
            public int[] boxend = new int[2];

            public AdventureSaveData(ulong _user, List<string> _fyrkanten, int[] _position, int[] _boxend)
            {
                user = _user;
                fyrkanten = _fyrkanten;
                position = _position;
                boxend = _boxend;
                boxend[0] = 10;
                boxend[1] = 5;
            }
        }

        public class BotCoinSaveData
        {
            public ulong user;
            public int antalBotCoin;
            public DateTime senastTjänadePeng;
            public string userName;

            public BotCoinSaveData(ulong _user, int _antalBotCoin, DateTime _senastTjänadePeng, string name)
            {
                user = _user;
                antalBotCoin = _antalBotCoin;
                senastTjänadePeng = _senastTjänadePeng;
                userName = name;
            }

            public BotCoinSaveData(ulong _user, int _antalBotCoin, DateTime _senastTjänadePeng)
            {
                user = _user;
                antalBotCoin = _antalBotCoin;
                senastTjänadePeng = _senastTjänadePeng;
            }
        }

        public class GuildSaveData
        {
            public bool finished = false;
            public List<ChannelSaveData> kanalerIGuild = new List<ChannelSaveData>();
            public ulong guild;
            public DiscordGuild discordGuild;

            public GuildSaveData(List<ChannelSaveData> _kanalerIGuild)
            {
                kanalerIGuild = _kanalerIGuild;
                finished = true;
            }

            public GuildSaveData(DiscordGuild _discordGuild)
            {
                discordGuild = _discordGuild;
                finished = false;
            }

            public GuildSaveData(ulong _guild)
            {
                guild = _guild;
                finished = false;
            }
        }

        [Serializable]
        public class ChannelSaveData
        {
            public ulong discordChannel;
            public bool finished = false;
            public DiscordChannel realDiscordChannel;
            public List<DiscordMemberSaveData> discordUsers = new List<DiscordMemberSaveData>();
            public string[] membersToAdd;

            public ChannelSaveData(DiscordChannel _discordChannel)
            {
                realDiscordChannel = _discordChannel;
                discordChannel = realDiscordChannel.Id;
                finished = true;
            }

            public ChannelSaveData(ulong _discordChannel)
            {
                discordChannel = _discordChannel;
                finished = false;
            }
        }

        public class DiscordUserSaveData
        {
            public ulong user;

            public string[] latestMessages = new string[3];
            public DateTime[] timeOfLatestMessage = new DateTime[3];

            public DiscordUserSaveData(ulong _user)
            {
                user = _user;
            }

            public void AddLatestMessage(string newMessage)
            {
                for (int i = 0; i < latestMessages.Length; i++)
                {
                    timeOfLatestMessage[i] = timeOfLatestMessage[i + 1 < timeOfLatestMessage.Length ? i + 1 : i];
                }
                timeOfLatestMessage[timeOfLatestMessage.Length - 1] = DateTime.Now;
                for (int i = 0; i < latestMessages.Length; i++)
                {
                    latestMessages[i] = latestMessages[i + 1 < latestMessages.Length ? i + 1 : i];
                }
                latestMessages[latestMessages.Length - 1] = newMessage;
            }

            public bool IsSpam(int seconds, string message, int amountEqual)
            {
                TimeSpan timeSpan = DateTime.Now - DateTime.Now;
                TimeSpan temp;
                for (int i = 0; i < timeOfLatestMessage.Length; i++)
                {
                    temp = timeOfLatestMessage[i] - timeOfLatestMessage[i - 1 > 0 ? i - 1 : i];
                    timeSpan.Add(temp);
                }
                temp = DateTime.Now - timeOfLatestMessage[timeOfLatestMessage.Length - 1];
                timeSpan.Add(temp);
                timeSpan.Divide(4);
                if (timeSpan.TotalSeconds < seconds)
                {
                    return false;
                }
                //TimeSpan timeSpan = DateTime.Now - timeOfLatestMessage[timeOfLatestMessage.Length];
                int amountfound = 0;
                if (timeSpan.TotalMinutes < seconds)
                {
                    for (int i = 0; i < latestMessages.Length; i++)
                    {
                        if (latestMessages[i] == message)
                        {
                            amountfound++;
                            if (amountfound >= amountEqual)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        [Serializable]
        public class DiscordMemberSaveData
        {
            public DiscordMember member;
            public ulong user;

            public string[] latestMessages = new string[3];
            public DateTime[] timeOfLatestMessage = new DateTime[3];

            public DiscordMemberSaveData(DiscordMember _member)
            {
                member = _member;
                user = member.Id;
            }

            public DiscordMemberSaveData(ulong id)
            {
                user = id;
            }

            public void AddLatestMessage(string newMessage)
            {
                for (int i = 0; i < latestMessages.Length; i++)
                {
                    timeOfLatestMessage[i] = timeOfLatestMessage[i + 1 < timeOfLatestMessage.Length ? i + 1 : i];
                }
                timeOfLatestMessage[timeOfLatestMessage.Length - 1] = DateTime.Now;
                for (int i = 0; i < latestMessages.Length; i++)
                {
                    latestMessages[i] = latestMessages[i + 1 < latestMessages.Length ? i + 1 : i];
                }
                latestMessages[latestMessages.Length - 1] = newMessage;
            }

            public bool IsSpam(int seconds, string message, int amountEqual)
            {
                TimeSpan timeSpan = DateTime.Now - DateTime.Now;
                TimeSpan temp;
                for (int i = 0; i < timeOfLatestMessage.Length; i++)
                {
                    temp = timeOfLatestMessage[i] - timeOfLatestMessage[i - 1 > 0 ? i - 1 : i];
                    timeSpan.Add(temp);
                }
                temp = DateTime.Now - timeOfLatestMessage[timeOfLatestMessage.Length - 1];
                timeSpan.Add(temp);
                timeSpan.Divide(4);
                if (timeSpan.TotalSeconds < seconds)
                {
                    return false;
                }
                //TimeSpan timeSpan = DateTime.Now - timeOfLatestMessage[timeOfLatestMessage.Length];
                int amountfound = 0;
                if (timeSpan.TotalMinutes < seconds)
                {
                    for (int i = 0; i < latestMessages.Length; i++)
                    {
                        if (latestMessages[i] == message)
                        {
                            amountfound++;
                            if (amountfound >= amountEqual)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        public class ScreenCapture
        {
            /// <summary>
            /// Creates an Image object containing a screen shot of the entire desktop
            /// </summary>
            /// <returns></returns>
            public Image CaptureScreen()
            {
                //void ImageExampleForm_Paint(object sender, PaintEventArgs e)
                //{
                //    // Create image.
                //    Image newImage = Image.FromFile("SampImag.jpg");

                //    // Create Point for upper-left corner of image.
                //    Point ulCorner = new Point(100, 100);

                //    // Draw image to screen.
                //    e.Graphics.DrawImage(newImage, ulCorner);
                //}
                return CaptureWindow(User32.GetDesktopWindow());
            }

            /// <summary>
            /// Creates an Image object containing a screen shot of a specific window
            /// </summary>
            /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>
            /// <returns></returns>
            public Image CaptureWindow(IntPtr handle)
            {
                // get te hDC of the target window
                IntPtr hdcSrc = User32.GetWindowDC(handle);
                // get the size
                User32.RECT windowRect = new User32.RECT();
                User32.GetWindowRect(handle, ref windowRect);
                int width = windowRect.right - windowRect.left;
                int height = windowRect.bottom - windowRect.top;
                // create a device context we can copy to
                IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
                // create a bitmap we can copy it to,
                // using GetDeviceCaps to get the width/height
                IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
                // select the bitmap object
                IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
                // bitblt over
                GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
                // restore selection
                GDI32.SelectObject(hdcDest, hOld);
                // clean up
                GDI32.DeleteDC(hdcDest);
                User32.ReleaseDC(handle, hdcSrc);
                // get a .NET image object for it
                Image img = Image.FromHbitmap(hBitmap);
                // free up the Bitmap object
                GDI32.DeleteObject(hBitmap);
                return img;
            }

            /// <summary>
            /// Captures a screen shot of a specific window, and saves it to a file
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="filename"></param>
            /// <param name="format"></param>
            public void CaptureWindowToFile(string filename, System.Drawing.Imaging.ImageFormat format)
            {
                IntPtr handle = FindWindow("Bot", "DiscordBot.exe");
                Image img = CaptureWindow(handle);
                img.Save(filename, format);
            }

            /// <summary>
            /// Captures a screen shot of the entire desktop, and saves it to a file
            /// </summary>
            /// <param name="filename"></param>
            /// <param name="format"></param>
            public void CaptureScreenToFile(string filename, System.Drawing.Imaging.ImageFormat format)
            {
                Image img = CaptureScreen();
                img.Save(filename, format);
            }

            public void CaptureScreenToFile(string filename, System.Drawing.Imaging.ImageFormat format, Image img)
            {
                //Image img = CaptureScreen();
                img.Save(filename, format);
            }

            [DllImport("user32.dll", SetLastError = true)]
            static public extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            public IntPtr hWnd = FindWindow("Bot", "DiscordBot.exe");

            /// <summary>
            /// Helper class containing Gdi32 API functions
            /// </summary>
            private class GDI32
            {
                public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter

                [DllImport("gdi32.dll")]
                public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                    int nWidth, int nHeight, IntPtr hObjectSource,
                    int nXSrc, int nYSrc, int dwRop);

                [DllImport("gdi32.dll")]
                public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                    int nHeight);

                [DllImport("gdi32.dll")]
                public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

                [DllImport("gdi32.dll")]
                public static extern bool DeleteDC(IntPtr hDC);

                [DllImport("gdi32.dll")]
                public static extern bool DeleteObject(IntPtr hObject);

                [DllImport("gdi32.dll")]
                public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
            }

            private class User32
            {
                [StructLayout(LayoutKind.Sequential)]
                public struct RECT
                {
                    public int left;
                    public int top;
                    public int right;
                    public int bottom;
                }

                [DllImport("user32.dll")]
                public static extern IntPtr GetDesktopWindow();

                [DllImport("user32.dll")]
                public static extern IntPtr GetWindowDC(IntPtr hWnd);

                [DllImport("user32.dll")]
                public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

                [DllImport("user32.dll")]
                public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
            }
        }

        public class ScreenShootingShit
        {
            [DllImport("user32.dll")]
            private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip,
           MonitorEnumDelegate lpfnEnum, IntPtr dwData);

            private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

            [StructLayout(LayoutKind.Sequential)]
            public struct Rect
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            /// <summary>
            /// The struct that contains the display information
            /// </summary>
            public class DisplayInfo
            {
                public string Availability { get; set; }
                public string ScreenHeight { get; set; }
                public string ScreenWidth { get; set; }
                public Rect MonitorArea { get; set; }
                public Rect WorkArea { get; set; }
                public IntPtr hMonitor { get; set; }
            }

            /// <summary>
            /// Collection of display information
            /// </summary>
            public class DisplayInfoCollection : List<DisplayInfo>
            {
            }

            [DllImport("user32.dll")]
            private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

            [DllImport("user32.dll")]
            private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

            // size of a device name string
            private const int CCHDEVICENAME = 32;

            /// <summary>
            /// The MONITORINFOEX structure contains information about a display monitor.
            /// The GetMonitorInfo function stores information into a MONITORINFOEX structure or a MONITORINFO structure.
            /// The MONITORINFOEX structure is a superset of the MONITORINFO structure. The MONITORINFOEX structure adds a string member to contain a name
            /// for the display monitor.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            internal struct MonitorInfoEx
            {
                /// <summary>
                /// The size, in bytes, of the structure. Set this member to sizeof(MONITORINFOEX) (72) before calling the GetMonitorInfo function.
                /// Doing so lets the function determine the type of structure you are passing to it.
                /// </summary>
                public int Size;

                /// <summary>
                /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates.
                /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
                /// </summary>
                public Rect Monitor;

                /// <summary>
                /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by applications,
                /// expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an application on the monitor.
                /// The rest of the area in rcMonitor contains system windows such as the task bar and side bars.
                /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
                /// </summary>
                public Rect WorkArea;

                /// <summary>
                /// The attributes of the display monitor.
                ///
                /// This member can be the following value:
                ///   1 : MONITORINFOF_PRIMARY
                /// </summary>
                public uint Flags;

                /// <summary>
                /// A string that specifies the device name of the monitor being used. Most applications have no use for a display monitor name,
                /// and so can save some bytes by using a MONITORINFO structure.
                /// </summary>
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
                public string DeviceName;

                public void Init()
                {
                    this.Size = 72;
                    this.DeviceName = string.Empty;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct MonitorInfo
            {
                /// <summary>
                /// The size, in bytes, of the structure. Set this member to sizeof(MONITORINFO) (40) before calling the GetMonitorInfo function.
                /// Doing so lets the function determine the type of structure you are passing to it.
                /// </summary>
                public int Size;

                /// <summary>
                /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates.
                /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
                /// </summary>
                public Rect Monitor;

                /// <summary>
                /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by applications,
                /// expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an application on the monitor.
                /// The rest of the area in rcMonitor contains system windows such as the task bar and side bars.
                /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
                /// </summary>
                public Rect WorkArea;

                /// <summary>
                /// The attributes of the display monitor.
                ///
                /// This member can be the following value:
                ///   1 : MONITORINFOF_PRIMARY
                /// </summary>
                public uint Flags;

                public void Init()
                {
                    this.Size = 40;
                }
            }

            /// <summary>
            /// Returns the number of Displays using the Win32 functions
            /// </summary>
            /// <returns>collection of Display Info</returns>
            public DisplayInfoCollection GetDisplays()
            {
                DisplayInfoCollection col = new DisplayInfoCollection();
                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                    delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
                    {
                        MonitorInfo mi = new MonitorInfo();
                        mi.Size = (int)Marshal.SizeOf(mi);
                        bool success = GetMonitorInfo(hMonitor, ref mi);
                        if (success)
                        {
                            DisplayInfo di = new DisplayInfo();
                            di.ScreenWidth = (mi.Monitor.right - mi.Monitor.left).ToString();
                            di.ScreenHeight = (mi.Monitor.bottom - mi.Monitor.top).ToString();
                            di.MonitorArea = mi.Monitor;
                            di.WorkArea = mi.WorkArea;
                            di.Availability = mi.Flags.ToString();
                            di.hMonitor = hMonitor;

                            col.Add(di);
                        }
                        return true;
                    }, IntPtr.Zero);
                return col;
            }

            public int GetTotalMonitors()
            {
                return GetDisplays().Count;
            }
        }

        /// <summary>Contains functionality to get all the open windows.</summary>
        public static class OpenWindowGetter
        {
            /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
            /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
            public static IDictionary<HWND, string> GetOpenWindows()
            {
                HWND shellWindow = GetShellWindow();
                Dictionary<HWND, string> windows = new Dictionary<HWND, string>();

                EnumWindows(delegate (HWND hWnd, int lParam)
                {
                    if (hWnd == shellWindow) return true;
                    if (!IsWindowVisible(hWnd)) return true;

                    int length = GetWindowTextLength(hWnd);
                    if (length == 0) return true;

                    StringBuilder builder = new StringBuilder(length);
                    GetWindowText(hWnd, builder, length + 1);

                    windows[hWnd] = builder.ToString();
                    return true;
                }, 0);

                return windows;
            }

            private delegate bool EnumWindowsProc(HWND hWnd, int lParam);

            [DllImport("USER32.DLL")]
            private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

            [DllImport("USER32.DLL")]
            private static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

            [DllImport("USER32.DLL")]
            private static extern int GetWindowTextLength(HWND hWnd);

            [DllImport("USER32.DLL")]
            public static extern bool IsWindowVisible(HWND hWnd);

            [DllImport("USER32.DLL")]
            public static extern IntPtr GetShellWindow();
        }
    }
}