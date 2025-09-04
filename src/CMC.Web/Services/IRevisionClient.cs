using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMC.Web.Services;

public interface IRevisionsClient
{
    Task<List<EditRevisionItem>> GetAsync(string table, Guid assetId, int take = 100);
    Task RestoreAsync(long revisionId);
}
