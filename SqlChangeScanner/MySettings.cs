using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SqlChangeScanner
{
    public class MySettings
    {
        private const string ConnectionStringCfgKey = "ConnectionString";
        public string ConnectionString { get; set; }

        private const string GetAllTablesSqlCfgKey = "GetAllTablesSql";
        public string GetAllTablesSql { get; set; }

        private const string GetCheckSumSqlCfgKey = "GetCheckSumSql";
        public string GetCheckSumSql { get; set; }
        
        
        private const string SQL_GetAllTablesDefault = 
            @" SELECT '[' + sys.schemas.name + '].[' + sys.Tables.name + ']' AS TABLEFULLNAME"
          + @" FROM sys.Tables"
          + @" JOIN sys.schemas ON sys.Tables.schema_id = sys.schemas.schema_id"
          + @" ORDER BY sys.schemas.name, sys.Tables.name";

        private const string SQL_GetCheckSumDefault = @"SELECT CHECKSUM_AGG(BINARY_CHECKSUM(*)) FROM  {0} WITH (NOLOCK)";


        public MySettings()
        {
            ConnectionString = ConfigurationManager.AppSettings[ConnectionStringCfgKey] ?? "";
            if (String.IsNullOrEmpty(ConnectionString))
            {
                ConnectionString = "Data Source=(local);Initial Catalog=temp;User ID=!!!SQL_USER!!!;Password=!!!SQL_PWD!!!;";
            }

            GetAllTablesSql = ConfigurationManager.AppSettings[GetAllTablesSqlCfgKey] ?? "";
            if (String.IsNullOrEmpty(GetAllTablesSql))
            {
                GetAllTablesSql = SQL_GetAllTablesDefault;
            }

            GetCheckSumSql = ConfigurationManager.AppSettings[GetCheckSumSqlCfgKey] ?? "";
            if (String.IsNullOrEmpty(GetCheckSumSql))
            {
                GetCheckSumSql = SQL_GetCheckSumDefault;
            }



        }

        public void Save()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            AppSettingsSection app = config.AppSettings;
            app.Settings.Remove(ConnectionStringCfgKey);
            app.Settings.Add(ConnectionStringCfgKey, ConnectionString);


            app.Settings.Remove(GetAllTablesSqlCfgKey);
            app.Settings.Add(GetAllTablesSqlCfgKey, GetAllTablesSql);

            app.Settings.Remove(GetCheckSumSqlCfgKey);
            app.Settings.Add(GetCheckSumSqlCfgKey, GetCheckSumSql);

            config.Save(ConfigurationSaveMode.Modified);
        }



    }
}
