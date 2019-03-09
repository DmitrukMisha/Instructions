using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Instructions.Models;
using Instructions.Data;

using System.Text;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;
using System.IO;

namespace Instructions.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext DbContext;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IStringLocalizer<HomeController> _localizer;
        static User user;
        static Record record;
        const int CountCloudTags = 10;

        public HomeController(IStringLocalizer<HomeController> localizer, UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            DbContext = context;
            _localizer = localizer;
        }

        public IActionResult Index()
        {
            ViewBag.TagsNames = GenerateTagsCloudValues();
            List<Record> records = DbContext.Records.ToList();
            user = _userManager.GetUserAsync(User).Result;
            records.Reverse();
            GetTags(records);
            AuthorDataView(records);
            return View(records);
        }
        public List<string> GenerateTagsCloudValues()
        {
            List<string> TagsCloudValues = new List<string>();
            IEnumerable<string> Tags = DbContext.Tags.Select(t => t.TagName).ToList().Distinct();
            Random random = new Random();
            int RandomValue;
            int count;
            if (Tags.Count() < CountCloudTags)
            {
             count = Tags.Count();
            } else { count = CountCloudTags; }
                for (int i = 0; i < count; i++)
                {
                    RandomValue = random.Next(Tags.Count());
                         if (!TagsCloudValues.Contains(Tags.ElementAt(RandomValue)))
                            {
                                 TagsCloudValues.Add(Tags.ElementAt(RandomValue));
                            }
                          else { count++; }
            }
            return TagsCloudValues;
        }
        public IActionResult Record(string id)
        {
            ViewData["EmailConfirmed"] = false;
            ViewData["readonly"] = "true";
            if (user != null)
            {
                ViewData["EmailConfirmed"] = user.EmailConfirmed;
                ViewData["Role"] = user.RoleISAdmin;
                ViewData["readonly"] = "false";
            }
            ViewData["RecordID"] = Convert.ToInt32(id);
            ViewData["RatingValue"] = GetRating(id);
            record = GetRecord(id);
            GetRecordData(id);
            return View(GetSteps(GetRecord(id)));
        }


        public IActionResult UserPage(string id)
        {
            ViewData["Name"] = GetUserById(id);
           
            return View(GetRecords(GetUserById(id)));
        }

        public IActionResult AddTheme()
        {

            return View(DbContext.Themes.ToList());
        }
        public string GetRating(string id)
        {
            List<Mark> marks = DbContext.Marks.Where(a => a.RecordID.RecordID == Int32.Parse(id)).ToList();
            if (marks.Count() != 0)
            {
                double value = 0;
                foreach (Mark mark in marks)
                {
                    value += mark.MarkValue;
                }
                value = value / marks.Count();
                return (Math.Round(value * 2, MidpointRounding.AwayFromZero) / 2).ToString().Replace(",", ".");
            }
            else { return "0"; }
        }

    

        public void GetRecordData(string id)
        {
            Record record = GetRecord(id);
            ViewData["Name"] = record.Name;
            ViewData["Theme"] = record.ThemeName;
            ViewData["Author"] = GetAuthorName(record);
        }

        public User GetUserById(string id)
        {
            return DbContext.Users.Where(a => a.Id == id).SingleOrDefault();
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

        public List<Record> GetRecords(User user)
        {
            return DbContext.Records.Where(a => a.USerID == user.Id).ToList();
        }

        public void GetTags(List<Record> records)
        {
            foreach (Record record in records)
            {
                var tags = DbContext.Tags.Where(a => a.Record == record).Select(p => p.TagName).ToList();
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

        public IActionResult Comments()
        {
            List<Comment> comments = DbContext.Comments.Where(a => a.RecordID == record.RecordID).ToList();
            ViewData["EmailConfirmed"] = false;
            if (user != null)
            {
                ViewData["EmailConfirmed"] = user.EmailConfirmed;
                ViewData["Role"] = user.RoleISAdmin;
                ViewBag.Likes = GetLikes(comments);
            }

            ViewBag.LikesCount = LikesCount(comments);
            return PartialView(comments);
        }

        public List<bool> GetLikes(List<Comment> comments)
        {
            List<bool> LikesIsSet = new List<bool>();
            foreach (Comment comment in comments)
            {
                if (DbContext.Likes.Where(a => a.CommentID == comment && a.UserID == user).FirstOrDefault() == null)
                    LikesIsSet.Add(false);
                else LikesIsSet.Add(true);
            }
            return LikesIsSet;
        }

        public List<int> LikesCount(List<Comment> comments)
        {
            List<int> LikesCount = new List<int>();
            foreach (Comment comment in comments)
            {
                LikesCount.Add(DbContext.Likes.Where(a => a.CommentID == comment).Count());

            }
            return LikesCount;
        }
        [HttpPost]
        public async Task<IActionResult> CreateLike(int id)
        {
            Comment comment = DbContext.Comments.Where(a => a.CommentID == id).FirstOrDefault();
            User user = _userManager.GetUserAsync(User).Result;
            Like like = new Like { CommentID = comment, UserID = user };
            await DbContext.Likes.AddAsync(like);
            await DbContext.SaveChangesAsync();
            return RedirectToAction("Comments");
        }
        [HttpPost]
        public async Task<IActionResult> RemoveLike(int id)
        {
            Comment comment = DbContext.Comments.Where(a => a.CommentID == id).FirstOrDefault();
            User user = _userManager.GetUserAsync(User).Result;
            Like like = DbContext.Likes.Where(a => a.CommentID == comment && a.UserID == user).FirstOrDefault();
            DbContext.Likes.Remove(like);
            await DbContext.SaveChangesAsync();
            return RedirectToAction("Comments");
        }

        [HttpPost]
        public async Task<IActionResult> AddMark(string value, int id)
        {
            
            double doubleValue = Convert.ToDouble(value.Replace(".", ","));
            Record record = DbContext.Records.Where(a => a.RecordID == id).FirstOrDefault();
            User user = _userManager.GetUserAsync(User).Result;
            Mark mark = DbContext.Marks.Where(a => a.RecordID.RecordID == id && a.UserID.Id == user.Id).SingleOrDefault();
            if (mark == null)
            {
                mark = new Mark { RecordID = record, UserID = user, MarkValue = doubleValue };
                await DbContext.Marks.AddAsync(mark);
            }
            else
            {
                mark.MarkValue = doubleValue;
                DbContext.Marks.Update(mark);
            }
            await DbContext.SaveChangesAsync();

            return Json(GetRating(id.ToString())) ;
        }
        public async void DeleteLikes(Comment comment)
        {
            List<Like> likes = DbContext.Likes.Where(a => a.CommentID == comment).ToList();
            foreach(Like like in likes)
            {
                DbContext.Likes.Remove(like);
            }
            await DbContext.SaveChangesAsync();
        }

         public async void DeleteComments(Record record)
        {
            List<Comment> comments = DbContext.Comments.Where(a => a.RecordID == record.RecordID).ToList();
            foreach(Comment comment in comments)
            {
                DeleteLikes(comment);
                DbContext.Comments.Remove(comment);
            }
            
            await DbContext.SaveChangesAsync();
        }

        public async void DeleteTags(Record record)
        {
            List<Tag> tags = DbContext.Tags.Where(a => a.Record == record).ToList();
            foreach (Tag tag in tags)
            {
                DbContext.Tags.Remove(tag);
            }

            await DbContext.SaveChangesAsync();
        }

        [HttpPost]
        public async Task<ActionResult> DeleteRecords(string[] selected)
        {
            if (selected != null)
            {
                foreach (var id in selected)
                {
                    var record = await DbContext.Records.FindAsync(Int32.Parse(id));


                    if (record != null)
                    {
                        List<Step> steps = DbContext.Steps.Where(a => a.RecordID == record).ToList();
                        foreach (var step in steps)
                        {
                            DbContext.Steps.Remove(step);
                        }
                        DeleteComments(record);
                        DeleteTags(record);
                        DeleteMarks(record);
                        DbContext.Records.Remove(record);
                        await DbContext.SaveChangesAsync();
                        
                    }
                }
            }
            return Redirect("~/Identity/Account/Manage/PersonalInstructions");
        }

        public async void DeleteMarks(Record record)
        {
            List<Mark> marks = DbContext.Marks.Where(a => a.RecordID == record).ToList();
            foreach(Mark mark in marks)
            {
                DbContext.Marks.Remove(mark);
            }
            await DbContext.SaveChangesAsync();
        }
        [HttpPost]
        public async Task<IActionResult> CreateComment(string Text)
        {
            Comment comment = new Comment
            {
                Text = Text,
                RecordID = record.RecordID,

            };
            User user = await _userManager.GetUserAsync(User);
            comment.UserID = user.Id;
            comment.UserName = user.UserName;
            DbContext.Comments.Add(comment);
            await DbContext.SaveChangesAsync();
            return RedirectToAction("Comments");

        }
        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
        {
            Comment comment = DbContext.Comments.Where(a => a.CommentID == id).FirstOrDefault();
            DeleteLikes(comment);
            DbContext.Comments.Remove(comment);
             await DbContext.SaveChangesAsync();
            return RedirectToAction("Comments");
        }
        public async Task<IActionResult> EditComment(int id, string Text)
        {
            Comment comment = DbContext.Comments.Where(a => a.CommentID == id).FirstOrDefault();
            comment.Text = Text.Remove(0, 12);
            DbContext.Update(comment);
            await DbContext.SaveChangesAsync();
            return RedirectToAction("Comments");
        }

        public FileResult CreateFile()
        {
            PdfDocument document = new PdfDocument();
            PdfPage page = document.Pages.Add();
            PdfGraphics graphics = page.Graphics;   
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 20);
            //graphics.DrawString("Hello World!!!", font, PdfBrushes.Black, new PointF(0, 0));
            graphics.DrawString(record.Name, font, PdfBrushes.Black, new PointF(0, 0));
          //  graphics.DrawString(record.Description, font, PdfBrushes.Black, new PointF(0, 20));
            MemoryStream stream = new MemoryStream();
            document.Save(stream);
            stream.Position = 0;
            FileStreamResult fileStreamResult = new FileStreamResult(stream, "application/pdf");
            fileStreamResult.FileDownloadName = record.Name.Replace(" ","_")+".pdf";
            return fileStreamResult;
        }
        public async Task<IActionResult> Enter(string returnUrl)
        {
            var unlocked = IsLocked(User.Identity.Name);
            unlocked.Wait();
            if (unlocked.Result)
            {
                return RedirectToAction("SetLanguage", "Home", new { culture = "", returnUrl });
            }
            else
                await _signInManager.SignOutAsync();
            return Redirect("~/Identity/Account/Lockout");
        }

        private async Task<bool> IsLocked(string name)
        {
            User user = await _userManager.FindByNameAsync(name);
            if (user.Status)
            {
                return true;
            }
            return false;
        }

        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (_signInManager.IsSignedIn(User))
            {
                if (culture == null)
                {
                    culture = UserLogin();
                }
                else
                {
                    var task = ChangeCulture(culture);
                    task.Wait();
                }
            }
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        public string UserLogin()
        {
            var getCulture = GetUserCulture();
            getCulture.Wait();
            string culture = getCulture.Result;
            var AddStyle = AddUserStyleToCookie();
            AddStyle.Wait();
            return culture;
        }

        public async Task<User> ChangeCulture(string culture)
        {
            var user = await _userManager.GetUserAsync(User);
            user.Language = culture;
            await _userManager.UpdateAsync(user);
            return user;
        }

        public async Task<User> GetUser()
        {
            var user = await _userManager.GetUserAsync(User);
            return user;
        }

        public async Task<User> AddUserStyleToCookie()
        {
            User user = await GetUser();
            AddStyleToCookie(user.Color);
            return user;
        }

        public void AddStyleToCookie(string style)
        {
            Response.Cookies.Append("style", style);
        }

        public async Task<string> GetUserCulture()
        {
            var user = await _userManager.GetUserAsync(User);
            return user.Language;
        }

        [HttpPost]
        public async Task<IActionResult> ChangeColor(string returnUrl)
        {
            User user = await GetUser();
            string bootstrapDarkly = "bootstrap-darkly.css";
            string bootstrapDefault = "bootstrap-default.css";
            var CookiesValue = Request.Cookies["style"];
            if (CookiesValue == bootstrapDefault)
            {
                AddStyleToCookie(bootstrapDarkly);
                CookiesValue = bootstrapDarkly;
            }
            else
            {
                AddStyleToCookie(bootstrapDefault);
                CookiesValue = bootstrapDefault;
            }

            if (_signInManager.IsSignedIn(User))
            {
                user.Color = CookiesValue;
                await _userManager.UpdateAsync(user);
            }
            return LocalRedirect(returnUrl);
        }

        public async Task UpdateUserStyle()
        {
            if (_signInManager.IsSignedIn(User))
            {
                User user = await GetUser();
                var CookiesValue = Request.Cookies["style"];
                user.Color = CookiesValue;
                await _userManager.UpdateAsync(user);
            }
        }


        [HttpPost]
        public async Task<ActionResult> Delete(string[] selected)
        {
            bool IsI = false;
            if (selected != null)
            {
                foreach (var id in selected)
                {
                    User user = await _userManager.FindByIdAsync(id);
                    if (user != null)
                    {
                        if (User.Identity.Name == user.Email) IsI = true;
                        IdentityResult result = await _userManager.DeleteAsync(user);
                    }
                }
            }
            if (IsI)
            {
                await _signInManager.SignOutAsync();
                return Redirect("~/Identity/Account/Logout");

            }
            else return Redirect("~/Identity/Account/Manage/AdminMenu");
        }
        [HttpPost]
        public async Task<ActionResult> MakeAdmin(string[] selected)
        {

            if (selected != null)
            {
                foreach (var id in selected)
                {
                    User user = await _userManager.FindByIdAsync(id);
                    if (user != null)
                    {
                        if (user.RoleISAdmin == true)
                        {
                            user.RoleISAdmin = false;
                        }
                        else { user.RoleISAdmin = true; }
                        await _userManager.UpdateAsync(user);
                    }
                }
            }

            return Redirect("~/Identity/Account/Manage/AdminMenu");
        }
       
        [HttpPost]
        public async Task<ActionResult> Lock(string[] selected)
        {

            if (selected != null)
            {
                foreach (var id in selected)
                {
                    User user = await _userManager.FindByIdAsync(id);
                    if (user != null)
                    {

                        user.Status = false;
                        await _userManager.UpdateAsync(user);
                    }
                }
            }

            return Redirect("~/Identity/Account/Manage/AdminMenu");
        }


        [HttpPost]
        public async Task<ActionResult> Unlock(string[] selected)
        {
            if (selected != null)
            {
                foreach (var id in selected)
                {
                    User user = await _userManager.FindByIdAsync(id);
                    if (user != null)
                    {
                        user.Status = true;
                        await _userManager.UpdateAsync(user);
                    }
                }
            }
            return Redirect("~/Identity/Account/Manage/AdminMenu");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
