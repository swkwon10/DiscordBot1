using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using StackExchange.Redis;

namespace dtest
{
    

    public class MessageTracker
    {
        static public MessageTracker inst = new MessageTracker();

		Dictionary<ulong, HashSet<ulong>> subs;

        public MessageTracker()
        {
            LoadSetting();
        }

        void LoadSetting()
        {
			subs = new Dictionary<ulong, HashSet<ulong>>();
			
			try
			{
				using (System.IO.StreamReader file = new System.IO.StreamReader("Tracker.txt", Encoding.UTF8))
				{
					string line;
					while ((line = file.ReadLine()) != null)
					{
						string[] sp = line.Split("-");

						AddChannel(ulong.Parse(sp[0]), ulong.Parse(sp[1]));
					}
				}					
			}
			catch (Exception ex) { };
		}

		void SaveSetting()
		{
			using (var stream = new FileStream("Tracker.txt", FileMode.Create))
			using (var writer = new StreamWriter(stream, Encoding.Default))
			{
				foreach(var g in subs)
				{
					foreach(var c in g.Value)
					{
						writer.WriteLine(g.Key.ToString()+"-"+c.ToString());
					}
				}				
			}
		}

        bool IsTracking(ulong guildId , ulong chanId)
        {
			if(subs.ContainsKey(guildId))
			{
				return subs[guildId].Contains(chanId);
			}
            return false;
        }

        public void Proc(SocketMessage message)
        {
            var chnl = message.Channel as SocketGuildChannel;
            ulong guild = chnl.Guild.Id;
            ulong channel = message.Channel.Id;

            if(IsTracking(guild, channel))
            {
                return;
            }


        }

		public void Commands(SocketMessage message, string[] cmd)
		{
			var chnl = message.Channel as SocketGuildChannel;
			ulong guild = chnl.Guild.Id;
			ulong chan = message.Channel.Id;
			switch (cmd[1])
			{
				case "Add":
					message.Channel.SendMessageAsync( CommandAdd(guild, chan) ? "등록 완료" : "이미 등록되어있습니다" );
					break;
				case "Del":
					message.Channel.SendMessageAsync( CommandDel(guild, chan) ? "삭제 완료" : "등록되어있지 않습니다" );					
					break;
				case "Print":

					break;
			}
		}

		bool CommandAdd(ulong guildId, ulong chanId)
		{
			if (IsTracking(guildId, chanId))
			{
				return false;
			}
			AddChannel(guildId, chanId);
			SaveSetting();
			return true;
		}

		bool CommandDel(ulong guildId, ulong chanId)
		{
			if (!IsTracking(guildId, chanId))
			{
				return false;
			}
			DelChannel(guildId, chanId);
			SaveSetting();
			return true;
		}

        public void AddChannel(ulong guildId, ulong chanId)
        {
			HashSet<ulong> h;
			if (subs.TryGetValue(guildId, out h))
			{
				h.Add(chanId);
			}
			else
			{
				h = new HashSet<ulong>();
				h.Add(chanId);
				subs.Add(guildId, h);
			}
		}

		public void DelChannel(ulong guildId, ulong chanId)
		{
			HashSet<ulong> h;
			if (subs.TryGetValue(guildId, out h))
			{
				h.Remove(chanId);
			}
		}
	}
}
