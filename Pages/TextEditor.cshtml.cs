using Kt2Editor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kt2Editor.Pages;

public class TextEditorModel : PageModel
{
    private readonly TextStorage storage;

    public TextEditorModel(TextStorage storage)
    {
        this.storage = storage;
    }

    [BindProperty]
    public string? EditorText { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public void OnGet()
    {
        EditorText = storage.Read("text.txt");
    }

    public IActionResult OnPost()
    {
        storage.Save("text.txt", EditorText ?? "");
        StatusMessage = "Текст сохранён на сервере";

        return RedirectToPage();
    }
}