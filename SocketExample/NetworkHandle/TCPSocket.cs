using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LouieTool.NetworkHandle
{
	abstract class TCPSocket : SocketBase
	{
		private TcpListener mServer = null;
		private TcpListener mClientListener = null;
		private TcpClient mClient = null;
		private SOCKET_PLUGIN_ENUM mSocketType = SOCKET_PLUGIN_ENUM.TYPE_NONE;
		private bool mFShutdown = false;
		private bool mFIsAlive = false;

        private Dictionary<Socket, LinkInfo> mMapSocketInfo;

        private Thread mThreadServer = null;
		private Thread mThreadClient = null;
		private Mutex mMutexAcceptNewClient = null;

		public List<string> mLinkedIP = new List<string>();
		public bool mStopListener = false;
		// ********************** PUBLIC
		public TCPSocket()
		{
			Clear();
		}

		public bool InitServer(ushort LocalPort)
		{
			bool rtn = false;

			try
			{
				IPEndPoint ipe = new IPEndPoint(IPAddress.Any, LocalPort);
				mServer = new TcpListener(ipe);
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
				mServer.Start();
				mServer.BeginAcceptTcpClient(DoAcceptTcpClientCallback, mServer);
				mMutexAcceptNewClient = new Mutex();
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

		private void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            // Get the listener that handles the client request.
            TcpListener listener = (TcpListener)ar.AsyncState;

			// End the operation and display the received data on
			// the console.
            if (!GetShutdownState())
			{
				TcpClient client = listener.EndAcceptTcpClient(ar);
				IPEndPoint clientIPEndPoint = ((IPEndPoint)client.Client.RemoteEndPoint);

				LinkInfo linkInfo = new LinkInfo();
				linkInfo.Client = client;
				linkInfo.IP = clientIPEndPoint.Address;
				linkInfo.Port = clientIPEndPoint.Port;

				ServerAcceptNewLink(linkInfo);

				listener.BeginAcceptTcpClient(DoAcceptTcpClientCallback, listener);
			}
        }

		public bool InitClient(string TargetIP, ushort TargetPort)
		{
			bool rtn = false;

			try
			{
				mClient = new TcpClient();
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

				mMutexAcceptNewClient = new Mutex();
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
			//packedMsg = System.Text.Encoding.ASCII.GetBytes(Message);

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
				mServer.Stop();
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

			mMutexAcceptNewClient.WaitOne();
			for (int i = mLinkedClients.Count - 1; i >= 0; i--)
			{
				if (!mLinkedClients[i].Client.Connected)
				{
					mLinkedClients.RemoveAt(i);
					mLinkedIP.RemoveAt(i);
				}
			}
			rtn = new List<LinkInfo>(mLinkedClients.ToArray());
			mMutexAcceptNewClient.ReleaseMutex();

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


				for (int i = 0; i < linkedClients.Count; i++)
				{
					LinkInfo linkInfo = linkedClients[i];

					List<Socket> socketListRead = new List<Socket>();
					socketListRead.Add(linkInfo.Client.Client);
					List<Socket> socketListError = new List<Socket>();
					socketListError.Add(linkInfo.Client.Client);
					Socket.Select(socketListRead, null, socketListError, 5000);

					if (socketListRead.Count != 0)
					{
						try
						{
							NetworkStream ns = linkInfo.Client.GetStream();
							StreamReader sr = new StreamReader(ns);
							//sr.BaseStream.ReadTimeout = 500;
							if (ns.CanRead)
							{
								do
								{
									int bufferSize = 65535;
									byte[] bufferRecv = new byte[bufferSize];

									try
									{
                                        int readLen = ns.Read(bufferRecv, 0, bufferSize);
										ServerRecvPacket(linkInfo, bufferRecv.Take(readLen).ToArray());

										if (readLen == 0 && sr.EndOfStream)
										{
											Console.WriteLine("Lose link");
											linkInfo.Client.Close();
										}
										//Console.WriteLine(ns.DataAvailable);
									}
									catch (Exception ex)
									{
										Util.OutputDebugMessage(ex.StackTrace.ToString());
									}
								}
								while (ns.DataAvailable);
							}
						}
						catch (Exception ex)
						{
							Util.OutputDebugMessage(ex.StackTrace.ToString());
						}
					}

					if (!linkInfo.Client.Connected)
					{
						RemoveLinkedClient(linkInfo);
						ServerLoseLink(linkInfo);
						linkedClients.RemoveAt(i);
						ClientLoseLink(mLinkedIP[i]);
						mLinkedIP.RemoveAt(i);
					}

				}
			}
		}

		private void ClientLiveLoop()
		{
			while (!GetShutdownState())
			{
				Thread.Sleep(10);
				try
				{
					NetworkStream ns = mClient.GetStream();
					StreamReader sr = new StreamReader(ns);
					// Receive the TcpServer.response.
					if (ns.CanRead)
					{
						int bufferSize = 65535;
						byte[] bufferRecv = new byte[bufferSize];

						try
						{
                            if (ns.DataAvailable)
                            {
                                ns.ReadTimeout = 50;
                                int readLen = ns.Read(bufferRecv, 0, bufferSize);
                                ClientRecvPacket(bufferRecv.Take(readLen).ToArray());
                            }
						}
						catch (Exception ex)
						{
							Util.OutputDebugMessage(ex.StackTrace.ToString());
						}
					}
				}
				catch (Exception ex)
				{
					Util.OutputDebugMessage(ex.StackTrace.ToString());
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
			mMapSocketInfo = new Dictionary<Socket, LinkInfo>();
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
				NetworkStream ns = mClient.GetStream();
				//StreamWriter sw = new StreamWriter(ns);
				SendImplement(ns, Data);
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
					if (linkInfo.Client.Connected)
					{
						NetworkStream ns = linkInfo.Client.GetStream();
						//StreamWriter sw = new StreamWriter(ns);
						SendImplement(ns, Data);
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
			mMutexAcceptNewClient.WaitOne();
			mLinkedClients.Add(Connection);
			mLinkedIP.Add(Connection.Client.Client.RemoteEndPoint.ToString());

			mMapSocketInfo.Add(Connection.Client.Client, Connection);
			mMutexAcceptNewClient.ReleaseMutex();
		}

		private void RemoveLinkedClient(LinkInfo Connection)
		{
			mMutexAcceptNewClient.WaitOne();
			LinkInfo result = mLinkedClients.Find(info => info.Client == Connection.Client);
			if (result != null)
			{
				mLinkedClients.Remove(result);
				mMapSocketInfo.Remove(result.Client.Client);
			}
			mMutexAcceptNewClient.ReleaseMutex();
		}


		// class end
	}
}
