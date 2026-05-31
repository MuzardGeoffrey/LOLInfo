namespace LOLInfo.Services;

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using LOLInfo.IServices;
using LOLInfo.Utils;

using Microsoft.Extensions.Logging;

/// <summary>
/// Récupère la version courante du patch DataDragon depuis l'API Riot
/// et met à jour <see cref="DataDragonCdn.Version"/> pour tous les URL builders.
/// </summary>
public class PatchVersionService(ILogger<PatchVersionService> logger) : IPatchVersionService
{
    private const string VersionsUrl = DataDragonCdn.VersionsUrl;
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(10) };

    public string CurrentVersion { get; private set; } = DataDragonCdn.DefaultVersion;

    public async Task InitializeAsync()
    {
        logger.LogDebug("[PatchVersion] Récupération des versions DataDragon — URL : {Url}", VersionsUrl);

        try
        {
            var json     = await _http.GetStringAsync(VersionsUrl);
            var versions = JsonSerializer.Deserialize<string[]>(json);

            if (versions is null || versions.Length == 0)
            {
                logger.LogWarning("[PatchVersion] Réponse vide, utilisation de la version par défaut '{Default}'",
                    DataDragonCdn.DefaultVersion);
                return;
            }

            this.CurrentVersion        = versions[0];
            DataDragonCdn.Version = this.CurrentVersion;
            logger.LogInformation("[PatchVersion] Version DataDragon courante : {Version}", this.CurrentVersion);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex,
                "[PatchVersion] Erreur réseau, utilisation de la version par défaut '{Default}'",
                DataDragonCdn.DefaultVersion);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[PatchVersion] Erreur inattendue, utilisation de la version par défaut '{Default}'",
                DataDragonCdn.DefaultVersion);
        }
    }
}
