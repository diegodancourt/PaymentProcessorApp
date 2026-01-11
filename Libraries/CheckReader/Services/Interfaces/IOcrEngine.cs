namespace CheckReader.Services;

public interface IOcrEngine
{
    Task<string> ExtractTextAsync(byte[] imageData);
}