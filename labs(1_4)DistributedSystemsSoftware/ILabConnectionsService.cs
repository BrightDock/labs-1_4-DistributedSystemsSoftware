using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace labs_1_4_DistributedSystemsSoftware
{
    public class ILabConnectionsService
    {
        [ServiceContract]
        public interface IService
        {
            [OperationContract]
            Tuple<string, string> checkOleDBODBCconnections();
        }
    }
}