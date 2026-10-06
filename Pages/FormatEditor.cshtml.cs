using System.ComponentModel.DataAnnotations;
using Kt2Editor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kt2Editor.Pages;

public class FormatEditorModel : PageModel
{
    private readonly TextStorage storage;

    public FormatEditorModel(TextStorage storage)
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
        EditorText = storage.Read("formatted.txt");
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        storage.Save("formatted.txt", EditorText ?? "");
        StatusMessage = "Текст с разметкой сохранён";

        return RedirectToPage();
    }
}