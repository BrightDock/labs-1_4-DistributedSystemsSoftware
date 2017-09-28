using System;
using System.Data.Odbc;
using System.Data.OleDb;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace labs_1_4_DistributedSystemsSoftware.Service_References
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class LabConnectionsService
    {
        [OperationContract]
        public Tuple<Tuple<bool, bool>, Tuple<string, string>> checkOleDBODBCconnections()
        {
            Tuple<Tuple<bool, bool>, Tuple<string, string>> result;
            string oleDB = "Соединено";
            string ODBC = "Соединено";
            bool oleDbState = true;
            bool ODBCState = true;

            string conStrOleDB = "Provider=SQLOLEDB;Server=tcp:labs1to4distributedsystemssoftware.database.windows.net,1433;" +
                "Initial Catalog=labs(1_4)DistributedSystemsSoftware;Persist Security Info=False;User ID=labs1to4;Password=!primestud10;" +
                "MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            OleDbConnection oleDbConnection = new OleDbConnection(conStrOleDB);
            try
            {
                oleDbConnection.Open();
                oleDbConnection.Close();
            }
            catch {
                oleDB = "ошибка соединения";
                oleDbState = false;
            }
            
            string conStrODBC = "Driver={ODBC Driver 13 for SQL Server};Server=tcp:labs1to4distributedsystemssoftware.database.windows.net,1433;" +
                "database=labs(1_4)DistributedSystemsSoftware;uid=labs1to4@labs1to4distributedsystemssoftware;Pwd=!primestud10;encrypt=yes;" +
                "trustservercertificate=no;connection timeout=30";

            OdbcConnection ODBCConnection = new OdbcConnection(conStrODBC);
            try
            {
                ODBCConnection.Open();
                ODBCConnection.Close();
            }
            catch
            {
                ODBC = "ошибка соединения";
                ODBCState = false;
            }

            result =new Tuple<Tuple<bool, bool>, Tuple<string, string>>(new Tuple<bool, bool>(oleDbState, ODBCState), 
                new Tuple<string, string>(oleDB, ODBC));
            return result;
        }
    }
}
