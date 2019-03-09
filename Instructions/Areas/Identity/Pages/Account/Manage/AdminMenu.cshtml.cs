using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Instructions.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Instructions.Areas.Identity.Pages.Account.Manage
{
    public class AdminMenuModel : PageModel
    {

        UserManager<User> _userManager;
        SignInManager<User> _signInManager;


        public AdminMenuModel(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;

        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["EmailConfirm"] = user.EmailConfirmed;
            return Page();

        }



    }
}