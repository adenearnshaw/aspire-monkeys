namespace MyMonkeys.Mobile.Services;

public static class ApiBaseUrl
{
    // Mobile BFF endpoint.
    // Android emulator uses 10.0.2.2 to reach host machine localhost.
    public static Uri Get()
    {
#if ANDROID
        return new Uri("http://10.0.2.2:5052");
#else
        return new Uri("http://localhost:5052");
#endif
    }
}
