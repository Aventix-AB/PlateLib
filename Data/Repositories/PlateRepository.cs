using Microsoft.EntityFrameworkCore;
using Common.DTOs;
using Common.Interfaces;
using Data.Entities;

namespace Data.Repositories;

public class PlateRepository(OpenPlateContext context) : IPlateRepository
{
    private readonly OpenPlateContext _context = context;

    public async Task<PlateDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plate = await _context.Plates
            .Include(p => p.Manufacturer)
            .Include(p => p.PlateFiles)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (plate == null)
            return null;

        return MapToDTO(plate);
    }

    public async Task<IEnumerable<PlateDTO>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var plates = await _context.Plates
            .Include(p => p.Manufacturer)
            .Include(p => p.PlateFiles)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return plates.Select(MapToDTO);
    }

    public async Task<IEnumerable<PlateDTO>> GetByManufacturerIdAsync(Guid manufacturerId, CancellationToken cancellationToken = default)
    {
        var plates = await _context.Plates
            .Include(p => p.Manufacturer)
            .Include(p => p.PlateFiles)
            .Where(p => p.ManufacturerId == manufacturerId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return plates.Select(MapToDTO);
    }

    public async Task<Plate> CreateAsync(Plate plate, CancellationToken cancellationToken = default)
    {
        _context.Plates.Add(plate);
        await _context.SaveChangesAsync(cancellationToken);
        return plate;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plate = await _context.Plates.FindAsync([id], cancellationToken);
        if (plate == null)
            return false;

        _context.Plates.Remove(plate);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Plates
            .AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<PlateFileDTO>> GetFilesByPlateIdAsync(Guid plateId, CancellationToken cancellationToken = default)
    {
        var files = await _context.PlateFiles
            .Where(f => f.PlateId == plateId)
            .OrderBy(f => f.FileName)
            .ToListAsync(cancellationToken);

        return files.Select(f => new PlateFileDTO
        {
            Id = f.Id,
            FileName = f.FileName,
            ContentType = f.ContentType
        });
    }

    public async Task<PlateFile> AddFileAsync(PlateFile plateFile, CancellationToken cancellationToken = default)
    {
        _context.PlateFiles.Add(plateFile);
        await _context.SaveChangesAsync(cancellationToken);
        return plateFile;
    }

    public async Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await _context.PlateFiles.FindAsync([fileId], cancellationToken);
        if (file == null)
            return false;

        _context.PlateFiles.Remove(file);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static PlateDTO MapToDTO(Plate plate)
    {
        return new PlateDTO
        {
            Id = plate.Id,
            Name = plate.Name,
            ManufacturerId = plate.ManufacturerId,
            ManufacturerName = plate.Manufacturer.Name,
            PlateFiles = plate.PlateFiles.Select(f => new PlateFileDTO
            {
                Id = f.Id,
                FileName = f.FileName,
                ContentType = f.ContentType
            }).ToList()
        };
    }
}
