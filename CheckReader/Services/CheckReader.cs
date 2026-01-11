using CheckReader.Domain;
using CheckReader.Parsers;
using Tesseract;

namespace CheckReader.Services
{
    public class CheckReader : ICheckReader
    {
        private readonly string _tessdataPath;

        public CheckReader(string? tessdataPath = null)
        {
            if (tessdataPath == null)
            {
                // Use the application's base directory to find tessdata
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                _tessdataPath = Path.Combine(baseDir, "tessdata");
            }
            else
            {
                _tessdataPath = tessdataPath;
            }
        }
        
        public async Task<Check> ReadCheckAsync(byte[] imageData)
        {
            using var engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default);
            // Optional: improve accuracy
            engine.SetVariable("tessedit_char_whitelist",
                "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz .,$/:|#-");

            using var img = Pix.LoadFromMemory(imageData);
            using var page = engine.Process(img);
            var fullText = page.GetText();
            Console.WriteLine("=== Extracted Text ===");
            Console.WriteLine(fullText);
            Console.WriteLine("======================\n");

            return new Check
            {
                RawText = fullText,
                Amount = new Amount(AmountParser.ExtractAmount(fullText)),
                Micr = new Micr(
                    MicrParser.ExtractRoutingNumber(fullText),
                    MicrParser.ExtractAccountNumber(fullText),
                    MicrParser.ExtractCheckNumber(fullText)
                ),
                Payee = new Payee(PayeeParser.ExtractPayee(fullText)),
                Date = DateParser.ExtractDate(fullText)
            };
        }
    }
}