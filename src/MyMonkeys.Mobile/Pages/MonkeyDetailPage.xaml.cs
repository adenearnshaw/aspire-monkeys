using MyMonkeys.Mobile.ViewModels;

namespace MyMonkeys.Mobile.Pages;

[QueryProperty(nameof(MonkeyId), "id")]
public partial class MonkeyDetailPage
{
    private readonly MonkeyDetailViewModel _vm;

    public MonkeyDetailPage(MonkeyDetailViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    public string? MonkeyId { get; set; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _vm.MonkeyId = MonkeyId;

        try
        {
            await _vm.LoadAsync();

            if (_vm.Monkey is null)
            {
                await DisplayAlertAsync("Not found", "Monkey not found.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }
}
