using System.Collections.Generic;
using System.Threading.Tasks;
using ADH.Core.Entities;

namespace ADH.Application.Interfaces;

public interface ISlaPolicyRepository
{
    Task<IEnumerable<SlaPolicy>> GetAllAsync();
    Task<SlaPolicy?> GetByPriorityAsync(string priority);
}
