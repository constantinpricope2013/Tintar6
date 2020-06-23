using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tintar6
{
    public partial class Client : Form
    {
        private String clientStatusText = "";
        public Client()
        {
            InitializeComponent();
            textBox1.Text = "Your request: ";
            textBox2.Text = "Debug info: ";
            
            new ClientServer(this);
        }

        private void Client_Load(object sender, EventArgs e)
        {

        }

        public void setClientStatusText(String stringInput)
        {
            clientStatusText += stringInput + "\n";
            clientStatus.Text = clientStatusText;
        }

    }

    public class ClientServer
    {
        public static Client clientFrame;
        private static readonly Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int PORT = 100;

        public ClientServer(Client clientFrameParam)
        {
            clientFrame = clientFrameParam;
            ConnectToServer();
            RequestLoop();
            Exit();

        }


        private static void ConnectToServer()
        {
            int attempts = 0;

            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    clientFrame.setClientStatusText("Connection attempt " + attempts);
                    // Change IPAddress.Loopback to a remote IP to connect to a remote host.
                    ClientSocket.Connect(IPAddress.Loopback, PORT);
                }
                catch (SocketException)
                {
                    //Console.Clear();
                    return;
                }
            }

            //Console.Clear();
            clientFrame.setClientStatusText("Connected");
        }

        private static void RequestLoop()
        {
            clientFrame.setClientStatusText(@"<Type ""exit"" to properly disconnect client>");

            while (true)
            {
                SendRequest();
                ReceiveResponse();
            }
        }

        /// <summary>
        /// Close socket and exit program.
        /// </summary>
        private static void Exit()
        {
            SendString("exit"); // Tell the server we are exiting
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Environment.Exit(0);
        }

        private static void SendRequest()
        {
            clientFrame.setClientStatusText("Send a request: ");
            string request = clientFrame.clientRequest.Text;
            SendString(request);

            if (request.ToLower() == "exit")
            {
                Exit();
            }
        }

        /// <summary>
        /// Sends a string to the server with ASCII encoding.
        /// </summary>
        private static void SendString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private static void ReceiveResponse()
        {
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            Console.WriteLine(text);
        }

    }
}
