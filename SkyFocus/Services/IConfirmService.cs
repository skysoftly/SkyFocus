using System.Threading.Tasks;

namespace SkyFocus.Services;

public interface IConfirmService
{
    Task<bool> Show(string text);
}