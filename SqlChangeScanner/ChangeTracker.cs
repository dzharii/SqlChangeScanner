using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading;
namespace SqlChangeScanner
{
    public class ChangeTracker
    {
        private MainForm _view;
        private SqlConnection _sqlConnection;
        public bool IsConnected { get; private set; }


        Dictionary<string, Int32?> _lastTables = new Dictionary<string, Int32?>();
        private bool _shouldStopScan;
        public bool IsScanInProgress { get; set; }


        private Thread _scannerThread;


        public ChangeTracker(MainForm view)
        {
            _view = view;
            _sqlConnection = null;
            IsConnected = false;
            IsScanInProgress = false;
        }


        public void ConnectToDatabase(string connectionString)
        {
            SqlConnection con;

            try
            {
                con = new SqlConnection(connectionString);
                con.Open();
            }
            catch (Exception e)
            {

                _view.LogError("SQL Connection failed");
                _view.LogError("Connection string: " + connectionString);
                _view.LogError("With the following error:");
                _view.LogError(e.Message);
                _view.Status("Error");
                return;
            }

            _view.Status("Connected");
            _view.Log("== Connected");
            _view.Log("=====================================================");
            _view.Log("Please start Scan to collect Database initial data. Then clean console and press Scan when you want to see the differences.");
            _view.UpdateOnSuccessfulConnectionStarted();
            _sqlConnection = con;
            IsConnected = true;

        }

        public void DisconnectFromDatabase()
        {
            try
            {
                if (_sqlConnection != null)
                {
                    _sqlConnection.Close();    
                }
                
            }
            catch (Exception e)
            {

                _view.LogError("SQL CLOSE Connection error");
                _view.LogError("With the following error:");
                _view.LogError(e.Message);
            }
            IsConnected = false;
            _view.ConnectionClosed();

        }


        public void SetScanIsCanceled()
        {
            _view.Status("Scan Canceled");
            _view.UpdateScanIsStopped();
            IsScanInProgress = false;
        }
        
        public void ScanWorker()
        {
            
            MySettings settings = new MySettings();
            _shouldStopScan = false;
            List<string> availableTables = new List<string>();

            _view.Status("Scanner: Getting all tables list");
            _view.Log("===================================================================");
            using (var command = new SqlCommand(settings.GetAllTablesSql, _sqlConnection))
            {
                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (_shouldStopScan)
                            {
                                SetScanIsCanceled();
                                return;
                            }
                            availableTables.Add(reader[0].ToString());
                        }
                    }
                }
                catch(Exception e)
                {
                    _view.LogError("Get All tables execution failed");
                    _view.LogError("QUERY: " + settings.GetAllTablesSql);
                    _view.LogError(e.Message);
                }
            }

            _view.InitProgress(availableTables.Count);


            using (var command = new SqlCommand())
            {
                command.Connection = _sqlConnection;
                foreach (var table in availableTables)
                {
                    if (_shouldStopScan )
                    {
                        SetScanIsCanceled();
                        return;
                    }
                    _view.Status("Scanning: " + table);
                    _view.PerformStep();
                    command.CommandText = String.Format(settings.GetCheckSumSql, table);
                    Int32? checksum = 0;
                    try
                    {
                        checksum = command.ExecuteScalar() as Int32?;
                    }
                    catch (Exception e)
                    {

                        _view.LogError(e.Message + " TABLE " + table);
                        _view.LogError("QUERY: " + settings.GetCheckSumSql);
                        continue;
                    }


                    bool printchanged = false;


                    if (!_lastTables.ContainsKey(table))
                    {
                        _lastTables[table] = 0;
                    }
                    
                    if (_lastTables[table] != checksum)
                    {
                        printchanged = true;
                        _lastTables[table] = checksum;
                    }

                    if (printchanged)
                    {
                        _view.Log(table);
                    }
                    
                }
                IsScanInProgress = false;
                _view.Status("Ready");
                _view.UpdateScanIsStopped();
            }
        }

        public void Scan()
        {
            IsScanInProgress = true;
            try
            {
                _scannerThread = new Thread(ScanWorker);
                _scannerThread.IsBackground = true;
                _scannerThread.Start();

            }
            catch (Exception e)
            {
                _view.LogError(e.Message);
                _view.UpdateScanIsStopped();
                IsScanInProgress = false;
            }
        }

        public void StopScan()
        {
            _shouldStopScan = true;
        }
    }
}
