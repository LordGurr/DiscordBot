namespace DiscordBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
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