using System.Web;
using System.Web.Mvc;
using UMC_Email.Bussiness;

namespace UMC_Email
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new AccessLoggingFilter());
        }
    }
}
