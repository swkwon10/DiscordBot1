using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;


namespace dtest
{
	public class SourceEngineResponse
	{
		public string name;
		public string map;
		public string folder;
		public string game;
		public short id;
		public byte players;
		public byte maxPlayers;
		public byte bots;
	}

	public class SourceEngineQuery
	{
		public static SourceEngineResponse Query(string ip, ushort port)
		{
			SourceEngineResponse res = new SourceEngineResponse();

			UdpClient cli = new UdpClient();
			cli.Client.ReceiveTimeout = 2000;
			
			byte[] Packet = new byte[100];

			int packetsize = 0;
			Array.Copy( BitConverter.GetBytes(0xffffffff) , 0, Packet, packetsize, 4);
			packetsize += 4;

			string msg = "TSource Engine Query";
			byte[] payload = Encoding.Default.GetBytes(msg);
			Array.Copy(payload, 0, Packet, packetsize, payload.Length);
			packetsize += payload.Length;
			Packet[packetsize] = 0;
			packetsize++;


			ByteReader reader = new ByteReader();
			byte[] recvData = null;
			try
			{
				cli.Send(Packet, packetsize, ip, port);
				IPEndPoint epRemote = new IPEndPoint(IPAddress.Any, 0);
				reader.data = cli.Receive(ref epRemote);
			}
			catch (Exception)
			{
				return null;
			}
			
			cli.Close();
			reader.MoveRear(4);

			if (reader.GetByte() != 'I')
			{
				return null;
			}

			byte protocolVer = reader.GetByte();			
			res.name = reader.GetString();
			res.map = reader.GetString();
			res.folder = reader.GetString();
			res.game = reader.GetString();
			res.id = reader.GetShort();
			res.players = reader.GetByte();
			res.maxPlayers = reader.GetByte();
			res.bots = reader.GetByte();

			return res;
		}

		
		class ByteReader
		{
			public ByteReader()
			{
				index = 0;
			}

			public string GetString()
			{
				int len = 0;
				while (data[index + len++] != 0) ;

				string ret = Encoding.Default.GetString(data, index, len - 1);
				index += len;
				return ret;
			}
			public byte GetByte()
			{
				index++;
				return data[index - 1];
			}
			public short GetShort()
			{
				index += 2;
				return BitConverter.ToInt16(data, index - 2);
			}

			public void MoveFront(int cnt)
			{
				index -= cnt;
			}

			public void MoveRear(int cnt)
			{
				index += cnt;
			}

			public byte[] data;
			int index;
		}
	}


}
