using MyMonkeys.Mobile.Models;
using MyMonkeys.Mobile.Services;

namespace MyMonkeys.Mobile.ViewModels;

public sealed class MonkeyDetailViewModel : ViewModelBase
{
    private readonly MonkeysClient _client;

    private string? _monkeyId;
    private Monkey? _monkey;
    private bool _isBusy;

    public MonkeyDetailViewModel(MonkeysClient client)
    {
        _client = client;
    }

    public string? MonkeyId
    {
        get => _monkeyId;
        set => SetProperty(ref _monkeyId, value);
    }

    public Monkey? Monkey
    {
        get => _monkey;
        private set
        {
            if (SetProperty(ref _monkey, value))
            {
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Location));
                OnPropertyChanged(nameof(Details));
                OnPropertyChanged(nameof(ImageUrl));
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string Title => Monkey?.Name ?? "Monkey";

    public string? Name => Monkey?.Name;
    public string? Location => Monkey?.Location;
    public string? Details => Monkey?.Details;
    public string? ImageUrl => Monkey?.ImageUrl;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(MonkeyId))
        {
            Monkey = null;
            return;
        }

        try
        {
            IsBusy = true;
            Monkey = await _client.GetMonkeyAsync(MonkeyId, cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }
}

