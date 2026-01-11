using MyMonkeys.Mobile.Models;
using MyMonkeys.Mobile.Services;

namespace MyMonkeys.Mobile.Pages;

public partial class MonkeyListPage : ContentPage
{
    private readonly MonkeysClient _client;

    public MonkeyListPage(MonkeysClient client)
    {
        InitializeComponent();
        _client = client;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var monkeys = await _client.GetMonkeysAsync();
            MonkeysView.ItemsSource = monkeys;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() is not Monkey monkey)
        {
            return;
        }

        MonkeysView.SelectedItem = null;
        await Shell.Current.GoToAsync($"monkey/{Uri.EscapeDataString(monkey.Id)}");
    }
}
