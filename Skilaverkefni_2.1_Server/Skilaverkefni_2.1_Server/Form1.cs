using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Skilaverkefni_2._1_Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            readThread = new Thread(RunServer);
            readThread.Start();
        }

        private Socket connection;
        private Thread readThread;
        private NetworkStream socketStream;
        private BinaryWriter writer;
        private BinaryReader reader;

        private delegate void DisplayDelegate(string message);

        private void DisplayMessage(string message)
        {
            if (displayTextBox.InvokeRequired)
            {
                Invoke(new DisplayDelegate(DisplayMessage),
                   new object[] { message });
            }
            else 
                displayTextBox.Text += message;
        }

        public void RunServer()
        {
            TcpListener listener;
            int counter = 1;

            try
            {
                IPAddress local = IPAddress.Parse("127.0.0.1");
                listener = new TcpListener(local, 50000);

                listener.Start();

                while (true)
                {
                    DisplayMessage("Waiting for connection...\r\n");

                    connection = listener.AcceptSocket();

                    socketStream = new NetworkStream(connection);

                    writer = new BinaryWriter(socketStream);
                    reader = new BinaryReader(socketStream);

                    DisplayMessage("Connection " + counter + " received.\r\n");

                    writer.Write("SERVER>>> Connection successful");

                    string theReply = "";

                    do
                    {
                        try
                        {
                            theReply = reader.ReadString();

                            DisplayMessage(theReply + "\r\n");

                            if (theReply.Split()[1].ToLower() == "get")
                            {
                                List<string> skra = ReadFile(theReply.Split()[2]);
                                string superString = null;


                                foreach (var item in skra)
                                {
                                    superString += item;

                                    if (item != skra[skra.Count - 1])
                                    {
                                        superString += "\n";
                                    }
                                }

                                writer.Write("FILE>>> " + theReply.Split()[2] + "\n" + superString);
                                DisplayMessage("FILE>>> " + theReply.Split()[2] + "\n" + superString);

                                
                            }
                            if (theReply.Split()[0] == "FILE>>>")
                            {
                                DisplayMessage(theReply);
                                string[] lines = theReply.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                                string filename = lines[1];
                                List<string> file = new List<string>();
                                for (int i = 2; i < lines.Length; i++)
                                {
                                    file.Add(lines[i]);
                                }
                                WriteFile(filename, file);
                            }
                            

                        }
                        catch (Exception)
                        {

                            break;
                        }
                    } while (theReply != "CLIENT>>> TERMINATE" && connection.Connected);

                    DisplayMessage("\r\nUser Terminated Connection\r\n");

                    writer.Close();
                    reader.Close();
                    socketStream.Close();
                    connection.Close();

                    counter++;
                }
            }
            catch (SocketException)
            {

                throw;
            }
        }
        public List<string> ReadFile(string filename)
        {
            List<string> list = new List<string>();
            try
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        list.Add(line); // Add to list.
                    }
                }
            }
            catch (Exception ex)
            {
                list.Add(ex.Message);
            }
            
            return list;
        }
        public void WriteFile(string filename, List<string> file)
        {
            File.WriteAllLines(filename, file);
        }

       
    }
}
