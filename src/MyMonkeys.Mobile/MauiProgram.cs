using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using MyMonkeys.Mobile.Services;
using MyMonkeys.Mobile.ViewModels;

namespace MyMonkeys.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		//builder.AddServiceDefaults();

		builder.Services.AddSingleton(sp =>
		{
			var http = new HttpClient
			{
				BaseAddress =  new Uri("https+http://mobile-bff"),
				Timeout = TimeSpan.FromSeconds(15)
			};
			return http;
		});
		builder.Services.AddSingleton<MonkeysClient>();
		builder.Services.AddTransient<MonkeyDetailViewModel>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
