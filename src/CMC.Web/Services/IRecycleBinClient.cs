using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMC.Contracts.RecycleBin;

namespace CMC.Web.Services;

public interface IRecycleBinClient
{
    Task<List<RecycleBinItemDto>> GetAllAsync(CancellationToken ct = default);
    Task RestoreAsync(RecycleBinItemDto item, CancellationToken ct = default);
    Task PurgeAsync(RecycleBinItemDto item, CancellationToken ct = default);
}
