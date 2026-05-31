namespace LOLInfo.Tests;

using LOLInfo.Localization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Fixe la culture de l'assembly de test sur le <b>français</b> avant l'exécution.
///
/// La majorité des tests vérifient les libellés français. La langue par défaut de
/// l'application est l'anglais (<see cref="AppLocalization.DefaultCulture"/>), mais
/// les tests sont volontairement découplés de ce défaut : ils fixent explicitement
/// la culture attendue. Sans cela, le ResourceManager renverrait le satellite
/// correspondant à la machine et les assertions échoueraient.
/// </summary>
[TestClass]
public static class TestCulture
{
    [AssemblyInitialize]
    public static void Initialize(TestContext _) => AppLocalization.ApplyCulture("fr");
}
