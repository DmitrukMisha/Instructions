using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Instructions.Data;
using Instructions.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Instructions.Areas.Identity.Pages.Account.Manage
{
    public class PersonalInstructionsModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<PersonalInstructionsModel> _logger;
        private readonly ApplicationDbContext _context;
        public PersonalInstructionsModel(ApplicationDbContext db,
            UserManager<User> userManager,
            ILogger<PersonalInstructionsModel> logger)
        {
            _context = db;
            _userManager = userManager;
            _logger = logger;
        }

        public List<Record> Records { get; set; }

    public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            Records =  _context.Records.Where(a => a.USerID == user.Id ).ToList();
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
           

            return Page();
        }
        public IActionResult createRec()
        {
            return Redirect("~/Controllers/Records");
        }


    }
}