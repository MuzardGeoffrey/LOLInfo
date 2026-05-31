namespace LOLInfo.Tests;

using System.Globalization;

using LOLInfo.Localization;
using LOLInfo.Models;
using LOLInfo.Models.CdragonModel;
using LOLInfo.Properties;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Vérifie que la localisation fonctionne pour les deux langues fournies
/// (français neutre + satellite anglais) et que la locale de données Riot suit.
/// </summary>
[TestClass]
public class LocalizationTests
{
    private CultureInfo _original = CultureInfo.CurrentUICulture;

    [TestInitialize]
    public void Save() => this._original = CultureInfo.CurrentUICulture;

    // Restaure la culture par défaut des tests après chaque cas.
    [TestCleanup]
    public void Restore()
    {
        CultureInfo.CurrentCulture   = this._original;
        CultureInfo.CurrentUICulture = this._original;
    }

    [TestMethod]
    public void Resources_French_ReturnsFrenchStrings()
    {
        AppLocalization.ApplyCulture("fr");

        Assert.AreEqual("Combattant", Resources.Tag_Fighter);
        Assert.AreEqual("Aucun",      Resources.Resource_None);
        Assert.AreEqual("Favoris",    Resources.Toolbar_Favorites);
        Assert.AreEqual("PA",         ChampionStat.AbilityPower.ToLabel());
    }

    [TestMethod]
    public void Resources_English_ReturnsEnglishStrings()
    {
        AppLocalization.ApplyCulture("en");

        Assert.AreEqual("Fighter",   Resources.Tag_Fighter);
        Assert.AreEqual("None",      Resources.Resource_None);
        Assert.AreEqual("Favorites", Resources.Toolbar_Favorites);
        Assert.AreEqual("AP",        ChampionStat.AbilityPower.ToLabel());
    }

    [TestMethod]
    public void DataLocale_FollowsUiLanguage()
    {
        AppLocalization.ApplyCulture("fr");
        Assert.AreEqual("fr_FR", AppLocalization.DataLocale);

        AppLocalization.ApplyCulture("en");
        Assert.AreEqual("en_US", AppLocalization.DataLocale);
    }

    [TestMethod]
    public void Tag_And_Resource_Keys_AreLanguageInvariant()
    {
        // Les clés de correspondance ne changent jamais, quelle que soit la langue.
        AppLocalization.ApplyCulture("en");
        Assert.AreEqual("Fighter", ChampionTags.Fighter);
        Assert.AreEqual("Mana",    ChampionResources.GetCategory("Mana"));
    }
}
