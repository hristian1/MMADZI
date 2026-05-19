using ASPMMA.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASPMMA.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class TwoFactorAuthenticationModel : PageModel
    {
        private readonly UserManager<Client> _userManager;
        private readonly SignInManager<Client> _signInManager;

        public TwoFactorAuthenticationModel(UserManager<Client> userManager, SignInManager<Client> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string UserName { get; set; } = string.Empty;
        public bool IsTwoFactorEnabled { get; set; }
        public int RecoveryCodesLeft { get; set; }
        public IReadOnlyList<string> Providers { get; set; } = [];

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostEnableAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
            if (!providers.Any())
            {
                ErrorMessage = "Профилът няма активен метод за двуфакторна защита. Потвърди имейл адреса си и опитай отново.";
                return RedirectToPage();
            }

            var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
            if (!result.Succeeded)
            {
                ErrorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Двуфакторната защита беше включена.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDisableAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!result.Succeeded)
            {
                ErrorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Двуфакторната защита беше изключена.";
            return RedirectToPage();
        }

        private async Task LoadAsync(Client user)
        {
            UserName = user.UserName ?? string.Empty;
            IsTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);
            Providers = (await _userManager.GetValidTwoFactorProvidersAsync(user)).ToList();
        }
    }
}
