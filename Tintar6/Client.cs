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
        int w = 800, h = 800;
        private int[] firstPlayerPosition = new int[] { 20, 200,
                                                        50, 200,
                                                        80, 200,
                                                        20, 230,
                                                        50, 230,
                                                        80, 230,
                                                        };
        private int[] secondPlayerPosition = new int[] { 520, 200,
                                                         550, 200,
                                                         580, 200,
                                                         520, 230,
                                                         550, 230,
                                                         580, 230,
                                                         };
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
            this.Invalidate();

        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            int width = 30;
            w = this.ClientSize.Width;
            h = this.ClientSize.Height;
            SolidBrush greenBrush = new SolidBrush(Color.Green);
            SolidBrush blueBrush = new SolidBrush(Color.Blue);


            g.DrawRectangle(Pens.Red, (w / 2) - 20, h / 2, 140, 120);
            g.DrawRectangle(Pens.Red, w / 2, (h / 2) + 20, 100, 80);
            
            
            //First Player
            g.FillEllipse(greenBrush, firstPlayerPosition[0], firstPlayerPosition[1], 20, 20);
            g.FillEllipse(greenBrush, firstPlayerPosition[2], firstPlayerPosition[3], 20, 20);
            g.FillEllipse(greenBrush, firstPlayerPosition[4], firstPlayerPosition[5], 20, 20);
            g.FillEllipse(greenBrush, firstPlayerPosition[6], firstPlayerPosition[7], 20, 20);
            g.FillEllipse(greenBrush, firstPlayerPosition[8], firstPlayerPosition[9], 20, 20);
            g.FillEllipse(greenBrush, firstPlayerPosition[10], firstPlayerPosition[11], 20, 20);

            //SecondPlayer
            g.FillEllipse(blueBrush, secondPlayerPosition[0], secondPlayerPosition[1], 20, 20);
            g.FillEllipse(blueBrush, secondPlayerPosition[2], secondPlayerPosition[3], 20, 20);
            g.FillEllipse(blueBrush, secondPlayerPosition[4], secondPlayerPosition[5], 20, 20);
            g.FillEllipse(blueBrush, secondPlayerPosition[6], secondPlayerPosition[7], 20, 20);
            g.FillEllipse(blueBrush, secondPlayerPosition[8], secondPlayerPosition[9], 20, 20);
            g.FillEllipse(blueBrush, secondPlayerPosition[10], secondPlayerPosition[11], 20, 20);


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
           // RequestLoop();
            //Exit();

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
                    clientFrame.setClientStatusText("Socket exception catched");
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
