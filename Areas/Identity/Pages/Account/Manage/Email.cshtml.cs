using System.ComponentModel.DataAnnotations;
using ASPMMA.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASPMMA.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class EmailModel : PageModel
    {
        private readonly UserManager<Client> _userManager;
        private readonly SignInManager<Client> _signInManager;

        public EmailModel(UserManager<Client> userManager, SignInManager<Client> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string UserName { get; set; } = string.Empty;
        public string CurrentEmail { get; set; } = string.Empty;

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Въведи имейл адрес.")]
            [EmailAddress(ErrorMessage = "Въведи валиден имейл адрес.")]
            [Display(Name = "Нов имейл")]
            public string NewEmail { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            LoadUserData(user);
            Input.NewEmail = CurrentEmail;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            LoadUserData(user);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.Equals(CurrentEmail, Input.NewEmail, StringComparison.OrdinalIgnoreCase))
            {
                StatusMessage = "Имейл адресът е без промяна.";
                return RedirectToPage();
            }

            var result = await _userManager.SetEmailAsync(user, Input.NewEmail);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            StatusMessage = "Имейл адресът беше обновен успешно.";
            return RedirectToPage();
        }

        private void LoadUserData(Client user)
        {
            UserName = user.UserName ?? string.Empty;
            CurrentEmail = user.Email ?? string.Empty;
        }
    }
}
