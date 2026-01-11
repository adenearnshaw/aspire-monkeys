namespace MyMonkeys.WebBff;

internal sealed record MonkeyDto(
    string Id,
    string Name,
    string Location,
    string Details,
    string ImageUrl,
    int Population,
    double Latitude,
    double Longitude);