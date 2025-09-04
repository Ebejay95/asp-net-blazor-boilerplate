using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMC.Infrastructure.Services;

namespace CMC.Web.Services;

public sealed class EfRevisionsClient : IRevisionsClient
{
	private readonly RevisionService _svc;

	public EfRevisionsClient(RevisionService svc)
	{
		_svc = svc;
	}

	public async Task<List<EditRevisionItem>> GetAsync(string table, Guid assetId, int take = 100)
	{
		var list = await _svc.GetAsync(table, assetId, take);
		return list.Select(r => new EditRevisionItem(
			r.Id,
			r.CreatedAt,
			r.Action,
			r.UserEmail,
			r.Data ?? "{}"
		)).ToList();
	}

	public Task RestoreAsync(long revisionId) => _svc.RestoreAsync(revisionId);
}
