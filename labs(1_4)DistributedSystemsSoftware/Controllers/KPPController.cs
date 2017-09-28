using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Web;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Threading;
using labs_1_4_DistributedSystemsSoftware.Hubs;

namespace labs_1_4_DistributedSystemsSoftware.Controllers
{
    public class KPPController : Controller
    {
        public ActionResult KPP0()
        {

            return PartialView();
        }

        public void OleDBConnect(int dataCount = 500000, string userID = "")
        {
            HomeController.sendNotificationSound("<h3>Обработка</h3><div class='notRow'>OleDB старт</div>", userID);
            string conStrOleDB = "Provider=SQLOLEDB;Server=tcp:labs1to4distributedsystemssoftware.database.windows.net,1433;" +
                "Initial Catalog=labs(1_4)DistributedSystemsSoftware;Persist Security Info=False;User ID=labs1to4;Password=!primestud10;" +
                "MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            OleDbConnection oleDbConnection = new OleDbConnection(conStrOleDB);
            if (@Request.Params.Get("dataCount") != null)
            {
                int.TryParse(@Request.Params.Get("dataCount"), out dataCount);
            }
            userID = @Request.Params.Get("userID");

            var watch = System.Diagnostics.Stopwatch.StartNew();
            Tuple<long, long> oleDb = new Tuple<long, long>(0, 0);
            Tuple<long, long> odbc = new Tuple<long, long>(0, 0);
            oleDbConnection.Open();

            Random rand = new Random();
            int prevPercent = 0;
            List<int> data = new List<int>();
            var json = new Dictionary<string, string>();
            OleDbCommand oleDbCommand = new OleDbCommand();
            string commStr = "";
            int progress = 0;

            Hubs.mainRHub.processProgress(0, userID);
            for (int i = 0; i < dataCount; i++)
            {
                data.Add(rand.Next());
                commStr = "INSERT INTO values_table (value) VALUES (?)";
                oleDbCommand = new OleDbCommand(commStr, oleDbConnection);
                oleDbCommand.Parameters.Add("value", OleDbType.Integer).Value = data.Last();
                oleDbCommand.ExecuteNonQuery();

                progress++;
                if ((((progress * 25 / dataCount) % 1 == 0) && (progress * 25 / dataCount) % 10 != prevPercent) ||
                        (progress / dataCount == 1))
                {
                    Hubs.mainRHub.processProgress((progress * 25 / dataCount), userID);
                }
                prevPercent = (progress * 25 / dataCount) % 10;
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            oleDbConnection.Close();
            oleDb = new Tuple<long, long>(elapsedMs, 0);

            oleDbConnection.Open();
            watch = System.Diagnostics.Stopwatch.StartNew();
            commStr = "SELECT * FROM values_table";
            oleDbCommand = new OleDbCommand(commStr, oleDbConnection);
            oleDbCommand.ExecuteNonQuery();
            watch.Stop();
            elapsedMs = watch.ElapsedMilliseconds;
            oleDb = new Tuple<long, long>(oleDb.Item1, elapsedMs);
            progress = 50;
            Hubs.mainRHub.processProgress(progress, userID);

            commStr = "TRUNCATE TABLE [dbo].[values_table];";
            oleDbCommand.ExecuteNonQuery();
            oleDbConnection.Close();

            json = new Dictionary<string, string>();
            json.Add("message", "Время выполнения insert/ select по OleDB в ms");
            json.Add("value", oleDb.ToString());
            Hubs.mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);

            odbc = OdbcConnect(dataCount, userID, progress);

            json = new Dictionary<string, string>();
            json.Add("title", "Выводы");
            Hubs.mainRHub.sendConclusionOfCalculating(JsonConvert.SerializeObject(json), userID, Hubs.mainRHub.typeOfSendingString.rowsTitle);

            json = new Dictionary<string, string>();
            string resultString = String.Empty;
            if (oleDb.Item1 > odbc.Item1)
            {
                resultString += "OleDB в случае записи данных проигрывает ODBC<br />";
            }
            else
            {
                resultString += "OleDB в случае записи данных выигрывает у ODBC<br />";
            }
            if (oleDb.Item2 > odbc.Item2)
            {
                resultString += "OleDB в случае считывании данных проигрывает ODBC";
            }
            else
            {
                resultString += "OleDB в случае считывании данных выигрывает у ODBC";
            }
            json.Add("conclusion", resultString);
            Hubs.mainRHub.sendConclusionOfCalculating(JsonConvert.SerializeObject(json), userID);
        }

        private Tuple<long, long> OdbcConnect(int dataCount = 500000, string userID = "", int startPercent = 0)
        {
            string conStrODBC = "Driver={ODBC Driver 13 for SQL Server};Server=tcp:labs1to4distributedsystemssoftware.database.windows.net,1433;" +
                "database=labs(1_4)DistributedSystemsSoftware;uid=labs1to4@labs1to4distributedsystemssoftware;Pwd=!primestud10;encrypt=yes;" +
                "trustservercertificate=no;connection timeout=30";
            OdbcConnection ODBCConnection = new OdbcConnection(conStrODBC);
            Tuple<long, long> odbc = new Tuple<long, long>(0, 0);

            var watch = System.Diagnostics.Stopwatch.StartNew();
            ODBCConnection.Open();

            Random rand = new Random();
            int prevPercent = startPercent;
            List<int> data = new List<int>();
            var json = new Dictionary<string, string>();
            OdbcCommand odbcDbCommand = new OdbcCommand();
            string commStr = "";
            int progress = startPercent;

            HomeController.sendNotificationSound("<h3>Обработка</h3><div class='notRow'>ODBC старт</div>", userID);
            for (int i = 0; i < dataCount; i++)
            {
                data.Add(rand.Next());
                commStr = "INSERT INTO values_table (value) VALUES (?)";
                odbcDbCommand = new OdbcCommand(commStr, ODBCConnection);
                odbcDbCommand.Parameters.AddWithValue("value", data.Last());
                odbcDbCommand.ExecuteNonQuery();

                progress++;
                if ((((progress * 25 / dataCount) % 1 == 0) && (progress * 25 / dataCount) % 10 != prevPercent) ||
                        (progress / dataCount == 1))
                {
                    Hubs.mainRHub.processProgress((progress * 25 / dataCount), userID);
                }
                prevPercent = (progress * 25 / dataCount) % 10;
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            ODBCConnection.Close();
            odbc = new Tuple<long, long>(elapsedMs, 0);

            ODBCConnection.Open();
            watch = System.Diagnostics.Stopwatch.StartNew();
            commStr = "SELECT * FROM values_table";
            odbcDbCommand = new OdbcCommand(commStr, ODBCConnection);
            odbcDbCommand.ExecuteNonQuery();
            watch.Stop();
            elapsedMs = watch.ElapsedMilliseconds;
            odbc = new Tuple<long, long>(odbc.Item1, elapsedMs);

            json = new Dictionary<string, string>();
            json.Add("message", "Время выполнения insert/ select по ODBC в ms");
            json.Add("value", odbc.ToString());
            Hubs.mainRHub.sendResultOfCalculating(JsonConvert.SerializeObject(json), userID);

            commStr = "TRUNCATE TABLE [dbo].[values_table];";
            odbcDbCommand.ExecuteNonQuery();
            ODBCConnection.Close();

            progress = 100;
            Hubs.mainRHub.processProgress(progress, userID);

            return odbc;
        }

        public ActionResult KPP1()
        {
            Service_References.LabConnectionsService objConnClient = new Service_References.LabConnectionsService();
            return PartialView("KPP1", objConnClient.checkOleDBODBCconnections());
        }
        public ActionResult KPP2()
        {
            Service_References.SongsService objSongClient = new Service_References.SongsService();
            return PartialView("KPP2", objSongClient.GetSongs());
        }
    }
}