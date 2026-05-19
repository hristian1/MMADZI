using System.Text;
using System.Text.Json;
using ASPMMA.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASPMMA.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<Client> _userManager;

        public PersonalDataModel(UserManager<Client> userManager)
        {
            _userManager = userManager;
        }

        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IReadOnlyList<ProfileRow> ProfileRows { get; set; } = [];

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            LoadUserData(user);
            return Page();
        }

        public async Task<IActionResult> OnPostDownloadAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var data = BuildPersonalData(user)
                .ToDictionary(row => row.Label, row => row.Value);

            data["Експортирано на"] = DateTimeOffset.UtcNow.ToString("u");

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return File(Encoding.UTF8.GetBytes(json), "application/json", "mma-arena-personal-data.json");
        }

        private void LoadUserData(Client user)
        {
            UserName = user.UserName ?? string.Empty;
            FullName = user.FullName;
            Email = user.Email ?? string.Empty;
            ProfileRows = BuildPersonalData(user);
        }

        private static IReadOnlyList<ProfileRow> BuildPersonalData(Client user)
        {
            return
            [
                new("Потребителско име", user.UserName ?? string.Empty),
                new("Име", user.FirstName),
                new("Фамилия", user.LastName),
                new("Имейл", user.Email ?? string.Empty),
                new("Телефон", user.PhoneNumber ?? string.Empty),
                new("Имейл потвърден", user.EmailConfirmed ? "Да" : "Не"),
                new("Телефон потвърден", user.PhoneNumberConfirmed ? "Да" : "Не"),
                new("Двуфакторна защита", user.TwoFactorEnabled ? "Включена" : "Изключена"),
                new("Профил ID", user.Id)
            ];
        }

        public sealed record ProfileRow(string Label, string Value);
    }
}
