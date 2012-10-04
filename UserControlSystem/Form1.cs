using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ConsoleTestProject;

namespace UserControlSystem
{
    public partial class Form1 : Form
    {
        private ExternalCleitnManagerClient _clientsManager;

        public Form1()
        {
            InitializeComponent();
            
            dataGridView1.Columns.Add("UserID", "UserID");
            dataGridView1.Columns.Add("ListenedSID", "ListenedSID");
            dataGridView1.Columns.Add("Brocker", "Brocker");
            dataGridView1.Columns.Add("Email", "Email");
            
            try
            {
                _clientsManager = null;
                _clientsManager = new ExternalCleitnManagerClient();
                _clientsManager.Open();
            }
            catch (Exception e)
            {
                ShowError("Cannot initialize connection to web service");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.Text = _clientsManager.GetVersion();
            }
            catch (NullReferenceException) { }
            catch (Exception ex)
            {
                ShowError("Exception performing operation. Additional info :\n" + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label2.Text = Convert.ToString(_clientsManager.GetProblematicListenersCount());
        }
        // ------ Service procedures
        public void ShowError(String errorText)
        {
            MessageBox.Show(errorText, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                ClientInfo[] clientInfo = _clientsManager.GetAllProblematicClients();
                label4.Text = "Get " + clientInfo.Length + " users";
                dataGridView1.Rows.Clear();
                for (int i = 0; i < clientInfo.Length; ++i)
                {
                    dataGridView1.Rows.Add(clientInfo[i].ClientId, clientInfo[i].ClientsListenedSystemId.ToString(),
                        clientInfo[i].ClientBrocker, clientInfo[i].ClientEmail);
                }
            }
            catch (NullReferenceException) { }
            catch (Exception ex)
            {
                ShowError("Exception while perform operations. Detailed info :\n" + ex.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length == 0)
            {
                ShowError("User ID not entered in text box below. cannot continue");
            }
            else
            {
                try
                {
                    _clientsManager.ContinueClientWork(textBox2.Text, 0, ClientContinueMode.Synchronize);
                }
                catch (Exception ex)
                {
                    ShowError("Error performng operations. Additional info :\n" + ex.Message);
                }
            }
        }
    }
}
