using System.Globalization;
using Blazored.LocalStorage;

namespace SellerInventory.Client.BlazorWeb.Services;

public enum Language
{
    ENG,
    VIE
}

public interface ILanguageService
{
    Language CurrentLanguage { get; }
    CultureInfo CurrentCulture { get; }
    event Action? OnLanguageChanged;

    Task InitializeAsync();
    Task SetLanguageAsync(Language language);

    string FormatCurrency(decimal amount);
    string FormatDate(DateTime date);
    string FormatDateTime(DateTime dateTime);
    string FormatShortDate(DateTime date);
    string FormatNumber(decimal number);
    string FormatNumber(int number);
}

public class LanguageService : ILanguageService
{
    private readonly ILocalStorageService _localStorage;
    private const string LanguageKey = "app_language";

    private Language _currentLanguage = Language.ENG;
    private CultureInfo _currentCulture = new("en-US");

    private static readonly Dictionary<Language, CultureInfo> CultureMap = new()
    {
        { Language.ENG, new CultureInfo("en-US") },
        { Language.VIE, new CultureInfo("vi-VN") }
    };

    public LanguageService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public Language CurrentLanguage => _currentLanguage;
    public CultureInfo CurrentCulture => _currentCulture;

    public event Action? OnLanguageChanged;

    public async Task InitializeAsync()
    {
        try
        {
            var storedLanguage = await _localStorage.GetItemAsync<string>(LanguageKey);
            if (!string.IsNullOrEmpty(storedLanguage) && Enum.TryParse<Language>(storedLanguage, out var language))
            {
                _currentLanguage = language;
                _currentCulture = CultureMap[language];
            }
        }
        catch
        {
            // Use default if localStorage is not available
        }
    }

    public async Task SetLanguageAsync(Language language)
    {
        if (_currentLanguage != language)
        {
            _currentLanguage = language;
            _currentCulture = CultureMap[language];

            try
            {
                await _localStorage.SetItemAsync(LanguageKey, language.ToString());
            }
            catch
            {
                // Ignore localStorage errors
            }

            OnLanguageChanged?.Invoke();
        }
    }

    public string FormatCurrency(decimal amount)
    {
        // Vietnamese: Show without decimals (VND doesn't use decimals)
        // English: Show with 2 decimals
        if (_currentLanguage == Language.VIE)
        {
            return amount.ToString("N0", _currentCulture) + " ₫";
        }
        return amount.ToString("C2", _currentCulture);
    }

    public string FormatDate(DateTime date)
    {
        // Vietnamese: dd/MM/yyyy
        // English: MMM dd, yyyy
        return _currentLanguage == Language.VIE
            ? date.ToString("dd/MM/yyyy", _currentCulture)
            : date.ToString("MMM dd, yyyy", _currentCulture);
    }

    public string FormatDateTime(DateTime dateTime)
    {
        // Vietnamese: dd/MM/yyyy HH:mm
        // English: MMM dd, yyyy h:mm tt
        return _currentLanguage == Language.VIE
            ? dateTime.ToString("dd/MM/yyyy HH:mm", _currentCulture)
            : dateTime.ToString("MMM dd, yyyy h:mm tt", _currentCulture);
    }

    public string FormatShortDate(DateTime date)
    {
        return date.ToString("d", _currentCulture);
    }

    public string FormatNumber(decimal number)
    {
        return number.ToString("N2", _currentCulture);
    }

    public string FormatNumber(int number)
    {
        return number.ToString("N0", _currentCulture);
    }
}
