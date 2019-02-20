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

        public async Task<IActionResult> AdminMenu()
        {
          
                return Page();
            
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
            else return RedirectToAction("Index");
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

            return RedirectToAction("Index");
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
            return RedirectToAction("Index");
        }

    }
}