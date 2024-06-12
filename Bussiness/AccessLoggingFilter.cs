using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UMC_Email.Bussiness
{
    public class AccessLoggingFilter :ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Lấy thông tin về yêu cầu
            //var request = filterContext.HttpContext.Request;
            //var ipAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress;
            //var userAgent = request.UserAgent;

            //// Ghi lại thông tin vào cơ sở dữ liệu
            //using (var db = new ApplicationDbContext()) // Sửa thành DbContext của bạn
            //{
            //    var visit = new Visit
            //    {
            //        VisitTime = DateTime.Now,
            //        IpAddress = ipAddress,
            //        UserAgent = userAgent
            //    };
            //    db.Visits.Add(visit);
            //    db.SaveChanges();
            //}

            //base.OnActionExecuting(filterContext);
        }
    }
}