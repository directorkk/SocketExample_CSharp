using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LouieTool.NetworkHandle
{
	abstract class UDPSocket : SocketBase
    {
		private Socket mServer = null;
		private UdpClient mClient = null;
		private IPEndPoint mIPEnd = null;
		private SOCKET_PLUGIN_ENUM mSocketType = SOCKET_PLUGIN_ENUM.TYPE_NONE;
		private bool mFShutdown = false;
		private bool mFIsAlive = false;

		private Dictionary<string, LinkInfo> mMapSocketInfo;

		private Thread mThreadServer = null;
		private Thread mThreadClient = null;

        private int mSizeLimitEachSend = 32768;

		// ********************** PUBLIC
		public UDPSocket()
		{
			Clear();
		}

		public bool InitServer(ushort LocalPort)
		{
			bool rtn = false;

			try
			{
				mIPEnd = new IPEndPoint(IPAddress.Any, LocalPort);
				mSocketType = SOCKET_PLUGIN_ENUM.TYPE_SERVER;

				rtn = true;
			}
			catch (Exception ex)
			{
				Util.OutputDebugMessage(ex.StackTrace.ToString());
			}

			return rtn;
		}

		public bool StartServer()
		{
			bool rtn = false;

			try
			{
				mServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				mServer.Bind(mIPEnd);
				mThreadServer = new Thread(ServerLiveLoop);
				mThreadServer.Start();

				rtn = true;
			}
			catch (Exception ex)
			{
				Util.OutputDebugMessage(ex.StackTrace.ToString());
			}

			return rtn;
		}

		public bool InitClient(string TargetIP, ushort TargetPort)
		{
			bool rtn = false;

			try
			{
				mClient = new UdpClient();
				mClient.Connect(TargetIP, TargetPort);
				mSocketType = SOCKET_PLUGIN_ENUM.TYPE_CLIENT;

				rtn = true;
			}
			catch (Exception ex)
			{
				Util.OutputDebugMessage(ex.StackTrace.ToString());
			}

			return rtn;
		}

		public bool StartClient()
		{

            bool rtn = false;

            try
            {
                mThreadClient = new Thread(ClientLiveLoop);
                mThreadClient.Start();

                rtn = true;
            }
            catch (Exception ex)
            {
                Util.OutputDebugMessage(ex.StackTrace.ToString());
            }

            return rtn;
        }

		public void SendRaw(byte[] RawData, List<LinkInfo> Targets = null)
		{
			switch (mSocketType)
			{
				case SOCKET_PLUGIN_ENUM.TYPE_NONE:
					break;
				case SOCKET_PLUGIN_ENUM.TYPE_SERVER:
					SendToClient(RawData, Targets);
					break;
				case SOCKET_PLUGIN_ENUM.TYPE_CLIENT:
					SendToServer(RawData);
					break;
				default:
					break;
			}
		}

		public void Send(string Message, List<LinkInfo> Targets = null)
		{
			byte[] packedMsg = null;
			PackMessage(Message, out packedMsg);

			switch (mSocketType)
			{
				case SOCKET_PLUGIN_ENUM.TYPE_NONE:
					break;
				case SOCKET_PLUGIN_ENUM.TYPE_SERVER:
					SendToClient(packedMsg, Targets);
					break;
				case SOCKET_PLUGIN_ENUM.TYPE_CLIENT:
					SendToServer(packedMsg);
					break;
				default:
					break;
			}
		}

		private Mutex mMutexShutdown = new Mutex();
		private bool GetShutdownState()
		{
			bool Rtn = false;

			mMutexShutdown.WaitOne();
			Rtn = mFShutdown;
			mMutexShutdown.ReleaseMutex();

			return Rtn;
		}

		private void SetShutdownState(bool State)
		{
			mMutexShutdown.WaitOne();
			mFShutdown = State;
			mMutexShutdown.ReleaseMutex();
		}


		public void Shutdown()
		{
			SetShutdownState(true);
			if (mThreadServer != null)
			{
				mThreadServer.Join();
				mServer.Shutdown(SocketShutdown.Both);
				mServer.Close();
			}
			if (mThreadClient != null)
			{
				mThreadClient.Join();
				mClient.Close();
			}
		}

		public void IsAlive()
		{

		}

		public List<LinkInfo> GetLinkedClients()
		{
			List<LinkInfo> rtn = null;

			/*for (int i = mLinkedCLients.Count - 1; i >= 0; i--)
			{
				if (!mLinkedCLients[i].Client.Connected)
				{
					mLinkedCLients.RemoveAt(i);
				}
			}*/
			rtn = new List<LinkInfo>(mLinkedClients.ToArray());

			return rtn;
		}


		// ********************** PRIVATE
		private void ServerLiveLoop()
		{
			while (!GetShutdownState())
			{
				Thread.Sleep(10);

				List<LinkInfo> linkedClients = GetLinkedClients();
				//Console.WriteLine(linkedClients.Count);
				//foreach(LinkInfo linkInfo in linkedClients)

				IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
				EndPoint remote = (EndPoint)sender;
				int bufferSize = mSizeLimitEachSend;
				byte[] bufferRecv = new byte[bufferSize];
				int readLen = 0;

				try
				{
					mServer.ReceiveTimeout = 50;
					readLen = mServer.ReceiveFrom(bufferRecv, bufferSize, SocketFlags.None, ref remote);
				}
				catch(SocketException ex)
                {
					// didnt receive anything
                }

				if(readLen >= 0)
				{
					LinkInfo connection = new LinkInfo();
					connection.IP = ((IPEndPoint)remote).Address;
					connection.Port = ((IPEndPoint)remote).Port;

					LinkInfo linkInfo = linkedClients.Find(info => info.GetFullIPAddress().Equals(connection.GetFullIPAddress()));
					if (linkInfo == null)
					{
						ServerAcceptNewLink(connection);
						linkInfo = mMapSocketInfo[connection.GetFullIPAddress()];
					}

					byte[] bufferInByte = bufferRecv.Take(readLen).ToArray();
					char[] bufferInChar = new char[bufferInByte.Length];
					ServerRecvPacket(linkInfo, bufferInByte);
				}
			}
		}

		private void ClientLiveLoop()
		{
            while (!GetShutdownState())
            {
                Thread.Sleep(10);
                
                IPEndPoint remoteIPEndPoint = mClient.Client.RemoteEndPoint as IPEndPoint;

				mClient.Client.ReceiveTimeout = 50;
                try
				{
					byte[] bufferInByte = mClient.Receive(ref remoteIPEndPoint);
					ClientRecvPacket(bufferInByte);
				}
				catch(SocketException ex)
                {

                }
            }
        }

		private void Clear()
		{
			mFShutdown = false;
			if (mServer != null)
			{
				mServer = null;
			}
			if (mClient != null)
			{
				mClient = null;
			}
			mMapSocketInfo = new Dictionary<string, LinkInfo>();
			mMapProcessedPackets = new Dictionary<LinkInfo, List<DataPacket>>();
			mMapAryRecvData = new Dictionary<LinkInfo, List<byte>>();
			mLinkedClients = new List<LinkInfo>();
		}

        protected virtual void ServerRecvPacket(LinkInfo SourceInfo, byte[] Data)
		{
			if (!mMapAryRecvData.ContainsKey(SourceInfo))
			{
				mMapAryRecvData.Add(SourceInfo, new List<byte>());
			}
			mMapAryRecvData[SourceInfo].AddRange(Data);

            byte[] patternInBytes = Encoding.UTF8.GetBytes(mPacketPattern.ToCharArray());
            UnpackMessage(SourceInfo, patternInBytes);
		}

        protected virtual void ClientRecvPacket(byte[] Data)
        {
            if (!mMapAryRecvData.ContainsKey(mEmptyLinkInfoForDatacmp))
            {
                mMapAryRecvData.Add(mEmptyLinkInfoForDatacmp, new List<byte>());
            }
            mMapAryRecvData[mEmptyLinkInfoForDatacmp].AddRange(Data);

            byte[] patternInBytes = Encoding.UTF8.GetBytes(mPacketPattern.ToCharArray());
            UnpackMessage(mEmptyLinkInfoForDatacmp, patternInBytes);
        }

		private void SendToServer(byte[] Data)
		{
			try
			{
                foreach(List<byte> dataSplited in ArrayHelper.SplitList(Data.ToList(), mSizeLimitEachSend))
                {
                    byte[] dataInBytes = dataSplited.ToArray();
                    mClient.Send(dataInBytes, dataInBytes.Length);
                    Thread.Sleep(1);
                }
            }
			catch (Exception ex)
			{
				Util.OutputDebugMessage(ex.StackTrace.ToString());
			}
		}

		private void SendToClient(byte[] Data, List<LinkInfo> Targets)
		{
			if (Targets == null)
			{
				Targets = mLinkedClients;
			}

			foreach (LinkInfo linkInfo in Targets)
			{
				try
				{
                    IPEndPoint remoteIPEndPoint = new IPEndPoint(linkInfo.IP, linkInfo.Port);

                    foreach (List<byte> dataSplited in ArrayHelper.SplitList(Data.ToList(), mSizeLimitEachSend))
                    {
                        byte[] dataInBytes = dataSplited.ToArray();
                        mServer.SendTo(dataInBytes, remoteIPEndPoint);
                        Thread.Sleep(1);
                    }
				}
				catch (Exception ex)
				{
					Util.OutputDebugMessage(ex.StackTrace.ToString());
				}
			}
		}

		private void SendImplement(NetworkStream NS, byte[] Data)
		{
			NS.Write(Data, 0, Data.Length);
			NS.Flush();
		}

        protected override void AddLinkedClient(LinkInfo Connection)
		{
			if (!mMapSocketInfo.ContainsKey(Connection.GetFullIPAddress()))
			{
				mLinkedClients.Add(Connection);
				mMapSocketInfo.Add(Connection.GetFullIPAddress(), Connection);
			}
		}

		private void RemoveLinkedClient(LinkInfo Connection)
		{
			LinkInfo result = mLinkedClients.Find(info => info.GetFullIPAddress() == Connection.GetFullIPAddress());
			if (result != null)
			{
				mLinkedClients.Remove(result);
				mMapSocketInfo.Remove(result.GetFullIPAddress());
			}
		}
        

        delegate void AsyncRecvDataCallback(DataPacket Packet);
		// class end
	}
}
