using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using Discord.WebSocket;

namespace dtest
{
	class Server
	{
		public Server() { lastStatus = false; }

		public string name { get; set; }
		public string ip { get; set; }
		public ushort port { get; set; }

		[JsonIgnore]
		public bool lastStatus;
	}

	class Observer
	{
		public Observer() { }

		public ulong guildNo { get; set; }
		public ulong channalNo { get; set; }
	}

	class ServerChecker
	{
		static ServerChecker inst = new ServerChecker();
		public static ServerChecker GetInstance()
		{
			return inst;
		}


		private static readonly string configDirectory = ".\\Config";
		private static readonly string serverFile = "Server.txt";
		private static readonly string observerFile = "Observer.txt";

		private static List<Server> serverList;
		private static List<Observer> observerList;

		Thread thread = null;
		public static ManualResetEvent ExitFlag = new ManualResetEvent(false);

		public ServerChecker()
		{
			
		}		
		public void Init()
		{
			ReadSetting();
		}

		private void ReadSetting()
		{
			try
			{
				serverList = JsonConvert.DeserializeObject<List<Server>>(File.ReadAllText($"{configDirectory}\\{serverFile}", Encoding.UTF8));
			}
			catch (Exception ex)
			{
				serverList = new List<Server>();
			}

			try
			{
				observerList = JsonConvert.DeserializeObject<List<Observer>>(File.ReadAllText($"{configDirectory}\\{observerFile}", Encoding.UTF8));
			}
			catch (Exception ex)
			{
				observerList = new List<Observer>();
			}
		}

		private void SaveObserver()
		{
			if (!Directory.Exists(configDirectory))
			{
				Directory.CreateDirectory(configDirectory);
			}
			
			using (StreamWriter file = new StreamWriter($"{configDirectory}\\{observerFile}", false, Encoding.UTF8))
			{
				lock (observerList)
				{
					file.Write(JsonConvert.SerializeObject(observerList, Formatting.Indented));
				}
			}
		}

		public void AddObserver(ulong guild , ulong channel)
		{
			lock (observerList)
			{
				observerList.Add(new Observer() { guildNo = guild, channalNo = channel });
			}
			SaveObserver();
		}
		public void DelObserver(ulong guild, ulong channel)
		{
			DelObserver(FindObserver(guild, channel));
		}
		public void DelObserver(Observer ob)
		{
			if(ob == null)
			{
				return;
			}
			lock(observerList)
			{
				observerList.Remove(ob);
			}
			SaveObserver();
		}

		private Observer FindObserver(ulong guild, ulong channel)
		{
			lock(observerList)
			{
				foreach (var ob in observerList)
				{
					if (ob.guildNo == guild && ob.channalNo == channel)
					{
						return ob;
					}
				}
			}
			return null;
		}


		public void OnCommand(SocketMessage message, string args)
		{
			string[] sp = args.Split(" ");

			switch(sp[0].ToLower())
			{
				case "add":
					AddChannel(message);
					break;
				case "delete":
					DelChannel(message);
					break;
			}
		}

		private void AddChannel(SocketMessage message)
		{
			var chnl = message.Channel as SocketGuildChannel;

			if (FindObserver(chnl.Guild.Id , chnl.Id) != null)
			{
				DiscordClient.GetInstance()?.Send(chnl.Guild.Id, chnl.Id, "이미 등록되어있습니다");
				return;
			}
			AddObserver(chnl.Guild.Id, chnl.Id);
			DiscordClient.GetInstance()?.Send(chnl.Guild.Id , chnl.Id, "채널등록완료");
		}

		private void DelChannel(SocketMessage message)
		{
			var chnl = message.Channel as SocketGuildChannel;
			var ob = FindObserver(chnl.Guild.Id, chnl.Id);

			if (ob == null)
			{
				DiscordClient.GetInstance()?.Send(chnl.Guild.Id, chnl.Id, "등록되어있지 않습니다");
				return;
			}

			DelObserver(ob);
			DiscordClient.GetInstance()?.Send(chnl.Guild.Id, chnl.Id, "채널삭제완료");
		}

		public void Run()
		{
			if(thread != null)
			{
				return;
			}

			thread = new Thread(ThreadProc);
			thread.Start();
		}

		public void Abort()
		{
			ExitFlag.Set();
			thread?.Join();			
		}

		private void ThreadProc()
		{
			while( !ExitFlag.WaitOne(60000) )
			{
				CheckServer();
			}
			ExitFlag.Reset();
		}

		private void CheckServer()
		{
			foreach(var server in serverList)
			{
				var resp = SourceEngineQuery.Query(server.ip, server.port);
				
				if( (resp != null) != server.lastStatus )
				{
					server.lastStatus = (resp != null);

					Alert(server.name, server.lastStatus);
				}
			}
		}

		private void Alert(string name , bool status)
		{
			string message = $"서버상태변동 {name}- " + (status? "실행중" : "응답없음");
			foreach (var observer in observerList)
			{
				DiscordClient.GetInstance()?.Send(observer.guildNo, observer.channalNo , message);
			}			
		}
	}
}
