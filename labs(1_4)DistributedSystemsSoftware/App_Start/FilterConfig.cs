using System.Web;
using System.Web.Mvc;

namespace labs_1_4_DistributedSystemsSoftware
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ErrorHandler.AiHandleErrorAttribute());
        }
    }
}
