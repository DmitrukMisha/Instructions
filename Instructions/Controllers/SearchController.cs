using System.Collections.Generic;
using System.Linq;
using System.Text;
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



       
        public IActionResult Index(string TextSearchInput)
        {
            SearchViewModel model = new SearchViewModel
            {
                Success = true,
                Text = TextSearchInput
            };
            List<Record> records = dbContext.Records.ToList();
            AuthorDataView(records);
            GetTags(records);
            if (!string.IsNullOrEmpty(model.Text))
            {
                model.Records = dbContext.Records.FullTextSearchQuery(model.Text);
                model.Steps = dbContext.Steps.FullTextSearchQuery(model.Text);
                model.Tags = dbContext.Tags.FullTextSearchQuery(model.Text);
                model.Comments = dbContext.Comments.FullTextSearchQuery(model.Text);
                List<Record> recordsForComments = new List<Record>();
                foreach(Comment comment in model.Comments)
                {
                    recordsForComments.Add(dbContext.Records.Where(a => a.RecordID == comment.RecordID).FirstOrDefault());
                }
                ViewBag.Records = recordsForComments;
                if (model.Records==null & model.Tags==null & model.Steps==null) model.Success = false;
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

        public void GetTags(List<Record> records)
        {
            foreach (Record record in records)
            {
                var tags = dbContext.Tags.Where(a => a.Record == record).Select(p => p.TagName).ToList();
                var sb = new StringBuilder();
                tags.ForEach(s => sb.Append(s));
                var combinedList = sb.ToString();
                ViewData[record.RecordID.ToString()] = combinedList;
            }
        }

        public string GetAuthorName(Record record)
        {
            string Name = dbContext.Users.Where(a => a.Id == record.USerID).Select(p => p.UserName).SingleOrDefault();
            return Name;
        }

    }    
}