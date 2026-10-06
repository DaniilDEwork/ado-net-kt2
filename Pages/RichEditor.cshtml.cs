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
        if (!ModelState.IsValid)
        {
            return Page();
        }

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
                operations.ValueKind != JsonValueKind.Array ||
                !IsValidDocument(operations))
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
    private static bool IsValidDocument(JsonElement operations)
    {
        if (operations.GetArrayLength() == 0)
        {
            return false;
        }

        foreach (var operation in operations.EnumerateArray())
        {
            if (operation.ValueKind != JsonValueKind.Object ||
                !operation.TryGetProperty("insert", out var insert) ||
                operation.TryGetProperty("retain", out _) ||
                operation.TryGetProperty("delete", out _))
            {
                return false;
            }

            if (insert.ValueKind != JsonValueKind.String)
            {
                if (insert.ValueKind != JsonValueKind.Object ||
                    !insert.TryGetProperty("image", out var image) ||
                    image.ValueKind != JsonValueKind.String ||
                    string.IsNullOrWhiteSpace(image.GetString()) ||
                    insert.EnumerateObject().Count() != 1)
                {
                    return false;
                }
            }

            if (operation.TryGetProperty("attributes", out var attributes))
            {
                if (attributes.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                foreach (var attribute in attributes.EnumerateObject())
                {
                    if (attribute.Value.ValueKind != JsonValueKind.String &&
                        attribute.Value.ValueKind != JsonValueKind.Number &&
                        attribute.Value.ValueKind != JsonValueKind.True &&
                        attribute.Value.ValueKind != JsonValueKind.False)
                    {
                        return false;
                    }
                }
            }
        }

        var lastOperation = operations[operations.GetArrayLength() - 1];
        var lastInsert = lastOperation.GetProperty("insert");

        return lastInsert.ValueKind == JsonValueKind.String &&
            lastInsert.GetString()!.EndsWith('\n');
    }
}
