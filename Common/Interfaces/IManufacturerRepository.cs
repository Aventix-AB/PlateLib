using Common.DTOs;

namespace Common.Interfaces;

public interface IManufacturerRepository
{
    Task<ManufacturerDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ManufacturerDTO>> GetAllAsync(CancellationToken cancellationToken = default);
}
