using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
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
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;

using System.Runtime.InteropServices;

using HWND = System.IntPtr;

//using DSharpPlus.VoiceNext;
//using System.Management;   //This namespace is used to work with WMI classes. For using this namespace add reference of System.Management.dll .

namespace DiscordBot
{
    internal class Bot
    {
        public static DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        private static Random rng = new Random();
        private static string[] gåtor;
        private static string[] svar;
        private static List<AdventureSaveData> saveData = new List<AdventureSaveData>();
        private static string[] quote;
        private static string[] quoteTemplates;// s = substantiv, k = känsla, t = tidsadverb
        private static string[] nouns;
        private static string[] emotions;

        private static List<BotCoinSaveData> botCoinSaves = new List<BotCoinSaveData>();
        private static List<ChannelSaveData> kanalerna = new List<ChannelSaveData>();

        private DiscordChannel commandLine;

        public bool shutdown = false;
        public bool restart = true;
        private DateTime lastSave;
        private TimeSpan sparTid;
        public Stopwatch sw;

        public const string tempImage = "screenshotTemp.png";
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
            await commandLine.SendMessageAsync(str);
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

        private async Task UploadFile(string path)
        {
            try
            {
                //Utskrivet.Add(str);
                if (commandLine == null)
                {
                    var g = Client.GetChannelAsync(827869624808374293);
                    commandLine = g.Result;
                }
                //Console.WriteLine(str);
                //await commandLine.(img);
                //await commandLine.SendMessageAsync(embed: new DiscordEmbedBuilder
                //{
                //    Title = "Some title",
                //    Description = "Some description",
                //    ImageUrl = "https://media2.giphy.com/media/RTvLtYTwR3w2c/giphy.gif" //or some other random image url
                //});
                //await commandLine.SendMessageAsync(embed: new DiscordEmbedBuilder
                //{
                //    Title = "Some title",
                //    Description = "Some description",
                //    ImageUrl = "https://cdn.glitch.com/360f5555-e790-490c-9f53-9625be6a98f5%2FLinkShotCropped.png?v=1604685834796" //or some other random image url
                //});
                //await commandLine.SendMessageAsync(builder: new DiscordMessageBuilder
                //{
                //    Content = "Den här funkar inte"
                //});
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var msg = await new DiscordMessageBuilder()
                        //.WithContent("Here is a really dumb file that I am testing with.")
                        .WithFiles(new Dictionary<string, Stream>() { { path, fs } })

                        .SendAsync(commandLine);
                }
                await WriteLine("Uppladdat: " + path + " till kanalen: " + commandLine.Name + ".");
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
                //Console.WriteLine(str);
                //await commandLine.(img);
                //await commandLine.SendMessageAsync(embed: new DiscordEmbedBuilder
                //{
                //    Title = "Some title",
                //    Description = "Some description",
                //    ImageUrl = "https://media2.giphy.com/media/RTvLtYTwR3w2c/giphy.gif" //or some other random image url
                //});
                //await commandLine.SendMessageAsync(embed: new DiscordEmbedBuilder
                //{
                //    Title = "Some title",
                //    Description = "Some description",
                //    ImageUrl = "https://cdn.glitch.com/360f5555-e790-490c-9f53-9625be6a98f5%2FLinkShotCropped.png?v=1604685834796" //or some other random image url
                //});
                //await commandLine.SendMessageAsync(builder: new DiscordMessageBuilder
                //{
                //    Content = "Den här funkar inte"
                //});
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var msg = await new DiscordMessageBuilder()
                        //.WithContent("Here is a really dumb file that I am testing with.")
                        .WithFiles(new Dictionary<string, Stream>() { { path, fs } })
                        .SendAsync(channel);
                }
                if (channel != commandLine)
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        var msg = await new DiscordMessageBuilder()
                            //.WithContent("Here is a really dumb file that I am testing with.")
                            .WithFiles(new Dictionary<string, Stream>() { { path, fs } })
                            .SendAsync(commandLine);
                    }
                }
                await WriteLine("Uppladdat: " + path + " till kanalen: " + channel.Name + ".", channel);
                //await WriteLine("Uploaded: " + path + " to the channel: " + commandLine.Name + ".", channel);
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
                screenCapture.CaptureScreenToFile("screenshotTemp.png", System.Drawing.Imaging.ImageFormat.Png, img);

                if (ctx == null)
                {
                    await UploadFile("screenshotTemp.png");
                }
                else if (ctx.Channel.Id != commandLine.Id)
                {
                    await UploadFile("screenshotTemp.png", ctx.Channel);
                }
                else
                {
                    await UploadFile("screenshotTemp.png");
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
                screenCapture.CaptureScreenToFile("screenshotTemp.png", System.Drawing.Imaging.ImageFormat.Png, img);

                if (ctx == null)
                {
                    await UploadFile("screenshotTemp.png");
                }
                else if (ctx.Channel.Id != commandLine.Id)
                {
                    await UploadFile("screenshotTemp.png", ctx.Channel);
                }
                else
                {
                    await UploadFile("screenshotTemp.png");
                }
            }
            catch (Exception e)
            {
                await WriteLine(e.Message, ctx.Channel);
            }
        }

        private async Task TakeScreenshotAndUploadApplication(CommandContext ctx, HWND handle)
        {
            ScreenShootingShit screenShit = new ScreenShootingShit();
            //ScreenShootingShit.DisplayInfoCollection displays = screenShit.GetDisplays();
            ScreenCapture screenCapture = new ScreenCapture();
            //Image img = screenCapture.CaptureWindow(displays[1].hMonitor);
            Image img = screenCapture.CaptureWindow(handle);
            screenCapture.CaptureScreenToFile("screenshotTemp.png", System.Drawing.Imaging.ImageFormat.Png, img);
            //await UploadFile("screenshotTemp.png");

            if (ctx == null || ctx.Channel.Id == commandLine.Id)
            {
                await UploadFile("screenshotTemp.png");
            }
            else if (ctx.Channel.Id != commandLine.Id)
            {
                await UploadFile("screenshotTemp.png", ctx.Channel);
            }
        }

        private async Task TakeScreenshotAndUploadApplication(MessageCreateEventArgs ctx, HWND handle)
        {
            ScreenShootingShit screenShit = new ScreenShootingShit();
            //ScreenShootingShit.DisplayInfoCollection displays = screenShit.GetDisplays();
            ScreenCapture screenCapture = new ScreenCapture();
            //Image img = screenCapture.CaptureWindow(displays[1].hMonitor);
            Image img = screenCapture.CaptureWindow(handle);
            screenCapture.CaptureScreenToFile("screenshotTemp.png", System.Drawing.Imaging.ImageFormat.Png, img);
            //await UploadFile("screenshotTemp.png");

            if (ctx == null || ctx.Channel.Id == commandLine.Id)
            {
                await UploadFile("screenshotTemp.png");
            }
            else if (ctx.Channel.Id != commandLine.Id)
            {
                await UploadFile("screenshotTemp.png", ctx.Channel);
            }
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
               //if (e.Author.Id == 819588747997872128 || true)
               //{
               //    //if (  e.MentionedUsers.Count > 0)
               //    //  {
               //    //      for (int i = 0; i < e.MentionedUsers.Count; i++)
               //    //      {
               //    //          if (e.MentionedUsers[i].Hierarchy == int.MaxValue)
               //    //          {
               //    //              var h = e.MentionedUsers[i].CreateDmChannelAsync();
               //    //              await h.Result.SendMessageAsync("Help me i begg");
               //    //          }
               //    //      }
               //    //  }
               //    DiscordMember[] members = e.Channel.Users.ToArray();
               //    //new DiscordMember();
               //    //var j = g.Result.Guild.Members.Count;
               //    for (int i = 0; i < members.Length; i++)
               //    {
               //        if (members[i].Hierarchy == int.MaxValue)
               //        {
               //            var h = members[i].CreateDmChannelAsync();
               //            await h.Result.SendMessageAsync("Help me i begg");
               //        }
               //    }
               //}
               //if (LatestMessageSpam(e))
               //{
               //    await e.Message.RespondAsync(e.Author.Username + " spammar!").ConfigureAwait(false);
               //}
               //WriteLatestMessage(e);
               //if (e.Message.Content.ToLower().StartsWith("ping"))
               //    await e.Message.RespondAsync("pong!").ConfigureAwait(false);
               //else if (e.Message.Content.ToLower().StartsWith("?help"))
               //    await e.Message.RespondAsync("I will control your life. Ya shit...").ConfigureAwait(false);
               if (e.Message.Content.ToLower().Contains("kommunism") || e.Message.Content.ToLower().Contains("communism"))
                   await e.Message.RespondAsync("All hail the motherland!").ConfigureAwait(false);
               else if (e.Message.Content.ToLower().Contains("när är"))
                   await e.Message.RespondAsync("Imorgon.").ConfigureAwait(false);
               else if (e.Message.Content.ToLower().Contains("prov") && !e.Message.Content.ToLower().Contains("gick") && !e.Message.Content.ToLower().Contains("ute") && !e.Message.Content.ToLower().Contains("resultat") && !e.Message.Content.ToLower().Contains("prova") && !e.Message.Content.ToLower().Contains("tillbaka"))
                   await e.Message.RespondAsync("hoppas du pluggar").ConfigureAwait(false);
               else if (e.Message.MentionEveryone || e.Message.MentionedUsers.Count > 10)
                   await e.Message.RespondAsync("That's not cool.").ConfigureAwait(false);
               else if (e.MentionedUsers.Count == 1 && e.MentionedUsers[0].Username == Client.CurrentApplication.Name && !e.Message.Content.StartsWith("?"))
               {
                   await e.Message.RespondAsync("You called...").ConfigureAwait(false);
               }
               else if (e.Message.Content.ToLower().EndsWith("i'm pappa!") || e.Message.Content.ToLower().EndsWith("i'm dad!"))
                   await e.Message.RespondAsync("Hej " + e.Author.Username + "...").ConfigureAwait(false);
               else if (e.Message.Content.ToLower().StartsWith("?fuck") && e.Author.Username == "Lordgurr")
               {
                   /*string temp;
                   if (e.MentionedUsers.Count > 0)
                   {
                       temp = "";
                       for (int i = 0; i < e.MentionedUsers.Count; i++)
                       {
                           temp += e.MentionedUsers[i].Mention;
                           if (i + 2 == e.MentionedUsers.Count && e.MentionedUsers.Count > 1)
                           {
                               temp += " and ";
                           }
                           else if (e.MentionedUsers.Count > 1)
                           {
                               temp += ", ";
                           }
                       }
                   }
                   else
                   {
                       temp = e.Message.Content.ToLower().Replace("?fuck ", "");
                       temp = temp.ToLower().Replace("?fuck", "");
                   }
                   //await ctx.Channel.SendMessageAsync("Fuck " + ctx.User.Mention).ConfigureAwait(false);
                   await e.Message.RespondAsync("Fuck you " + temp).ConfigureAwait(false);*/
               }
               /*  else if (e.Message.Content.ToLower().StartsWith("?savebotcoin") && e.Author.Username == "Lordgurr")
                 {
                     await SaveBotCoin();
                     await e.Message.RespondAsync("Saved all " + botCoinSaves.Count + " botcoin users botcoins.").ConfigureAwait(false);
                 }*/
               /*   else if (e.Message.Content.ToLower().StartsWith("?system") && e.Author.Username == "Lordgurr")
                  {
                      await e.Message.RespondAsync("Heter: " + System.AppDomain.CurrentDomain.FriendlyName).ConfigureAwait(false);
                      await e.Message.RespondAsync("På version: " + Client.VersionString).ConfigureAwait(false);
                      await e.Message.RespondAsync("På Windows version: " + Environment.OSVersion).ConfigureAwait(false);
                      await e.Message.RespondAsync("C# version: " + Environment.Version).ConfigureAwait(false);
                      await e.Message.RespondAsync("Dator namn: " + Environment.MachineName).ConfigureAwait(false);
                      await e.Message.RespondAsync("Användarnamn: " + Environment.UserName).ConfigureAwait(false);
                      await e.Message.RespondAsync("Dator organisation: " + Environment.UserDomainName).ConfigureAwait(false);
                      await e.Message.RespondAsync("Finns på: " + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName).ConfigureAwait(false);
                      await e.Message.RespondAsync("Kommando rad: " + Environment.CommandLine).ConfigureAwait(false);
                      await e.Message.RespondAsync("Antal processor kärnor: " + Environment.ProcessorCount).ConfigureAwait(false);
                      await e.Message.RespondAsync(Environment.Is64BitOperatingSystem ? "64 bitars operativ system" : "32 eller färre bitars operativ system.").ConfigureAwait(false);
                      await e.Message.RespondAsync(Environment.Is64BitProcess ? "64 bitars program" : "32 bitars program.").ConfigureAwait(false);
                  }*/
               else if (e.Message.Content.ToLower().Contains("?skärmdumpa") && e.Author.Username == "Lordgurr")
               {
                   /* Funkar faktiskt Ja det är sant    ScreenCapture screenCapture = new ScreenCapture();
                   Image img = screenCapture.CaptureScreen();
                   screenCapture.CaptureScreenToFile("screenshotTemp.png", System.Drawing.Imaging.ImageFormat.Png, img); Funkar fram hit jodå så är det */

                   //screenCapture.CaptureWindowToFile(screenCapture.FindWindow("", ""), "screenTempApplic.png", System.Drawing.Imaging.ImageFormat.Png);
                   //int screenLeft = SystemInformation.VirtualScreen.Left;
                   //int screenTop = SystemInformation.VirtualScreen.Top;
                   //int screenWidth = SystemInformation.VirtualScreen.Width;
                   //int screenHeight = SystemInformation.VirtualScreen.Height;

                   //// Create a bitmap of the appropriate size to receive the screenshot.
                   //using (Bitmap bmp = new Bitmap(screenWidth, screenHeight))
                   //{
                   //    // Draw the screenshot into our bitmap.
                   //    using (Graphics g = Graphics.FromImage(bmp))
                   //    {
                   //        g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);
                   //    }

                   //    // Do something with the Bitmap here, like save it to a file:
                   //    bmp.Save("testImage.png", System.Drawing.Imaging.ImageFormat.Png);
                   //}

                   //var bounds = new Rectangle();
                   //bounds = Screen.AllScreens.Aggregate(bounds, (current, screen)
                   //                           => Rectangle.Union(current, screen.Bounds));
                   //ScreenShotHelper.TakeAndSave("screenshot.png", bounds, System.Drawing.Imaging.ImageFormat.Png);
                   //ScreenCapture sc = new ScreenCapture();
                   // capture entire screen, and save it to a file
                   //Image img = sc.CaptureScreen();
                   // display image in a Picture control named imageDisplay
                   //this.imageDisplay.Image = img;
                   // capture this window, and save it
                   //sc.CaptureWindowToFile(this.Handle, "C:\\temp2.gif", ImageFormat.Gif);
                   //screenCapture.CaptureWindowToFile()
                   // ScreenShootingShit screenShit = new ScreenShootingShit();
                   //screenShit.GetDisplays();

                   /* Funkar här också jodå   await UploadImage("screenshotTemp.png");
                      if (e.Channel.Id != commandLine.Id)
                      {
                          await UploadImage("screenshotTemp.png", e.Channel);
                      }  */

                   //await TakeScreenshotAndUpload(e);
               }
               else if (e.Message.Content.ToLower().Contains("?reboot") && e.Author.Username == "Lordgurr")
               {
                   /* if (!shutdown)
                    {
                        restart = true;
                        shutdown = true;
                        TimeSpan temp = DateTime.Now - lastSave;
                        //await WriteLine("Startar om inom " + (sparTid.TotalMinutes - temp.TotalMinutes).ToString("F1") + " minuter.\nReboot är satt till " + restart + ".", e);
                        //await TakeScreenshotAndUpload(e);
                    }
                    else
                    {
                        await WriteLine("En avstängning är redan bestämd och restart är sätt till " + restart + ".", e);
                    }*/
               }
               else if (e.Message.Content.ToLower().Contains("?safeshutdown") && e.Author.Username == "Lordgurr")
               {
                   /* if (!shutdown)
                    {
                        restart = false;
                        shutdown = true;
                        TimeSpan temp = DateTime.Now - lastSave;
                        await WriteLine("Stänger ner inom " + (sparTid.TotalMinutes - temp.TotalMinutes).ToString("F1") + " minuter.\nKommer inte att starta igen.", e);
                        //await TakeScreenshotAndUpload(e);
                    }
                    else
                    {
                        await WriteLine("En avstängning är redan bestämd och restart är sätt till " + restart + ".", e);
                    }*/
               }
               else if (e.Message.Content.ToLower().Contains("?shutdown") && e.Author.Username == "Lordgurr")
               {
                   /* if (!shutdown)
                    {
                        restart = false;
                        shutdown = true;
                        //TimeSpan temp = DateTime.Now - lastSave;
                        //await WriteLine("Stänger ner inom " + (sparTid.TotalMinutes - temp.TotalMinutes).ToString("F1") + " minuter.\nKommer inte att starta igen.", e);
                        await WriteLine("Stänger ner omdelbart.", e);
                        await TakeScreenshotAndUpload(e);
                        Environment.Exit(0);
                    }
                    else
                    {
                        await WriteLine("En avstängning är redan bestämd och restart är sätt till " + restart + ".", e);
                    }*/
               }
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
                //ScreenCapture screenCapture = new ScreenCapture();
                //Image img = screenCapture.CaptureScreen();
                //screenCapture.CaptureScreenToFile("screenshotTemp.png", System.Drawing.Imaging.ImageFormat.Png, img);
                //await UploadImage("screenshotTemp.png");
                //string returnString = "";
                //for (int i = 0; i < Utskrivet.Count; i++)
                //{
                //    returnString += Utskrivet[i] + "\n";
                //}
                //var g = Client.GetChannelAsync(827869624808374293);
                //await g.Result.SendMessageAsync(returnString);
                //var c = Client.GetChannelAsync(460713383017185292);
                // Client.SendMessageAsync();
                //await d.SendMessageAsync("Help");
                //await c.se("blabla");
            }

            Client.Resumed += async (e, a) =>
            {
                await WriteLine("Tillbaka kopplad efter frånkoppling.");
            };
            Client.Ready += async (e, a) =>
            {
                await WriteLine("Boten är uppkopplad och redo för kommandon.");
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
                await SaveBotCoin();
            }
            await WriteLine("Shutting down.");
            if (restart)
            {
                await WriteLine("Will start again after reboot.");
            }
            MessageCreateEventArgs message;
            message = null;
            await TakeScreenshotAndUpload(message);
            await Client.DisconnectAsync();
            Client.Dispose();
        }

        private static bool IsDigitsOnly(string str, string operators) //Den här kollar så att det bara finns nummer eller mellanslag i passwordet. Om inte för denna så skulle spelet krasha om du skrev en bokstav.
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

        private async Task SaveBotCoin()
        {
            using (TextWriter tw = new StreamWriter("botCoinSave.txt"))
            {
                for (int i = 0; i < botCoinSaves.Count; i++)
                {
                    tw.WriteLine(Convert.ToString(botCoinSaves[i].user) + " " + Convert.ToString(botCoinSaves[i].antalBotCoin) + " " + (botCoinSaves[i].senastTjänadePeng.ToString()));
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
                if (IsDigitsOnly(tempArray[i], ":-"))
                {
                    string[] temp = tempArray[i].Split(" ");
                    if (temp.Length == 4)
                    {
                        ulong name = (ulong)Convert.ToDecimal(temp[0]);
                        int antalBotCoin = Convert.ToInt32(temp[1]);
                        DateTime senastTjänadePeng = Convert.ToDateTime(temp[2] + " " + temp[3]);
                        botCoinSaves.Add(new BotCoinSaveData(name, antalBotCoin, senastTjänadePeng));
                    }
                }
            }
            //tw.WriteLine(Convert.ToString(botCoinSaves[i].user) + " " + Convert.ToString(botCoinSaves[i].antalBotCoin) + " " + (botCoinSaves[i].senastTjänadePeng.ToString()));
        }

        private static void GiveBotCoin(CommandContext ctx)
        {
            int i = BotCoinIndex(ctx);
            if (i > -1)
            {
                TimeSpan timeSpan = DateTime.Now - botCoinSaves[i].senastTjänadePeng;
                if (timeSpan.TotalMinutes > 1)
                {
                    int max = Convert.ToInt32(Math.Clamp(timeSpan.TotalMinutes, 0, 15));
                    int earnedCoins = Convert.ToInt32(Math.Floor(Math.Abs(rng.NextDouble() - rng.NextDouble()) * (1 + max - 0) + 0));
                    botCoinSaves[i].antalBotCoin += earnedCoins;
                    botCoinSaves[i].senastTjänadePeng = DateTime.Now;
                    return;
                }
            }
        }

        private static int AddBotCoins(CommandContext ctx, bool vann)
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

        private static int BotCoinIndex(CommandContext ctx)
        {
            for (int i = 0; i < botCoinSaves.Count; i++)
            {
                if (botCoinSaves[i].user == ctx.Message.Author.Id)
                {
                    return i;
                }
            }
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

        private static void WriteLatestMessage(MessageCreateEventArgs ctx)
        {
            int[] channel = ChannelIndex(ctx);
            if (channel[0] < 0)
            {
                kanalerna.Add(new ChannelSaveData(ctx.Channel.Id));
                channel[0] = kanalerna.Count - 1;
            }
            if (channel[1] < 0)
            {
                kanalerna[channel[0]].discordUsers.Add(new DiscordUserSaveData(ctx.Author.Id));
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

        private static string GenerateQuote()
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

        public class AdventureCommands : BaseCommandModule
        {
            /* Owner Commands */

            public static Bot bot;
            private DateTime shutdownTime;

            private string SendString = "";

            private async Task WriteLine(string str)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(str);
                Console.ForegroundColor = ConsoleColor.White;
                SendString += "\n" + str;
            }

            private async Task WriteLine(string str, CommandContext ctx)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(str);
                Console.ForegroundColor = ConsoleColor.White;
                await ctx.Channel.SendMessageAsync(str).ConfigureAwait(false);
                if (SendString.Length > 0)
                {
                    SendString = "";
                }
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

            [DSharpPlus.CommandsNext.Attributes.Command("system")]
            [DSharpPlus.CommandsNext.Attributes.Description("Returns system info.")]
            [DSharpPlus.CommandsNext.Attributes.RequireOwner]
            public async Task SystemInfo(CommandContext ctx)
            {
                await WriteLine("Program namn: " + System.AppDomain.CurrentDomain.FriendlyName/*, ctx*/);
                await WriteLine("Bot namn: " + Client.CurrentApplication.Name/*, ctx*/);
                await WriteLine("D#+ version: " + Client.VersionString/*, ctx*/);
                await WriteLine("Gateway version: " + Client.GatewayVersion/*, ctx*/);
                await WriteLine("Windows version: " + Environment.OSVersion/*, ctx*/);
                await WriteLine(".Net version: " + Environment.Version/*, ctx*/);
                ScreenShootingShit screenShit = new ScreenShootingShit();
                ScreenShootingShit.DisplayInfoCollection displays = screenShit.GetDisplays();
                for (int i = 0; i < displays.Count; i++)
                {
                    await WriteLine("Monitor " + (i + 1) + " har en upplösning på " + displays[i].ScreenWidth + " gånger " + displays[i].ScreenHeight + " pixlar"/*, ctx*/);
                }
                await WriteLine("Dator namn: " + Environment.MachineName/*, ctx*/);
                await WriteLine("Användarnamn: " + Environment.UserName/*, ctx*/);
                await WriteLine("Dator organisation: " + Environment.UserDomainName/*, ctx*/);
                await WriteLine("Fil mapp: " + Environment.CurrentDirectory/*, ctx*/);
                await WriteLine("Kommando rad: " + "\"" + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + "\""/*, ctx*/);

                TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
                if (uptime.TotalDays >= 1)
                {
                    await WriteLine("Tid sen full nedstängning: " + uptime.Days + " dagar " + uptime.Hours + " timmar " + uptime.Minutes + " minuter"/*, ctx*/);
                }
                else if (uptime.TotalHours >= 1)
                {
                    await WriteLine("Tid sen full nedstängning: " + (int)uptime.TotalHours + " timmar " + uptime.Minutes + " minuter"/*, ctx*/);
                }
                else if (uptime.TotalMinutes >= 1)
                {
                    await WriteLine("Tid sen full nedstängning: " + (int)uptime.TotalMinutes + " minuter " + (int)uptime.Seconds + " sekunder "/*, ctx*/);
                }
                else
                {
                    await WriteLine("Tid sen full nedstängning: " + (int)uptime.TotalSeconds + " sekunder "/*, ctx*/);
                }

                await WriteLine("Antal processor kärnor: " + Environment.ProcessorCount/*, ctx*/);
                await WriteLine(Environment.Is64BitOperatingSystem ? "64 bitars operativ system" : "32 eller färre bitars operativ system"/*, ctx*/);
                await WriteLine(Environment.Is64BitProcess ? "64 bitars program" : "32 bitars program"/*, ctx*/);
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
                SendString = string.Empty;
                await WriteLine("Bot namn: " + Client.CurrentApplication.Name/*, ctx*/);
                //await WriteLine("Team name: " + Client.CurrentApplication.Team.Name/*, ctx*/);
                //var a = Client.CurrentApplication.Team.Members.ToArray();
                //for (int i = 0; i < a.Length; i++)
                //{
                //    await WriteLine("Member " + (i + 1) + ": " + Client.CurrentApplication.Team.Members/*, ctx*/);
                //}
                var b = Client.CurrentApplication.Owners.ToArray();
                for (int i = 0; i < b.Length; i++)
                {
                    await WriteLine("Owner " + (i + 1) + ": " + b[i].Username/*, ctx*/);
                }
                TimeSpan uptime = bot.sw.Elapsed;
                if (uptime.TotalDays >= 1)
                {
                    await WriteLine("Upptid: " + uptime.Days + " dagar " + uptime.Hours + " timmar " + uptime.Minutes + " minuter"/*, ctx*/);
                }
                else if (uptime.TotalHours >= 1)
                {
                    await WriteLine("Upptid: " + (int)uptime.TotalHours + " timmar " + uptime.Minutes + " minuter"/*, ctx*/);
                }
                else if (uptime.TotalMinutes >= 1)
                {
                    await WriteLine("Upptid: " + (int)uptime.TotalMinutes + " minuter " + (int)uptime.Seconds + " sekunder "/*, ctx*/);
                }
                else
                {
                    await WriteLine("Upptid: " + (int)uptime.TotalSeconds + " sekunder "/*, ctx*/);
                }
                await WriteLine("Har " + botCoinSaves.Count + " botcoin användare");
                await WriteLine("Github repository: https://github.com/LordGurr/DiscordBot");
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
                await WriteLine("Sparade alla " + botCoinSaves.Count + " botcoin användares botcoin.", ctx);
            }

            [DSharpPlus.CommandsNext.Attributes.Command("upload")]
            [DSharpPlus.CommandsNext.Attributes.Aliases("laddaup")]
            [DSharpPlus.CommandsNext.Attributes.Description("Saves botcoin users.")]
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

            [DSharpPlus.CommandsNext.Attributes.Command("app")]
            [DSharpPlus.CommandsNext.Attributes.Description("Takes a screenshot.")]
            [DSharpPlus.CommandsNext.Attributes.RequireOwner]
            public async Task ScreenshotApp(CommandContext ctx)
            {
                try
                {
                    await bot.TakeScreenshotAndUploadApplication(ctx, Process.GetCurrentProcess().MainWindowHandle);
                }
                catch (Exception e)
                {
                    await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
                }
            }

            [DSharpPlus.CommandsNext.Attributes.Command("getapps")]
            [DSharpPlus.CommandsNext.Attributes.Description("Takes a screenshot.")]
            [DSharpPlus.CommandsNext.Attributes.RequireOwner]
            public async Task GetApp(CommandContext ctx)
            {
                SendString = string.Empty;
                foreach (KeyValuePair<IntPtr, string> window in OpenWindowGetter.GetOpenWindows())
                {
                    IntPtr handle = window.Key;

                    string title = window.Value;

                    await WriteLine(title + ", (" + handle + ")");
                }
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Applications info",
                    Description = SendString,
                });
                SendString = string.Empty;
            }

            [DSharpPlus.CommandsNext.Attributes.Command("appsscreenshots")]
            [DSharpPlus.CommandsNext.Attributes.Aliases("appsskärmbilder", "appsskärmdumpar")]
            [DSharpPlus.CommandsNext.Attributes.Description("Takes a screenshot.")]
            [DSharpPlus.CommandsNext.Attributes.RequireOwner]
            public async Task GetAppScreen(CommandContext ctx)
            {
                SendString = string.Empty;
                foreach (KeyValuePair<IntPtr, string> window in OpenWindowGetter.GetOpenWindows())
                {
                    IntPtr handle = window.Key;

                    string title = window.Value;

                    await CommandWriteLine(title + ", (" + handle + ")", ctx);
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

            [DSharpPlus.CommandsNext.Attributes.Command("commandline")]
            [DSharpPlus.CommandsNext.Attributes.Description("Takes a screenshot.")]
            [DSharpPlus.CommandsNext.Attributes.RequireOwner]
            public async Task ExecCommand(CommandContext ctx, [RemainingText] string strCmdLine)
            {
                // Start the child process.
                Process p = new Process();
                // Redirect the output stream of the child process.
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.Arguments = strCmdLine;
                p.Start();

                // Do not wait for the child process to exit before
                // reading to the end of its redirected stream.
                // p.WaitForExit();
                // Read the output stream first and then wait.
                string output = p.StandardOutput.ReadToEnd();
                await ctx.Channel.SendMessageAsync(output).ConfigureAwait(false);
                p.WaitForExit();
                await bot.TakeScreenshotAndUpload(ctx);
            }

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
                    //TimeSpan temp = DateTime.Now - lastSave;
                    //await WriteLine("Stänger ner inom " + (sparTid.TotalMinutes - temp.TotalMinutes).ToString("F1") + " minuter.\nKommer inte att starta igen.", e);
                    await CommandWriteLine("Stänger ner omdelbart på order av: " + ctx.Member.DisplayName + "(" + ctx.Member.Username + ")", ctx);
                    await SaveAllbotcoin(ctx);
                    await bot.TakeScreenshotAndUpload(ctx);
                    await Client.DisconnectAsync();
                    Client.Dispose();
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
            public async Task Activity(CommandContext ctx, int activityType, params string[] inputs)
            {
                //DiscordClient discord = ctx.Client;
                //string input = Console.ReadLine();
                string input = "";
                //for (int i = 0; i < inputs.Length; i++)
                //{
                //    try
                //    {
                //        //inputs[i] = inputs[i].Trim('<', '>');
                //        //inputs[i] = inputs[i].Trim('<', '>', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
                //        if (inputs[i].Contains('<', '>'))
                //        {
                //            string[] temp = inputs[i].Split('<', '>');
                //            for (int a = 0; a < temp.Length; a++)
                //            {
                //                try
                //                {
                //                    DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, temp[a].Trim('<', '>', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'));
                //                    //activity.Name.Replace(inputs[i], "");
                //                    input += $"{emoji}";
                //                    await ctx.RespondAsync($"{emoji}");
                //                }
                //                catch (Exception e)
                //                {
                //                    await ctx.RespondAsync(e.Message);
                //                    input += inputs[i];
                //                }
                //            }
                //        }
                //        else
                //        {
                //            DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, inputs[i].Trim('<', '>', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'));
                //            //activity.Name.Replace(inputs[i], "");
                //            input += $"{emoji}";
                //            await ctx.RespondAsync($"{emoji}");
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        await ctx.RespondAsync(e.Message);
                //        input += inputs[i];
                //    }
                //}

                // Real Shit
                for (int i = 0; i < inputs.Length; i++)
                {
                    input += inputs[i] + " ";
                }

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
                    await ctx.Channel.SendMessageAsync("Du är redan uppskriven för botcoin och har " + botCoinSaves[i].antalBotCoin + " botcoins.").ConfigureAwait(false);
                    return;
                }
                botCoinSaves.Add(new BotCoinSaveData(ctx.Message.Author.Id, rng.Next(0, 10), DateTime.Now.AddMinutes(-5)));
                await ctx.Channel.SendMessageAsync("Du är nu uppskriven för botcoin och har: " + botCoinSaves[botCoinSaves.Count - 1].antalBotCoin + " botcoins.").ConfigureAwait(false);
            }

            [DSharpPlus.CommandsNext.Attributes.Command("remindme")]
            [DSharpPlus.CommandsNext.Attributes.Description("Reminds you at specified time")]
            public async Task Remind(CommandContext ctx, [DSharpPlus.CommandsNext.Attributes.Description("amount of time to wait in integers")]int time, [DSharpPlus.CommandsNext.Attributes.Description("Your measurement of time.")]string measurement, [DSharpPlus.CommandsNext.Attributes.Description("Sends message at specified time. Optional")]params string[] message)
            {
                DateTime current = DateTime.Now;
                DateTime remindTime = DateTime.Now;
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
                await Task.Delay(Convert.ToInt32(timeSpan.TotalMilliseconds));
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

                GiveBotCoin(ctx);
            }

            [DSharpPlus.CommandsNext.Attributes.Command("remindme")]
            [DSharpPlus.CommandsNext.Attributes.Description("Reminds you at specified time")]
            public async Task Remind(CommandContext ctx, [DSharpPlus.CommandsNext.Attributes.Description("amount of time to wait in integers")]int time, [DSharpPlus.CommandsNext.Attributes.Description("Your measurement of time.")]string measurement)
            {
                DateTime current = DateTime.Now;
                DateTime remindTime = DateTime.Now;
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
                await Task.Delay(Convert.ToInt32(timeSpan.TotalMilliseconds));
                await ctx.Channel.SendMessageAsync(ctx.User.Mention + " You told me to remind you now.").ConfigureAwait(false);
                GiveBotCoin(ctx);
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
            [DSharpPlus.CommandsNext.Attributes.Description("Reminds you at specified time")]
            public async Task Image2string(CommandContext ctx)
            {
                //Doesn't work yet    //List<DiscordAttachment> attachments = (List<DSharpPlus.Entities.DiscordAttachment>)ctx.Message.Attachments;
                //attachments.FindAll(x => x.GetType() == typeof(".png"))
                SendString = "";
                Image image = Image.FromFile(@"C:\Users\gustav.juul\Pictures\GaleBackup\ToadSpriteRightJump.png");
                try
                {
                    if (ctx.Message.Attachments.Count > 0)
                    {
                        string url = ctx.Message.Attachments.FirstOrDefault().Url;
                        SaveImage("screenshotTemp.png", System.Drawing.Imaging.ImageFormat.Png, url);
                        image = Image.FromFile("screenshotTemp.png");
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
                    string empty = "‌‌ ";
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

                        await WriteLine(temp);
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

            //[DSharpPlus.CommandsNext.Attributes.Command("inspiroquote")]
            //public async Task InspiroCitat(CommandContext ctx)
            //{
            //    using (System.Net.WebClient webClient = new System.Net.WebClient())
            //    {
            //        using (Stream stream = webClient.OpenRead("https://generated.inspirobot.me/a/yZedqEgwJn.jpg"))
            //        {
            //            Image newImage = Image.FromFile("SampImag.jpg");
            //        }
            //    }
            //}

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
                DiscordMember[] members = ctx.Channel.Users.ToArray();
                int membersModified = 0;
                int membersNotAbleToModify = 0;
                for (int i = 0; i < members.Length; i++)
                {
                    try
                    {
                        await members[i].ModifyAsync(u => u.Nickname = name);
                        membersModified++;
                    }
                    catch (Exception e)
                    {
                        WriteLine("Försökte ändra smeknamn: " + e.Message, ctx);
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

            [DSharpPlus.CommandsNext.Attributes.Command("flipCoin")]
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

            [DSharpPlus.CommandsNext.Attributes.Command("Adventure")]
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

            [DSharpPlus.CommandsNext.Attributes.Command("Adventure")]
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

        private class AdventureSaveData
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

        private class BotCoinSaveData
        {
            public ulong user;
            public int antalBotCoin;
            public DateTime senastTjänadePeng;

            public BotCoinSaveData(ulong _user, int _antalBotCoin, DateTime _senastTjänadePeng)
            {
                user = _user;
                antalBotCoin = _antalBotCoin;
                senastTjänadePeng = _senastTjänadePeng;
            }
        }

        private class ChannelSaveData
        {
            public ulong discordChannel;
            public List<DiscordUserSaveData> discordUsers = new List<DiscordUserSaveData>();

            public ChannelSaveData(ulong _discordChannel)
            {
                discordChannel = _discordChannel;
            }
        }

        private class DiscordUserSaveData
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

        private class ScreenCapture
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

        public static class ScreenShotHelper
        {
            private static Bitmap CopyFromScreen(Rectangle bounds)
            {
                try
                {
                    var image = new Bitmap(bounds.Width, bounds.Height);
                    using var graphics = Graphics.FromImage(image);
                    graphics.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    return image;
                }
                catch (Win32Exception)
                {//When screen saver is active
                    return null;
                }
            }

            public static Image Take(Rectangle bounds)
            {
                return CopyFromScreen(bounds);
            }

            public static byte[] TakeAsByteArray(Rectangle bounds)
            {
                using var image = CopyFromScreen(bounds);
                using var ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }

            public static void TakeAndSave(string path, Rectangle bounds, System.Drawing.Imaging.ImageFormat imageFormat)
            {
                using var image = CopyFromScreen(bounds);
                image.Save(path, imageFormat);
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
            private static extern bool IsWindowVisible(HWND hWnd);

            [DllImport("USER32.DLL")]
            private static extern IntPtr GetShellWindow();
        }
    }
}