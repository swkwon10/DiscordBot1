using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

namespace dtest
{
	class Admin
	{
		public Admin() { }

		public ulong id { get; set; }
		public int level { get; set; }
	}

	class DiscordClient : DiscordSocketClient
	{
		private static readonly string configDirectory = ".\\Config";
		private static readonly string adminFile = "Admin.txt";

		private Dictionary<string, (Action<SocketMessage, string>,int)> commandDict = new Dictionary<string, (Action<SocketMessage, string>, int)>();
		public void AddCommandHandler(string command, Action<SocketMessage, string> callback , int callLevel) => commandDict.Add(command.ToLower(), (callback,callLevel));

		private Dictionary<ulong, Admin> adminDict;
		public void AddAdmin(Admin admin)
		{
			adminDict.Add(admin.id, admin);
			SaveAdmin();
		}
		public Admin FindAdmin(ulong id) => adminDict.ContainsKey(id) ? adminDict[id] : null;

		private Action<string> log_Error = null;
		public void SetLogError(Action<string> logCallback) => log_Error = logCallback;
		private void LogError(string msg) => log_Error?.Invoke(msg);

		private Action<string> log_Info = null;
		public void SetLogInfo(Action<string> logCallback) => log_Info = logCallback;
		private void LogInfo(string msg) => log_Info?.Invoke(msg);

		public delegate void onRecvHandler(SocketMessage message);
		private onRecvHandler messageRecvHander;
		public void AddOnMessageReceived(Action<SocketMessage> evnt) => messageRecvHander += new onRecvHandler(evnt);


		static DiscordClient inst = new DiscordClient();
		public static DiscordClient GetInstance()
		{
			return inst;
		}

		public DiscordClient()
		{
			base.Log += OnLogMessage;
			base.MessageReceived += OnMessageReceived;
			
		}

		public void Init()
		{
			LoadAdmin();
		}

		public async Task Run(string token)
		{
			await LoginAsync(TokenType.Bot, token);
			await StartAsync();
		}

		public void LoadAdmin()
		{
			try
			{				
				var list = JsonConvert.DeserializeObject<List<Admin>>(File.ReadAllText($"{configDirectory}\\{adminFile}" , Encoding.UTF8));
				adminDict = list.ToDictionary(x => x.id, x => x);
			}
			catch(Exception ex)
			{
				adminDict = new Dictionary<ulong, Admin>();
			}			
		}

		public void SaveAdmin()
		{
			if(!Directory.Exists(configDirectory))
			{
				Directory.CreateDirectory(configDirectory);
			}
			using (StreamWriter file = new StreamWriter($"{configDirectory}\\{adminFile}" , false , Encoding.UTF8))
			{
				file.Write(JsonConvert.SerializeObject(adminDict.Values.ToList<Admin>(), Formatting.Indented));
			}
		}

		private async Task OnMessageReceived(SocketMessage rowmessage)
		{
			if (rowmessage.Author.IsBot)
			{
				return;
			}

			if(rowmessage.Content.Length != 0)
			{
				if (rowmessage.Content.Substring(0, 1).Equals("?"))
				{
					CommandResolver(rowmessage);
				}
				messageRecvHander(rowmessage);
			}
		}

		private void CommandResolver(SocketMessage message)
		{
			if(message.Content.Length == 0)
			{
				return;
			}

			Admin admin = FindAdmin(message.Author.Id);
			int level = admin?.level ?? 0;

			string Message = message.Content.Substring(1);
			string[] sp = Message.Split(" ".ToCharArray(), 2);
			string command = sp[0];

			command = command.ToLower();

			if (!commandDict.ContainsKey(command))
			{
				return;
			}

			var cmd = commandDict[command];
			if (cmd.Item2 > level)
			{
				return;
			}
			cmd.Item1(message, sp[1]);       
		}
		
		private Task OnLogMessage(LogMessage msg)
		{
			LogInfo(msg.ToString());
			return Task.CompletedTask;
		}

		public bool Send(ulong guildId , ulong channelId , string message)
		{
			SocketGuild guild = GetGuild(guildId);
			if (guild == null)
			{
				return false;
			}
			SocketTextChannel channel = guild.GetTextChannel(channelId);
			if (channel == null)
			{
				return false;
			}		
			return channel.SendMessageAsync(message).Result != null;
		}

	}
}
