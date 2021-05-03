using System;

namespace DiscordBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Starting up discordbot");
#if DEBUG
            Console.WriteLine("In debug mode");
#else
            Console.WriteLine("In release mode");
#endif
            Bot bot = new Bot();
            while (bot.restart)
            {
                bot.RunAsync().GetAwaiter().GetResult();
                if (!bot.restart)
                {
                    return;
                }
                bot = new Bot();
                System.Threading.Thread.Sleep(2000);
            }
        }
    }
}