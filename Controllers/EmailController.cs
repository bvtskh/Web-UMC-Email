using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.Data;
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
            var mailList = context.UMC_EMAIL.ToList();
            return View(mailList);                    
        }
        public JsonResult Search(string searchTerm = "", int page = 1, int pageSize = 10)
        {

            //var result = context.UMC_EMAIL.Where(p => p.DEPARTMENT.Contains(searchTerm) || p.NAME.Contains(searchTerm) || p.EMAIL.Contains(searchTerm)).ToList();
            //return Json(result, JsonRequestBehavior.AllowGet);

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
                            }).ToList();
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
            Session["YourDataSessionKey"] = selectedItems;
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
    }
}