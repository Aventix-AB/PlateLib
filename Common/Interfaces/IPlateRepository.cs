using Common.DTOs;

namespace Common.Interfaces;

public interface IPlateRepository
{
    Task<PlateDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PlateDTO>> GetAllAsync(CancellationToken cancellationToken = default);

    // PlateFile operations
    Task<IEnumerable<PlateFileDTO>> GetFilesByPlateIdAsync(Guid plateId, CancellationToken cancellationToken = default);
}
