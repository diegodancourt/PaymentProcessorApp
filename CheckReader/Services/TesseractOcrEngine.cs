using Tesseract;

namespace CheckReader.Services;

public class TesseractOcrEngine : IOcrEngine
{
    private readonly string _tessdataPath;

    public TesseractOcrEngine(string? tessdataPath = null)
    {
        if (tessdataPath == null)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _tessdataPath = Path.Combine(baseDir, "tessdata");
        }
        else
        {
            _tessdataPath = tessdataPath;
        }
    }

    public Task<string> ExtractTextAsync(byte[] imageData)
    {
        using var engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default);
        engine.SetVariable("tessedit_char_whitelist",
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz .,$/:|#-");

        using var img = Pix.LoadFromMemory(imageData);
        using var page = engine.Process(img);
        var text = page.GetText();

        return Task.FromResult(text);
    }
}