using LouieTool;
using LouieTool.NetworkHandle;
using System;
using System.Text;
using System.Windows.Forms;

namespace SocketExample
{
    public partial class MainForm : Form
    {
        SocketImplement mServer;
        SocketImplement mClient;

        public MainForm()
        {
            InitializeComponent();

            this.FormClosing += MainForm_FormClosing;

            mServer = new SocketImplement(mTextServerRecv);
            mServer.InitServer(12345);
            mServer.StartServer();

            mClient = new SocketImplement(mTextClientRecv);
            mClient.InitClient("127.0.0.1", 12345);
            mClient.StartClient();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mServer.Shutdown();
            mClient.Shutdown();
        }

        private void mBtnServerSend_Click(object sender, EventArgs e)
        {
            mServer.Send(mTextServerSend.Text);
            mTextServerSend.SetPropertyThreadSafe(() => mTextServerSend.Text, string.Empty);
        }

        private void mBtnClientSend_Click(object sender, EventArgs e)
        {
            mClient.Send(mTextClientSend.Text);
            mTextClientSend.SetPropertyThreadSafe(() => mTextClientSend.Text, string.Empty);
        }

        private class SocketImplement : TCPSocket
        {
            TextBox mTextRecv;

            public SocketImplement(TextBox TextBoxRecv)
            {
                mTextRecv = TextBoxRecv;
            }

            protected override void RecvDataCallback(DataPacket Packet)
            {
                string message = Encoding.UTF8.GetString(Packet.Data.ToArray());

                if(mTextRecv != null)
                {
                    mTextRecv.SetPropertyThreadSafe(() => mTextRecv.Text, message);
                }
            }

            protected override void ServerAcceptNewLink(LinkInfo Connection)
            {
                base.ServerAcceptNewLink(Connection);

                Console.WriteLine($"server accept new link -> {Connection.IP}:{Connection.Port}");
            }
        }
        

    }
}
