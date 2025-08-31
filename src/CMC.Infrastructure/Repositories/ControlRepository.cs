using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories
{
	public class ControlRepository : IControlRepository
	{
		private readonly AppDbContext _db;
		public ControlRepository(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

		public Task<Control?> GetByIdAsync(Guid id, CancellationToken ct = default)
			=> _db.Controls.FirstOrDefaultAsync(x => x.Id == id, ct);

		public Task<List<Control>> GetAllAsync(CancellationToken ct = default)
			=> _db.Controls.AsNoTracking().OrderBy(x => x.CreatedAt).ToListAsync(ct);

		public async Task AddAsync(Control e, CancellationToken ct = default)
		{
			_db.Controls.Add(e);
			await _db.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(Control e, CancellationToken ct = default)
		{
			_db.Controls.Update(e);
			await _db.SaveChangesAsync(ct);
		}

        public async Task DeleteAsync(Control e, CancellationToken ct = default)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            e.Delete();
            _db.Controls.Update(e);
            await _db.SaveChangesAsync(ct);
        }

		public Task<List<Control>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
			=> _db.Controls.AsNoTracking()
				.Where(x => x.CustomerId == customerId)
				.OrderBy(x => x.CreatedAt)
				.ToListAsync(ct);

        public Task<Control?> GetByIdWithCustomerAsync(Guid id, CancellationToken ct = default)
            => _db.Controls
                .Include(x => x.Customer)
                    .ThenInclude(c => c.CustomerIndustries)
                        .ThenInclude(ci => ci.Industry)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

		public async Task AddRangeAsync(IEnumerable<Control> controls, CancellationToken ct = default)
		{
			_db.Controls.AddRange(controls);
			await _db.SaveChangesAsync(ct);
		}
	}
}
