using CheckReader.Domain;
using CheckReader.Parsers;

namespace CheckReader.Services;

public class CheckReader : ICheckReader
{
    private readonly IOcrEngine _ocrEngine;

    public CheckReader(IOcrEngine? ocrEngine = null)
    {
        _ocrEngine = ocrEngine ?? new TesseractOcrEngine();
    }

    public async Task<Check> ReadCheckAsync(byte[] imageData)
    {
        var fullText = await _ocrEngine.ExtractTextAsync(imageData);

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