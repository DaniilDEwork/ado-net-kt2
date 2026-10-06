using System.Text.Json;
using Kt2Editor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kt2Editor.Pages;

public class RichEditorModel : PageModel
{
    private readonly TextStorage storage;

    public RichEditorModel(TextStorage storage)
    {
        this.storage = storage;
    }

    [BindProperty]
    public string? EditorText { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public void OnGet()
    {
        EditorText = storage.Read("rich.json");
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(EditorText))
        {
            ModelState.AddModelError("", "Не получены данные редактора");
            return Page();
        }

        if (EditorText.Length > 1_000_000)
        {
            ModelState.AddModelError(
                "",
                "Документ слишком большой. Уменьшите число изображений"
            );

            return Page();
        }

        try
        {
            using var document = JsonDocument.Parse(EditorText);

            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty("ops", out var operations) ||
                operations.ValueKind != JsonValueKind.Array)
            {
                ModelState.AddModelError("", "Неверный формат документа");
                return Page();
            }
        }
        catch (JsonException)
        {
            ModelState.AddModelError("", "Ошибка чтения JSON");
            return Page();
        }

        storage.Save("rich.json", EditorText);
        StatusMessage = "Документ и форматирование сохранены";

        return RedirectToPage();
    }
}