using Microsoft.Ajax.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace labs_1_4_DistributedSystemsSoftware.Hubs
{
    public class mainRHub : Hub<mainRHub>
    {
        static List<string> IDs = new List<string>();
        // Отправка сообщений
        public static void Send(string message)
        {
            HubContext.Clients.All.addMessage(message);
        }

        public static void SongAdded(string songName, string userID)
        {
            HubContext.Clients.All.Notify(songName, IDs);
        }

        public static void NotifyByID(string message, string userID)
        {
            HubContext.Clients.Client(userID.Trim()).Notify(message);
        }

        public static void addChart(string jsonData, string title, string userID, string chartType = "column")
        {
            HubContext.Clients.Client(userID.Trim()).addChart(jsonData, title, chartType);
        }

        public static void sendResultOfCalculating(string result, string userID)
        {
            HubContext.Clients.Client(userID.Trim()).calcResult(result);
        }

        public static void sendConclusionOfCalculating(string result, string userID, typeOfSendingString dataType = typeOfSendingString.row)
        {
            HubContext.Clients.Client(userID.Trim()).calcConclusion(result, System.Enum.GetName(typeof(typeOfSendingString), (int)dataType));
        }

        public static void NotifyAll(string message)
        {
            HubContext.Clients.All.Notify(message);
        }

        public static void processProgress(double percentage, string userID)
        {
            HubContext.Clients.Client(userID.Trim()).processProgress(percentage.ToString("0.000", System.Globalization.CultureInfo.GetCultureInfo("en-US")));
        }

        // Подключение нового пользователя
        public void Connect()
        {
            var id = Context.ConnectionId;


            if (!IDs.Any(x => x == id))
            {
                IDs.Add(id);

                // Посылаем сообщение текущему пользователю
                Clients.Caller.onConnected(id, IDs);
                // Посылаем сообщение всем пользователям, кроме текущего
//                Clients.AllExcept(id).onNewUserConnected(id);
            }
        }

        // Отключение пользователя
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var item = IDs.FirstOrDefault(x => x == Context.ConnectionId);
            if (item != null)
            {
                IDs.Remove(item);
                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id);
            }

            return base.OnDisconnected(stopCalled);
        }

        public enum typeOfSendingString {rowsTitle = 0, row};
    }
}