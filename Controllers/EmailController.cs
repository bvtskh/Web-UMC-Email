using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using UMC_Email.Bussiness;
using UMC_Email.Models;

namespace UMC_Email.Controllers
{
    public class EmailController : Controller
    {
        List<UMC_EMAIL> selectedItems;
        // GET: Email
        DBContext context = new DBContext();

        public ActionResult Index()
        {
            // _ = EmailHelper.AvalidEmail("quyetpv@umcvn.com");
            CreateAccessLog();
            var mailList = context.UMC_EMAIL.ToList(); 
            return View(mailList);                    
        }

        private void CreateAccessLog()
        {
            // Lấy thông tin về yêu cầu
            var request = HttpContext.Request;
            var ipAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress;
            var userAgent = request.UserAgent;

            // Ghi lại thông tin vào cơ sở dữ liệu
            using (var dbContext = new DBContext())
            {
                var visit = new ACCESS_LOG
                {
                    AccsessTime = DateTime.Now,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };
                context.ACCESS_LOG.Add(visit);
                context.SaveChanges();
            }
        }
        public int TotalVisits()
        {
            using(var context = new DBContext() )
            {
                int totalVisits = context.ACCESS_LOG.Count();
                return totalVisits;
            }
        }

        public JsonResult Search(string searchTerm = "", int page = 1, int pageSize = 40)
        {
            var query = context.UMC_EMAIL.AsEnumerable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => NormalString.NormalizeString(p.DEPARTMENT).Contains(NormalString.NormalizeString(searchTerm)) || NormalString.NormalizeString(p.NAME).Contains(NormalString.NormalizeString(searchTerm)) || NormalString.NormalizeString(p.EMAIL).Contains(NormalString.NormalizeString(searchTerm))).ToList();
            }

            int totalItems = query.Count();
            var data = query.OrderBy(x => x.ID)
            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .Select(x => new
                            {
                                x.DEPARTMENT,
                                x.NAME,
                                x.EMAIL
                            }).OrderBy(o=>o.DEPARTMENT).ThenBy(t=>t.NAME).ToList();
            List<UMC_EMAIL> selectedItems = Session["SelectedItems"] as List<UMC_EMAIL> ?? new List<UMC_EMAIL>();
            var result = new
            {
                allData = data,
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                data = selectedItems,
                count = selectedItems.Count
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ProcessCheckbox(bool isChecked, UMC_EMAIL dataRow)
        {
            selectedItems = Session["SelectedItems"] as List<UMC_EMAIL> ?? new List<UMC_EMAIL>();
            if (isChecked)
            {
                selectedItems.Add(dataRow);
            }
            else
            {
                if(selectedItems.Count > 0)
                {
                    foreach(var item in selectedItems)
                    {
                        if(item.EMAIL == dataRow.EMAIL)
                        {
                            selectedItems.Remove(item);
                            break;
                        }
                    }
                }
            }
            if(selectedItems.Count > 0)
            {
                if(selectedItems.Where(w=>w.EMAIL == dataRow.EMAIL).Count()>1)
                {
                    selectedItems.Remove(dataRow);
                }
            }
            Session["SelectedItems"] = selectedItems;
            return Json(new { count = selectedItems.Count, data = selectedItems }, JsonRequestBehavior.AllowGet);
        }
        // Action để xóa danh sách khỏi Session
        [HttpPost]
        public JsonResult ClearSession()
        {
            selectedItems = Session["SelectedItems"] as List<UMC_EMAIL> ?? new List<UMC_EMAIL>();
            selectedItems.Clear();
            Session["SelectedItems"] = selectedItems;
            return Json(new { success = true });
        }
        public ActionResult OpenOutlook()
        {
            selectedItems = Session["SelectedItems"] as List<UMC_EMAIL> ?? new List<UMC_EMAIL>();
            var emailList = selectedItems.Select(s=>s.EMAIL).ToList();
            var email = string.Join("; ", emailList);
            string sendto = "mailto:" + email;
            return Redirect(sendto);
        }
        // Action để lấy dữ liệu
        public JsonResult GetData()
        {
            var data = Session["SelectedItems"] as List<UMC_EMAIL> ?? new List<UMC_EMAIL>();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteItem(string email)
        {
            try
            {
                selectedItems = Session["SelectedItems"] as List<UMC_EMAIL> ?? new List<UMC_EMAIL>();
                var data = selectedItems.Where(w => w.EMAIL == email).FirstOrDefault();
                if (data != null) { selectedItems.Remove(data); }

                Session["SelectedItems"] = selectedItems;
                return Json(new { success = true, message = "Xóa phần tử thành công." });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa phần tử: " + ex.Message });
            }
        }
    }
}