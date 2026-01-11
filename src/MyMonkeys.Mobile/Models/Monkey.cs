namespace MyMonkeys.Mobile.Models;

public sealed class Monkey
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Location { get; init; }
    public required string Details { get; init; }
    public required string ImageUrl { get; init; }

    public int Population { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}
