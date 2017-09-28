using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WcfService
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "Service" в коде, SVC-файле и файле конфигурации.
    // ПРИМЕЧАНИЕ. Чтобы запустить клиент проверки WCF для тестирования службы, выберите элементы Service.svc или Service1.svc.cs в обозревателе решений и начните отладку.
    public class MyService : IService
    {      
        [WebGet]
        public List<Songs> GetSongs()
        {
            Entities DB = new Entities();
            return DB.Songs.ToList();
        }
    }
}
