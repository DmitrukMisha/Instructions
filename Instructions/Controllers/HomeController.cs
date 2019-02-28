using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Instructions.Models;
using Instructions.Data;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace Instructions.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext DbContext;
        private readonly UserManager<User> _userManager;
        public HomeController(ApplicationDbContext context, UserManager<User> userManager)
        {

            _userManager = userManager;
            DbContext = context;

        }
        public IActionResult Index()
        {
            List<Record> records = DbContext.Records.ToList();
            records.Reverse();
            GetTags(records);
            AuthorDataView(records);
            return View(records);
        }
        public IActionResult Record(string id)
        {
            GetRecordData(id);
            return View(GetSteps(GetRecord(id)));
        }

        public void GetRecordData(string id)
        {
            Record record = GetRecord(id);
            ViewData["Name"] = record.Name;
            ViewData["Theme"] = record.ThemeName;
            ViewData["Author"] = GetAuthorName(record);
        }

        public Record GetRecord(string id)
        {
            int idNumeric = Convert.ToInt32(id);
            return DbContext.Records.Where(a => a.RecordID == idNumeric).SingleOrDefault();
        }
        public List<Step> GetSteps(Record record)
        {
             return DbContext.Steps.Where(a => a.RecordID == record).ToList();
        }

        public void GetTags(List<Record> records)
        {
            foreach (Record record in records)
            {
                var tags = DbContext.Tags.Where(a => a.Record == record).Select(p=>p.TagName).ToList() ;
                var sb = new StringBuilder();
                tags.ForEach(s => sb.Append(s));
                var combinedList = sb.ToString();
                ViewData[record.RecordID.ToString()] = combinedList;
            }
        }
        
        public string GetAuthorName(Record record)
        {
            string Name = DbContext.Users.Where(a => a.Id == record.USerID).Select(p => p.UserName).SingleOrDefault();
            return Name;
        }
        
        public void AuthorDataView(List<Record> records)
        {
            foreach (Record record in records)
            {
                ViewData["author" + record.RecordID.ToString()] = GetAuthorName(record);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
