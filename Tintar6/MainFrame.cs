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
    public partial class MainFrame : Form
    {
        private Form currentForm;
        private static String serverStatusText = "";
        public MainFrame()
        {
            InitializeComponent();
            currentForm = this;
            textBox1.Text = "Status Server: ";
            new Server(this);
        }

        public void setServerStatusText(String stringInput)
        {
            serverStatusText += stringInput + "\n";
            server_status.Text = serverStatusText;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }

        class Server
        {
            private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            private static readonly List<Socket> clientSockets = new List<Socket>();
            private const int BUFFER_SIZE = 2048;
            private const int PORT = 100;
            private static readonly byte[] buffer = new byte[BUFFER_SIZE];
            private static MainFrame mainFrame;

            public Server(MainFrame mainFrameParam)
            {
                // Console.Title = "Server";
                mainFrame = mainFrameParam;
                SetupServer();
               // Console.ReadLine(); // When we press enter close everything
                CloseAllSockets();
            }
            private void SetupServer()
            {
                mainFrame.setServerStatusText("Setting up server...");
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
                serverSocket.Listen(0);
                serverSocket.BeginAccept(AcceptCallback, null);
                mainFrame.setServerStatusText("Server setup complete");
            }

            /// <summary>
            /// Close all connected client (we do not need to shutdown the server socket as its connections
            /// are already closed with the clients).
            /// </summary>
            private static void CloseAllSockets()
            {
                foreach (Socket socket in clientSockets)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }

                serverSocket.Close();
            }

            private static void AcceptCallback(IAsyncResult AR)
            {
                Socket socket;

                try
                {
                    socket = serverSocket.EndAccept(AR);
                }
                catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
                {
                    return;
                }

                clientSockets.Add(socket);
                socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
                mainFrame.setServerStatusText("Client connected, waiting for request...");
                serverSocket.BeginAccept(AcceptCallback, null);
            }


            private static void ReceiveCallback(IAsyncResult AR)
            {
                Socket current = (Socket)AR.AsyncState;
                int received;

                try
                {
                    received = current.EndReceive(AR);
                }
                catch (SocketException)
                {
                    mainFrame.server_status.Text = "Client forcefully disconnected";
                    // Don't shutdown because the socket may be disposed and its disconnected anyway.
                    current.Close();
                    clientSockets.Remove(current);
                    return;
                }

                byte[] recBuf = new byte[received];
                Array.Copy(buffer, recBuf, received);
                string text = Encoding.ASCII.GetString(recBuf);
                mainFrame.setServerStatusText("Received Text: " + text);

                if (text.ToLower() == "get time") // Client requested time
                {
                    Console.WriteLine("Text is a get time request");
                    byte[] data = Encoding.ASCII.GetBytes(DateTime.Now.ToLongTimeString());
                    current.Send(data);
                    Console.WriteLine("Time sent to client");
                }
                else if (text.ToLower() == "exit") // Client wants to exit gracefully
                {
                    // Always Shutdown before closing
                    current.Shutdown(SocketShutdown.Both);
                    current.Close();
                    clientSockets.Remove(current);
                    Console.WriteLine("Client disconnected");
                    return;
                }
                else
                {
                    Console.WriteLine("Text is an invalid request");
                    byte[] data = Encoding.ASCII.GetBytes("Invalid request");
                    current.Send(data);
                    Console.WriteLine("Warning Sent");
                }

                current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            }

        }

    }
}
