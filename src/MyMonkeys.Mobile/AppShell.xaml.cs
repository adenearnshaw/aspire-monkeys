namespace MyMonkeys.Mobile;

using MyMonkeys.Mobile.Pages;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("monkey/{id}", typeof(MonkeyDetailPage));
	}
}
