using System.Collections.Generic;
using System.Threading.Tasks;
using ADH.Core.Entities;

namespace ADH.Application.Interfaces;

public interface ISlaPolicyRepository
{
    Task<IEnumerable<SlaPolicy>> GetAllAsync(CancellationToken cancellationToken);
    Task<SlaPolicy?> GetByPriorityAsync(string priority, CancellationToken cancellationToken);
}
