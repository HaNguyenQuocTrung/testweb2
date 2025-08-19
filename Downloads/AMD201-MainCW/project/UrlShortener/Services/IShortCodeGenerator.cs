namespace UrlShortener.Services
{
    public interface IShortCodeGenerator
    {
        string Generate(int length = 8);
    }
}
