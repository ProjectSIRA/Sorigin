namespace Sorigin.Settings;

public interface IMaxMindSettings
{
    int ID { get; }
    string Key { get; }
}

internal class MaxMindSettings : IMaxMindSettings
{
    public int ID { get; set; }
    public string Key { get; init; } = null!;
}