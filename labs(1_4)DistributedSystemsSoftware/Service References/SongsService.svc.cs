using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

namespace labs_1_4_DistributedSystemsSoftware.Service_References
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SongsService : ISongsService
    {
        // Чтобы использовать протокол HTTP GET, добавьте атрибут [WebGet]. (По умолчанию ResponseFormat имеет значение WebMessageFormat.Json.)
        // Чтобы создать операцию, возвращающую XML,
        //     добавьте [WebGet(ResponseFormat=WebMessageFormat.Xml)]
        //     и включите следующую строку в текст операции:
        //         WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
        [OperationContract]
        public List<Songs> GetSongs()
        {
            labDB DB = new labDB();
            // Добавьте здесь реализацию операции
            return DB.Songs.ToList();
        }

        // Добавьте здесь дополнительные операции и отметьте их атрибутом [OperationContract]
    }
}
