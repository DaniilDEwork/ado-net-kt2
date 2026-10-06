using System.Text;

namespace Kt2Editor.Services;

public class TextStorage
{
    private readonly string folder;

    public TextStorage(IWebHostEnvironment environment)
    {
        folder = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(folder);
    }

    public string Read(string fileName)
    {
        string path = Path.Combine(folder, fileName);

        if (!File.Exists(path))
        {
            return "";
        }

        return File.ReadAllText(path, Encoding.UTF8);
    }

    public void Save(string fileName, string text)
    {
        string path = Path.Combine(folder, fileName);
        File.WriteAllText(path, text, Encoding.UTF8);
    }
}