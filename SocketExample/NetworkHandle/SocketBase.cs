using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LouieTool.NetworkHandle
{
    abstract class SocketBase
    {
        protected Dictionary<LinkInfo, List<DataPacket>> mMapProcessedPackets;
        protected string mPacketPattern = "DATA_START";

        protected Dictionary<LinkInfo, List<byte>> mMapAryRecvData;
        protected List<LinkInfo> mLinkedClients;
        protected LinkInfo mEmptyLinkInfoForDatacmp = new LinkInfo();
        

        public void SetPacketPattern(string Pattern)
        {
            mPacketPattern = Pattern;
        }


        private List<int> DatacmpIndex(byte[] Data, int DataSize, byte[] Pattern, int PatternSize, int MaxCount = 0)
        {
            List<int> output = new List<int>();

            if (PatternSize == 0)
                return output;
            if (DataSize < PatternSize)
                return output;

            for (int i = 0; i < DataSize; i++)
            {
                bool isFound = false;
                if (Data[i] == Pattern[0])
                {
                    isFound = true;
                    for (int j = 0; j < PatternSize; j++)
                    {
                        if (i + j >= DataSize)
                        {
                            isFound = false;
                            break;
                        }
                        if (Data[i + j] != Pattern[j])
                        {
                            isFound = false;
                            break;
                        }
                    }
                }
                if (isFound)
                {
                    output.Add(i);
                    if (MaxCount <= 0)
                        continue;
                    else
                    {
                        if (output.Count() == MaxCount)
                            break;
                    }
                }
            }

            return output;
        }

        // ********************** PROTECTED
        protected virtual void AddLinkedClient(LinkInfo Connection)
        {

        }

        protected virtual void ServerAcceptNewLink(LinkInfo Connection)
        {
            AddLinkedClient(Connection);
        }

        protected virtual void ServerLoseLink(LinkInfo Connection)
        {

        }

        protected virtual void ClientLoseLink(string linkedIp)
        {

        }

        protected virtual void RecvDataCallback(DataPacket Packet)
        {

        }

        protected virtual void PackMessage(string Message, out byte[] Data)
        {
            List<byte> packedMsg = new List<byte>();

            byte[] messageInBytes = Encoding.UTF8.GetBytes(Message.ToCharArray());

            long messageLen = messageInBytes.Length;
            packedMsg.AddRange(Encoding.UTF8.GetBytes(mPacketPattern.ToCharArray()));
            packedMsg.AddRange(BitConverter.GetBytes(messageLen));
            packedMsg.AddRange(messageInBytes);

            Data = packedMsg.ToArray();
        }

        protected virtual void UnpackMessage(LinkInfo SourceInfo, byte[] Pattern)
        {
            byte[] buffer = mMapAryRecvData[SourceInfo].ToArray();
            int bufferIndex = buffer.Length;
            List<int> indices = DatacmpIndex(buffer.ToArray(), buffer.Length, Pattern, Pattern.Length);

            if (!mMapProcessedPackets.ContainsKey(SourceInfo))
            {
                mMapProcessedPackets.Add(SourceInfo, new List<DataPacket>());
            }
            List<DataPacket> aryProcessedPacket = mMapProcessedPackets[SourceInfo];

            if (indices.Count > 0)
            {
                // data before index: append on previous array
                if (aryProcessedPacket.Count > 0)
                {
                    int subBufferLen = indices[0];
                    byte[] subBuffer = buffer.Take(subBufferLen).ToArray();
                    DataPacket packet = aryProcessedPacket[aryProcessedPacket.Count - 1];
                    packet.Data.AddRange(subBuffer);
                    packet.RecvCounter += subBufferLen;
                }

                // data after index: create a new array
                long remainDataIndex = 0;
                for (int i = 0; i < indices.Count; i++)
                {
                    if (indices[i] + Pattern.Length + 8 > bufferIndex)
                    {
                        remainDataIndex = indices[i];
                        break;
                    }

                    // pick data length
                    byte[] dataLenInByte = new byte[8];
                    Array.Copy(buffer, indices[i] + Pattern.Length, dataLenInByte, 0, 8);
                    long dataLen = BitConverter.ToInt64(dataLenInByte, 0);

                    // pick data
                    long indexBegin = indices[i] + Pattern.Length + 8;
                    long indexExpect = indexBegin + dataLen;
                    long indexEnd = i + 1 >= indices.Count ? (indexExpect > bufferIndex ? bufferIndex : indexExpect) : indices[i + 1];
                    long subBufferLen = indexEnd - indexBegin;
                    byte[] subBuffer = new byte[indexEnd - indexBegin];
                    Array.Copy(buffer, indexBegin, subBuffer, 0, subBufferLen);

                    DataPacket packet = new DataPacket();
                    packet.Data.AddRange(subBuffer);
                    packet.Len = dataLen;
                    packet.RecvCounter += subBufferLen;
                    packet.Owner = SourceInfo;

                    aryProcessedPacket.Add(packet);
                    remainDataIndex = indexEnd;
                }

                long remainBufferLen = bufferIndex - remainDataIndex;
                char[] remainBuffer = new char[remainBufferLen];
                Array.Copy(buffer, remainDataIndex, remainBuffer, 0, remainBufferLen);
                mMapAryRecvData[SourceInfo].RemoveRange(0, (int)remainDataIndex);
            }
            else
            {
                // data remained: append on previous array
                if (aryProcessedPacket.Count > 0)
                {
                    DataPacket packet = aryProcessedPacket[aryProcessedPacket.Count - 1];
                    packet.Data.AddRange(buffer);
                    packet.RecvCounter += buffer.Length;

                    mMapAryRecvData[SourceInfo].RemoveRange(0, buffer.Length);
                }
            }

            //List<IAsyncResult> listRecvDataCallbackResult = new List<IAsyncResult>();
            // receive data aryProcessedPacket
            for (int i = 0; i < aryProcessedPacket.Count(); i++)
            {
                DataPacket packet = aryProcessedPacket[i];
                if (packet.RecvCounter == packet.Len)
                {
                    AsyncRecvDataCallback asyncRecvDataCallbackDelegate = new AsyncRecvDataCallback(RecvDataCallback);
                    //IAsyncResult asyncResult = asyncRecvDataCallbackDelegate.BeginInvoke(packet, null, null);
                    var workTask = Task.Run(() => asyncRecvDataCallbackDelegate.Invoke(packet));
                    //listRecvDataCallbackResult.Add(asyncResult);

                    aryProcessedPacket.RemoveAt(i);
                    i--;
                }
            }
            mMapProcessedPackets[SourceInfo] = aryProcessedPacket;

            // clean async states
            /*
			for (int i = 0; i < listRecvDataCallbackResult.Count(); i++)
			{
				IAsyncResult asyncResult = listRecvDataCallbackResult[i];
				if (asyncResult.IsCompleted)
				{
					listRecvDataCallbackResult.RemoveAt(i);
					i--;
				}
			}
			*/
        }

        protected virtual void ExceptionCatched(Exception Ex)
        {

        }

        delegate void ExceptionCatchedCallback(Exception Ex);
        delegate void AsyncRecvDataCallback(DataPacket Packet);

    }
}
