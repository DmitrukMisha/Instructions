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

namespace Instructions.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext DbContext;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IStringLocalizer<HomeController> _localizer;

        public HomeController(IStringLocalizer<HomeController> localizer, UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context)
        {

            _userManager = userManager;
            _signInManager = signInManager;
            DbContext = context;
            _localizer = localizer;

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


        public IActionResult UserPage(string id)
        {
            ViewData["Name"] = GetUserById(id);
           
            return View(GetRecords(GetUserById(id)));
        }

        public IActionResult AddTheme()
        {

            return View(DbContext.Themes.ToList());
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
