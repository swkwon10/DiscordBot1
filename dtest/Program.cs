using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;


namespace dtest
{	
    class Program
    {
		public static ManualResetEvent ExitFlag = new ManualResetEvent(false);
		
        static void Main ( string [] args )
        => new Program().MainAsync(args);

		public void MainAsync(string[] args)
		{
			Logger.GetInstance().Init();
			var discord = DiscordClient.GetInstance();
			var checker = ServerChecker.GetInstance();
			var tracker = MessageTracker.GetInstance();
			discord.Init();
			checker.Init();
			tracker.Init();

			discord.SetLogInfo(Logger.Info);
			discord.SetLogError(Logger.Error);
			
			discord.AddCommandHandler("track", tracker.OnCommand, 3);
			discord.AddCommandHandler("rust", checker.OnCommand, 3);
			discord.AddOnMessageReceived(tracker.Proc);

			discord.Run(args[0]).Wait();

			ServerChecker.GetInstance().Run();
									
			ExitFlag.WaitOne();
			//Exiting
			
			ServerChecker.GetInstance().Abort();
        }
    }

}
