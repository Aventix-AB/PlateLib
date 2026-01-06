using Microsoft.EntityFrameworkCore;
using Common.DTOs;
using Common.Interfaces;
using Data.Entities;

namespace Data.Repositories;

public class ManufacturerRepository(OpenPlateContext context) : IManufacturerRepository
{
    private readonly OpenPlateContext _context = context;

    public async Task<ManufacturerDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var manufacturer = await _context.Manufacturers
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (manufacturer == null)
            return null;

        return MapToDTO(manufacturer);
    }

    public async Task<IEnumerable<ManufacturerDTO>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var manufacturers = await _context.Manufacturers
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);

        return manufacturers.Select(MapToDTO);
    }

    public async Task<Manufacturer> CreateAsync(Manufacturer manufacturer, CancellationToken cancellationToken = default)
    {
        _context.Manufacturers.Add(manufacturer);
        await _context.SaveChangesAsync(cancellationToken);
        return manufacturer;
    }

    private static ManufacturerDTO MapToDTO(Manufacturer manufacturer)
    {
        return new ManufacturerDTO
        {
            Id = manufacturer.Id,
            Name = manufacturer.Name
        };
    }
}
