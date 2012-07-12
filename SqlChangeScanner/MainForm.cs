using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SqlChangeScanner
{
    public partial class MainForm : Form
    {
        private ChangeTracker _presenter;
        private MySettings _mySettings;

        protected StatusBar mainStatusBar = new StatusBar();
        protected StatusBarPanel statusPanel = new StatusBarPanel();
    


        public MainForm()
        {
            _presenter = new ChangeTracker(this);
            _mySettings = new MySettings();
            InitializeComponent();
            InitFormView();
            CreateStatusBar();

        }



        private void CreateStatusBar()
        {
            // Set first panel properties and add to StatusBar
            statusPanel.BorderStyle = StatusBarPanelBorderStyle.Sunken;
            statusPanel.Text = "Not connected";
            statusPanel.ToolTipText = "Last Activity";
            statusPanel.AutoSize = StatusBarPanelAutoSize.Spring;
            mainStatusBar.Panels.Add(statusPanel);
            mainStatusBar.ShowPanels = true;
            // Add StatusBar to Form controls
            this.Controls.Add(mainStatusBar);

        }

        private void InitFormView()
        {
            UpdateFromConfigurationFile();
            WhenNotConnectedThenDisableScanButton();
            DisconnectButtonShouldBeDisabledWhenNoConnection();
            ConnectButtonShouldBeDisabledWhenConnectionStringIsEmpty();
        }

        private void UpdateFromConfigurationFile()
        {
            txtConnectionString.Text = _mySettings.ConnectionString;
        }

        private void ConnectButtonShouldBeDisabledWhenConnectionStringIsEmpty()
        {
            if (String.IsNullOrEmpty(txtConnectionString.Text))
            {
                btnConnect.Enabled = false;
            }
            else
            {
                btnConnect.Enabled = !_presenter.IsConnected;
            }
        }

        private void DisconnectButtonShouldBeDisabledWhenNoConnection()
        {
            btnDisconnect.Enabled = _presenter.IsConnected;
        }

        private void WhenNotConnectedThenDisableScanButton()
        {
            if (!_presenter.IsConnected)
            {
                btnStartScan.Enabled = false;
            }
        }

        private void txtConnectionString_TextChanged(object sender, EventArgs e)
        {
            ConnectButtonShouldBeDisabledWhenConnectionStringIsEmpty();
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            _mySettings.ConnectionString = txtConnectionString.Text;
            _mySettings.Save();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            _presenter.ConnectToDatabase(txtConnectionString.Text);
        }

        public void Log(string logLine)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker invoker = () => txtConsole.AppendText(logLine + "\r\n");
                this.Invoke(invoker);
            }

            else
            {
                txtConsole.AppendText(logLine + "\r\n");
            }
        }

        private void btnClearConsole_Click(object sender, EventArgs e)
        {
            txtConsole.Clear();
        }

        public void UpdateOnSuccessfulConnectionStarted()
        {
            btnDisconnect.Enabled = true;
            btnConnect.Enabled = false;
            btnStartScan.Enabled = true;
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            _presenter.DisconnectFromDatabase();
        }

        public void ConnectionClosed()
        {
            WhenNotConnectedThenDisableScanButton();
            DisconnectButtonShouldBeDisabledWhenNoConnection();
            ConnectionButtonShouldBeEnabledWhenNoConnection();
        }

        private void ConnectionButtonShouldBeEnabledWhenNoConnection()
        {
            btnConnect.Enabled = !_presenter.IsConnected;
        }

        private void btnStartScan_Click(object sender, EventArgs e)
        {

            if (!_presenter.IsScanInProgress)
            {
                ChangeStartButtonToCancel();
                _presenter.Scan();
            }
            else
            {
                _presenter.StopScan();
                ChangeCancelToStartScan();
            }
        }



        private void ChangeCancelToStartScan()
        {
            btnStartScan.Text = "Scan";
            
        }

        private void ChangeStartButtonToCancel()
        {
            btnStartScan.Text = "Cancel";
        }

        public void LogError(string message)
        {
            Log(message);
        }

        public void Status(string statusMessage)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker invoker = () => statusPanel.Text = statusMessage;
                this.Invoke(invoker);
            }

            else
            {
                statusPanel.Text = statusMessage;
            }            
            
            
        }

        public void InitProgress(int itemsMax)
        {
            MethodInvoker invoker = () =>
                                        {
                                            ctrlScanProgress.Minimum = 1;
                                            ctrlScanProgress.Maximum = itemsMax;
                                            ctrlScanProgress.Value = 1;
                                            ctrlScanProgress.Step = 1;
                                        };
            if (this.InvokeRequired)
            {
                this.Invoke(invoker);
            }

            else
            {
                invoker();
            }   
        }

        public void PerformStep()
        {
            MethodInvoker invoker = 
                () => ctrlScanProgress.PerformStep();
            
            if (this.InvokeRequired)
            {
                this.Invoke(invoker);
            }
            else
            {
                invoker();
            }   
            
        }

        public void UpdateScanIsStopped()
        {
            MethodInvoker invoker =
                            () => ChangeCancelToStartScan();

            if (this.InvokeRequired)
            {
                this.Invoke(invoker);
            }
            else
            {
                invoker();
            }   
            
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            txtConsole.Text =
@"
       _
    '-(-' _     _.....__    _   _          SqlChangeScanner (alpha)
       '-(-'  .'        '. | \_/_|         ~~~~~~~~~~~~~~~~~~~~~~~~ 
          `'-/            \/      \        Author: Dmitry Zhariy 
            |              `   6 6 |_         (Dmytro Zharii/Дмитрий Жарий), 2012
            |                     /..\     Contact:
             \       |          ,_\__/        http://zhariy.com
              /     /    /   ___.--'          @dzhariy
             <   .-;`----`\  \ \           Project Home:
              \  \ \       \  \ \             https://github.com/dzhariy/SqlChangeScanner
          jgs  \__\_\       \__\_\
 
";
        }
    }
}
