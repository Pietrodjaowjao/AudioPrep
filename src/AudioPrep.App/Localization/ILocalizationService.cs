namespace AudioPrep.App.Localization;

public interface ILocalizationService
{
    IReadOnlyList<LanguageOption> SupportedLanguages { get; }

    AppLanguage CurrentLanguage { get; set; }

    UiText Text { get; }
}
