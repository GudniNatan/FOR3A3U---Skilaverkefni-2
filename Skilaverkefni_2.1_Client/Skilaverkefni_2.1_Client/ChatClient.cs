//Modified chat client
// Fig. 23.2: Skilaverkefni_2._1_Client.cs
// Set up a client that will send information to and
// read information from a server.
using System;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

public partial class ChatClientForm : Form
{
    public ChatClientForm()
    {
        InitializeComponent();
    } // end constructor

    private NetworkStream output; // stream for receiving data           
    private BinaryWriter writer; // facilitates writing to the stream    
    private BinaryReader reader; // facilitates reading from the stream  
    private Thread readThread; // Thread for processing incoming messages
    private string message = "";
    private string currentFile = null;
    private bool fileMode = false;

    // initialize thread for reading
    private void ChatClientForm_Load(object sender, EventArgs e)
    {
        readThread = new Thread(new ThreadStart(RunClient));
        readThread.Start();
    } // end method ChatClientForm_Load

    // close all threads associated with this application
    private void ChatClientForm_FormClosing(object sender,
       FormClosingEventArgs e)
    {
        System.Environment.Exit(System.Environment.ExitCode);
    } // end method ChatClientForm_FormClosing

    // delegate that allows method DisplayMessage to be called
    // in the thread that creates and maintains the GUI       
    private delegate void DisplayDelegate(string message);

    // method DisplayMessage sets displayTextBox's Text property
    // in a thread-safe manner
    private void DisplayMessage(string message)
    {
        // if modifying displayTextBox is not thread safe
        if (displayTextBox.InvokeRequired)
        {
            // use inherited method Invoke to execute DisplayMessage
            // via a delegate
            Invoke(new DisplayDelegate(DisplayMessage),
              new object[] { message });
            
        } // end if
        else // OK to modify displayTextBox in current thread+
            displayTextBox.Text += message;
    } // end method DisplayMessage

    private delegate void ClearOutputDelegate();

    private void ClearOutput()
    {
        if (displayTextBox.InvokeRequired)
        {
            Invoke(new ClearOutputDelegate(ClearOutput));
        }
        else
        {
            displayTextBox.Clear();
        }
    }

    private delegate void EnableButtonDelegate(bool enabled);

    private void EnableButton(bool enabled)
    {
        if (updateButton.InvokeRequired)
        {
            Invoke(new EnableButtonDelegate(EnableButton),
              new object[] { enabled });
        }
        else
        {
            updateButton.Enabled = enabled;
        }
    }
    private delegate void EnableTextDelegate(bool enabled);

    private void EnableText(bool enabled)
    {
        if (updateButton.InvokeRequired)
        {
            Invoke(new EnableTextDelegate(EnableText),
              new object[] { enabled });
        }
        else
        {
            displayTextBox.ReadOnly = !enabled;
        }
    }

    // delegate that allows method DisableInput to be called 
    // in the thread that creates and maintains the GUI
    private delegate void DisableInputDelegate(bool value);

    // method DisableInput sets inputTextBox's ReadOnly property
    // in a thread-safe manner
    private void DisableInput(bool value)
    {
        // if modifying inputTextBox is not thread safe
        if (inputTextBox.InvokeRequired)
        {
            // use inherited method Invoke to execute DisableInput
            // via a delegate                                     
            Invoke(new DisableInputDelegate(DisableInput),
               new object[] { value });
        } // end if
        else // OK to modify inputTextBox in current thread
            inputTextBox.ReadOnly = value;
    } // end method DisableInput

    // sends text the user typed to server
    private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (e.KeyCode == Keys.Enter && inputTextBox.ReadOnly == false)
            {
                writer.Write("CLIENT>>> " + inputTextBox.Text);
                displayTextBox.Text += "\r\nCLIENT>>> " + inputTextBox.Text;
                inputTextBox.Clear();
            } // end if
        } // end try
        catch (SocketException)
        {
            displayTextBox.Text += "\nError writing object";
        } // end catch
    } // end method inputTextBox_KeyDown

    // connect to server and display server-generated text
    public void RunClient()
    {
        TcpClient client;

        // instantiate TcpClient for sending data to server
        try
        {
            DisplayMessage("Attempting connection\r\n");

            // Step 1: create TcpClient and connect to server
            client = new TcpClient();
            IPAddress server = IPAddress.Parse("127.0.0.1");
            client.Connect(server, 50000);

            // Step 2: get NetworkStream associated with TcpClient
            output = client.GetStream();

            // create objects for writing and reading across stream
            writer = new BinaryWriter(output);
            reader = new BinaryReader(output);

            DisplayMessage("\r\nGot I/O streams\r\n");
            DisableInput(false); // enable inputTextBox

            // loop until server signals termination
            do
            {
                // Step 3: processing phase
                try
                {
                    // read message from server        
                    message = reader.ReadString();
                    DisplayMessage(message + "\r\n");
                } // end try
                catch (Exception)
                {
                    // handle exception if error in reading server data
                    System.Environment.Exit(System.Environment.ExitCode);
                } // end catch
                if (!string.IsNullOrEmpty(message) && message.Split()[0] == "FILE>>>")
                {
                    ClearOutput();
                    EnableButton(true);
                    EnableText(true);
                    currentFile = message.Split()[1];
                    int headerLength = message.Split()[0].Length + message.Split()[1].Length;
                    string[] lines = message.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                    foreach (var item in lines)
                    {
                        DisplayMessage(item + "\n");
                    }
                    fileMode = true;
                }
            } while (message != "SERVER>>> TERMINATE");

            // Step 4: close connection
            writer.Close();
            reader.Close();
            output.Close();
            client.Close();

            Application.Exit();
        } // end try
        catch (Exception error)
        {
            // handle exception if error in establishing connection
            MessageBox.Show(error.ToString(), "Connection Error",
               MessageBoxButtons.OK, MessageBoxIcon.Error);
            System.Environment.Exit(System.Environment.ExitCode);
        } // end catch
    }

    private void updateButton_Click(object sender, EventArgs e)
    {
        if (fileMode)
        {
            writer.Write("FILE>>> " + currentFile + displayTextBox.Text);
            ClearOutput();
            EnableButton(false);
            EnableText(false);
        }
    } // end method RunClient
} // end class ChatClientForm

/**************************************************************************
 * (C) Copyright 1992-2006 by Deitel & Associates, Inc. and               *
 * Pearson Education, Inc. All Rights Reserved.                           *
 *                                                                        *
 * DISCLAIMER: The authors and publisher of this book have used their     *
 * best efforts in preparing the book. These efforts include the          *
 * development, research, and testing of the theories and programs        *
 * to determine their effectiveness. The authors and publisher make       *
 * no warranty of any kind, expressed or implied, with regard to these    *
 * programs or to the documentation contained in these books. The authors *
 * and publisher shall not be liable in any event for incidental or       *
 * consequential damages in connection with, or arising out of, the       *
 * furnishing, performance, or use of these programs.                     *
 *************************************************************************/