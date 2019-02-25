using System.Threading.Tasks;
using Instructions.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Instructions.Areas.Identity.Pages.Account.Manage
{
    public class PersonalInstructionsModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<PersonalInstructionsModel> _logger;

        public PersonalInstructionsModel(
            UserManager<User> userManager,
            ILogger<PersonalInstructionsModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
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