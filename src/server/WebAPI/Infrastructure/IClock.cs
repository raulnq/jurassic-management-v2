namespace Infrastructure
{
    public interface IClock
    {
        DateTimeOffset Now { get; }
    }
}