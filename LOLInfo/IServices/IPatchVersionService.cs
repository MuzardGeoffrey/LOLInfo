namespace LOLInfo.IServices;

using System.Threading.Tasks;

public interface IPatchVersionService
{
    string CurrentVersion { get; }
    Task InitializeAsync();
}
