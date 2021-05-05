namespace Combinations
{
    public interface IValueParser<out T>
    {
        T Parse(string text);
    }
}
