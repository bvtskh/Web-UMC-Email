using ExcelDataReader;
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
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        { 
            return View();
        }
        [HttpPost]
        [Obsolete]
        public ActionResult Upload(HttpPostedFileBase excelFile)
        {
            try
            {
                if (excelFile != null && excelFile.ContentLength > 0 && Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    // Tạo đường dẫn lưu tệp tạm thời
                    var filePath = Path.Combine(Server.MapPath("~/App_Data/Uploads"), Path.GetFileName(excelFile.FileName));

                    // Tạo thư mục nếu chưa tồn tại
                    Directory.CreateDirectory(Server.MapPath("~/App_Data/Uploads"));

                    // Lưu tệp lên server
                    excelFile.SaveAs(filePath);

                    // Đọc dữ liệu từ tệp Excel
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet();
                            var table = result.Tables[0]; // Đọc bảng đầu tiên
                            EmailHelper.Upload(table);
                        }
                        return Json(new { success = true, message = "Upload success full!" });
                    }
                }
                return Json(new { success = false, message = "Please select file!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error upload!" + ex.Message });
            }
            
        }
        public ActionResult Download()
        {
            string filePath = Server.MapPath("~/App_Data/Uploads/Umc_email.xlsx");
            string fileName = "Umc_email.xlsx";
            return File(filePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        public ActionResult Search(string searchTerm)
        {
            using (var context = new DBContext())
            {
                var query = context.UMC_EMAIL.AsEnumerable();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p => NormalString.NormalizeString(p.DEPARTMENT).Contains(NormalString.NormalizeString(searchTerm)) || NormalString.NormalizeString(p.NAME).Contains(NormalString.NormalizeString(searchTerm)) || NormalString.NormalizeString(p.EMAIL).Contains(NormalString.NormalizeString(searchTerm))).ToList();
                }
                List<UMC_EMAIL> selectedItems = Session["SelectedItems"] as List<UMC_EMAIL> ?? new List<UMC_EMAIL>();
                var result = new
                {
                    data = query.OrderBy(o=>o.DEPARTMENT).Take(10).ToList(),
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                using(var context = new DBContext())    
                {
                    var data = context.UMC_EMAIL.Where(w => w.ID == id).FirstOrDefault();
                    context.UMC_EMAIL.Remove(data);
                    context.SaveChanges();
                }
                return Json(new { success = true, message = "Delete email succsess!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error while delete!" + ex.Message });
            }
        }
        [HttpPost]
        public ActionResult Edit(string dept, string name, string email, int id)
        {
            try
            {
                using(var context = new DBContext())
                {
                    var data = context.UMC_EMAIL.Where(w => w.ID == id).FirstOrDefault();
                    if(data != null)
                    {
                        data.DEPARTMENT = dept;
                        data.NAME = name;
                        data.EMAIL = email;
                        context.Entry(data).State= System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                }
                return Json(new { success = true, message = "Update email succsess!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error while update!" + ex.Message });
            }
        }
    }
}