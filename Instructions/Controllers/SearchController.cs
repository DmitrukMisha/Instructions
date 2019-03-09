using System.Collections.Generic;
using System.Linq;
using Instructions.Data;
using Instructions.Models;
using Korzh.EasyQuery.Linq;
using Microsoft.AspNetCore.Mvc;



namespace Instructions.Controllers
{

     public class SearchController : Controller
      {

           
        private ApplicationDbContext dbContext;
        public SearchController(ApplicationDbContext context)
            {
              dbContext = context;
        }

        //[HttpGet]
        //    public IActionResult Index(string TextSearchInput)
        //    {
        //    List<Record> records = dbContext.Records.ToList();
        //    AuthorDataView(records);
        //    var model = new SearchViewModel
        //    {
        //        Records = dbContext.Records
        //    };
        //        return View(model);
        //    }

        [HttpPost]
        public IActionResult Index(string TextSearchInput)
        {
            SearchViewModel model = new SearchViewModel
            {
                Success = true,
                Text = TextSearchInput
            };
            //  model.Records = dbContext.Records;
            // model.Users = dbContext.Users;
            // model.Steps = dbContext.Steps;
            List<Record> records = dbContext.Records.ToList();
            AuthorDataView(records);

            if (!string.IsNullOrEmpty(model.Text))
            {
                model.Records = dbContext.Records.FullTextSearchQuery(model.Text);
                model.Users= dbContext.Users.FullTextSearchQuery(model.Text);
                model.Steps = dbContext.Steps.FullTextSearchQuery(model.Text);

                if(model.Records==null & model.Users==null & model.Steps==null) model.Success = false;
            }else
            {
                model.Success = false;
            }
                return View(model);
        }


        public void AuthorDataView(List<Record> records)
        {
            foreach (Record record in records)
            {
                ViewData["author" + record.RecordID.ToString()] = GetAuthorName(record);
            }
        }

        public string GetAuthorName(Record record)
        {
            string Name = dbContext.Users.Where(a => a.Id == record.USerID).Select(p => p.UserName).SingleOrDefault();
            return Name;
        }

    }    
}