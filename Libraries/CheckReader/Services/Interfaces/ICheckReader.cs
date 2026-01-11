using CheckReader.Domain;

namespace CheckReader.Services
{
    public interface ICheckReader
    {
        public Task<Check> ReadCheckAsync(byte[] imageData);
    }
}