using System.ComponentModel.DataAnnotations;
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
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string? EditorText { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public void OnGet()
    {
        EditorText = storage.Read("text.txt");
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        storage.Save("text.txt", EditorText ?? "");
        StatusMessage = "Текст сохранён на сервере";

        return RedirectToPage();
    }
}