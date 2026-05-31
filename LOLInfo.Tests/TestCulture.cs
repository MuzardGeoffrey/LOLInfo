namespace LOLInfo.Tests;

using LOLInfo.Localization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Fixe la culture de l'assembly de test sur la langue par défaut de l'application
/// (<see cref="AppLocalization.DefaultCulture"/>) avant l'exécution des tests.
///
/// Les tests vérifient les libellés de la locale par défaut (français) ; sans cela,
/// sur une machine configurée dans une autre langue, le ResourceManager renverrait
/// le satellite correspondant (ex : anglais) et les assertions échoueraient.
/// </summary>
[TestClass]
public static class TestCulture
{
    [AssemblyInitialize]
    public static void Initialize(TestContext _) => AppLocalization.ApplyDefaultCulture();
}
