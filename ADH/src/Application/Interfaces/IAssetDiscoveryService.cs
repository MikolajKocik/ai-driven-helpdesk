using System.Threading.Tasks;

namespace ADH.Application.Interfaces;

public interface IAssetDiscoveryService
{
    Task<int> DiscoverNewAssetsAsync();
}