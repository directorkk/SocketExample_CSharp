using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LouieTool.NetworkHandle
{
	enum SOCKET_PLUGIN_ENUM
	{
		ADDRESS_IP = 0,
		ADDRESS_PORT,
		ADDRESS_IP_AND_PORT,
		TYPE_NONE,
		TYPE_CLIENT,
		TYPE_SERVER,
	}

	class DataPacket
	{
		public LinkInfo Owner;
		public List<byte> Data;
		public long Len;
		public long RecvCounter;

		public DataPacket()
		{
			Owner = new LinkInfo();
			Data = new List<byte>();
			Len = RecvCounter = 0;
		}
	}
	
	class LinkInfo
	{
		public TcpClient Client;
		public IPAddress IP;
		public int Port;
		public string GetFullIPAddress()
		{
			if(IP == null)
			{
				return "";
			}
			return string.Format("{0}:{1}", IP, Port);
		}
	}
}
