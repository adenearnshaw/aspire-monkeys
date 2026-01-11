namespace MyMonkeys.MobileBff;

internal sealed record MonkeyDto(
    string Id,
    string Name,
    string Location,
    string Details,
    string ImageUrl);