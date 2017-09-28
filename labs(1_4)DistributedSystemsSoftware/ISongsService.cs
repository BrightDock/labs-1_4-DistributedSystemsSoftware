using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace labs_1_4_DistributedSystemsSoftware
{
    public class ISongsService
    {
        [ServiceContract]
        public interface IService
        {
            [OperationContract]
            List<Songs> GetSongs();
            // TODO: Добавьте здесь операции служб
        }
    }
}