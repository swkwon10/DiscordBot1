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

    //temp
    public static class ExpandMethod
    {
        public static int GetLine(this string a)
        {
            return 0;
        }
    }

    class Program
    {
        private DiscordSocketClient _client;
        static void Main ( string [] args )
        => new Program().MainAsync().GetAwaiter().GetResult();

        List<string> lChannel;

        public async Task MainAsync ()
        {
            Thread Checker = new Thread(ProcServerQuery);
            Checker.Start();
            lChannel = new List<string>();

            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader("Channel.txt" , Encoding.Default);
                string line;
                while ( ( line = file.ReadLine() ) != null )
                {
                    lChannel.Add(line);
                }
            } catch ( Exception ex ) { };
                      

            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.MessageReceived += MessageReceived;

            await _client.LoginAsync(TokenType.Bot , @"");
            await _client.StartAsync();
            
            await Task.Delay(-1);
            
        }

        private Task Log ( LogMessage msg )
        {
            Console.WriteLine(DateTime.Now.ToString("MM-dd ") + msg.ToString());
            return Task.CompletedTask;
        }

		ulong[] admin = { 329176543194775552 };

        private void CommandResolver(SocketMessage message)
        {
			Console.WriteLine(message.Author.Id);

			if(!Array.Exists(admin, x => x == message.Author.Id))
			{
				Console.WriteLine("NULL");
				return;
			}

            string Message = message.Content.Substring(1);
            string[] sp = Message.Split(" ".ToCharArray(),2);

            string command = sp[0];

            switch(command)
            {
                case "Rust":

                    break;
				case "Track":
					MessageTracker.inst.Commands(message, sp);					
                    break;
            }
			
            /*
            string identifyer = guild.ToString() + "-" + channel.ToString();

            if (Message.Equals("RustAdd"))
            {

                if (lChannel.Contains(identifyer))
                {
                    await rowmessage.Channel.SendMessageAsync("이미 등록되어있습니다 " + identifyer);
                    return;
                }
                lChannel.Add(identifyer);
                using (var stream = new FileStream("Channel.txt", FileMode.Append))
                using (var writer = new StreamWriter(stream, Encoding.Default))
                {
                    writer.WriteLine(identifyer);
                }
                RestUserMessage sended = await rowmessage.Channel.SendMessageAsync("채널등록완료 ID : " + identifyer);
            }
            else if (Message.Equals("RustDelete"))
            {
                if (!lChannel.Contains(identifyer))
                {
                    return;
                }
                lChannel.Remove(identifyer);
                using (var stream = new FileStream("Channel.txt", FileMode.Create))
                using (var writer = new StreamWriter(stream, Encoding.Default))
                {
                    foreach (var chan in lChannel)
                    {
                        writer.WriteLine(chan);
                    }
                }
                RestUserMessage sended = await rowmessage.Channel.SendMessageAsync("채널삭제완료 ID : " + identifyer);
            }
            else
            {
                Task.Run(async () => {

                    Console.WriteLine(Message);
                    RestUserMessage sended = await rowmessage.Channel.SendMessageAsync(Message);
                    await Task.Delay(10000);
                    await sended.ModifyAsync(msg => msg.Content = Message + " _ edit after 10 sec :star: ");
                });
            }*/
        }

        private async Task MessageReceived ( SocketMessage rowmessage )
        {
            if ( rowmessage.Author.IsBot )
            {
				return;
            }
			/*if( rowmessage.Attachments.Count != 0 && (rowmessage.Content.Length != 0 && rowmessage.Content.Equals("태그")))
				{
					Console.WriteLine(rowmessage.Attachments.Count+"Count Attached");
					foreach(var file in rowmessage.Attachments )
					{
						Console.WriteLine("Attach Name = " + file.Filename +" Size = "+file.Size + " URL=" + file.Url);
					}
					return;
				}*/
            if(rowmessage.Content.Length == 0)
            {
                return;
            }

			if ( rowmessage.Content.Substring(0 , 1).Equals("?") )
			{
                CommandResolver(rowmessage);
			}

            MessageTracker.inst.Proc(rowmessage);
        }

        List<string> resend;
        public void ProcServerQuery()
        {
            bool lastServer1 = false;
            bool lastServer2 = false;
            resend = new List<string>();
            List<string> success = new List<string>();
            while (true)
            {
                Thread.Sleep(300000);
                //Thread.Sleep(30000);
                bool server1 = CheckServer(28215);
                bool server2 = CheckServer(28115);

                

                foreach ( var chan in resend )
                {
                    Console.WriteLine(DateTime.Now.ToString("HH:mm;ss]") + "Resending - " + chan);
                    SocketGuild guild = _client.GetGuild(UInt64.Parse(chan.Split('-') [0]));
                    if ( guild == null )
                    {
                        Console.WriteLine(DateTime.Now.ToString("HH:mm;ss]") + "g null");
                        continue;
                    }
                    SocketTextChannel channel = guild.GetTextChannel(UInt64.Parse(chan.Split('-') [1]));
                    if ( channel == null )
                    {
                        Console.WriteLine(DateTime.Now.ToString("HH:mm;ss]") + "c null");
                        continue;
                    }

                    var ret = channel.SendMessageAsync("서버상태변동 커뮤-" + GetMessage(server1) + " 모드-" + GetMessage(server2));
                    success.Add(chan);
                }

                foreach ( var chan in success )
                {
                    resend.Remove(chan);
                }
                success.Clear();

                if ( server1 == lastServer1 && server2 == lastServer2 )
                {
                    continue;
                }
                lastServer1 = server1;
                lastServer2 = server2;

                Console.WriteLine(DateTime.Now.ToString("HH:mm;ss]") + "서버상태변동 커뮤-" + GetMessage( server1) +" 모드-"+ GetMessage(server2));

                

                foreach (var chan in lChannel)
                {
                    
                    SocketGuild guild = _client.GetGuild( UInt64.Parse(chan.Split('-') [0]) );
                    if ( guild == null )
                    {
                        Console.WriteLine(DateTime.Now.ToString("HH:mm;ss]") + "g null");
                        resend.Add(chan);
                        continue;
                    }
                    SocketTextChannel channel = guild.GetTextChannel(UInt64.Parse(chan.Split('-') [1]) );
                    if(channel == null)
                    {
                        Console.WriteLine(DateTime.Now.ToString("HH:mm;ss]") + "c null");
                        resend.Add(chan);
                        continue;
                    }

                    var ret = channel.SendMessageAsync("서버상태변동 커뮤-" + GetMessage( server1) +" 모드-"+ GetMessage(server2));
                    ret.Wait();
                    if(ret.Result == null)
                    {
                        Console.WriteLine(DateTime.Now.ToString("HH:mm;ss]") + "var null");
                        resend.Add(chan);                        
                    }
                }
            }
        }

        string GetMessage(bool ret)
        {
            if(ret )
            {
                return "실행중";
            }
            else
            {
                return "응답없음";
            }
        }


        bool CheckServer(int port)
        {
            UdpClient cli = new UdpClient();
            string msg = "TSource Engine Query";

            byte [] Packet = new byte [1500];
            int packetsize = 0;
            byte [] header = BitConverter.GetBytes(0xffffffff);
            Array.Copy(header , 0 , Packet , packetsize , 4);
            packetsize += 4;

            byte [] payload = Encoding.Default.GetBytes(msg);
            Array.Copy(payload , 0 , Packet , packetsize , payload.Length);
            packetsize += payload.Length;
            Packet [packetsize] = 0;
            packetsize++;

            byte [] bytes = null;
            // (2) 데이타 송신
            try
            {
                cli.Send(Packet , packetsize , "127.0.0.1" , port);
				cli.Client.ReceiveTimeout = 2000;
				// (3) 데이타 수신
				IPEndPoint epRemote = new IPEndPoint(IPAddress.Any , 0);
                bytes = cli.Receive(ref epRemote);
				
            } catch ( Exception ex )
            {
                return false;
            }


            // (4) UdpClient 객체 닫기
            cli.Close();

            int nameLen = 0;
            while ( true )
            {
                if ( bytes [6 + nameLen] == 0 )
                {
                    break;
                }
                nameLen++;
            }
            byte [] name = new byte [nameLen];
            Array.Copy(bytes , 6 , name , 0 , nameLen);
            string stName = Encoding.Default.GetString(name);
            return true;
        }


    }

}
