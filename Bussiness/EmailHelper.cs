using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using UMC_Email.Models;
using static System.Net.WebRequestMethods;

namespace UMC_Email.Bussiness
{
    public class EmailHelper
    {
        public static void Upload(DataTable datatable)
        {
            using (var context = new DBContext())
            {
                for (int index =1; index < datatable.Rows.Count; index++)
                {
                    DataRow row = datatable.Rows[index];
                    string code = row[0].ToString().Trim();
                    string name = row[1].ToString().Trim();
                    string dept = row[2].ToString().Trim();
                    string email = row[3].ToString().Trim();
                    var data = context.UMC_EMAIL.Where(w => w.EMAIL == email).FirstOrDefault();
                    if (data == null)
                    {
                        UMC_EMAIL uMC_EMAIL = new UMC_EMAIL();
                        uMC_EMAIL.CODE = code;
                        uMC_EMAIL.NAME = name;
                        uMC_EMAIL.DEPARTMENT = dept;
                        uMC_EMAIL.EMAIL = email;
                        context.UMC_EMAIL.Add(uMC_EMAIL);
                        context.SaveChanges();
                    }
                }   
            }
        }

        public static async Task AvalidEmail(string email)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://www.infobyip.com/verifyemailaccount.php");

                var jsonData = new
                {
                    email = email
                };

                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonData);
                StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/verifyemailaccount.php", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Response: " + responseContent);
                }
                else
                {
                    Console.WriteLine("Error: " + response.StatusCode);
                }
            }
        }
    }
    
}