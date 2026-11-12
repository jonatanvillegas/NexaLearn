using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NexaLearn.Datatable;
using NexaLearn.DTOs;
using NexaLearn.DTOs.Datatable;
using NexaLearn.Exceptions;
using NexaLearn.Models;
using NexaLearn.Repositories.Interfaces;

namespace NexaLearn.Repositories;

public class PagoRepository : IPagoRepository
{
    private readonly NexaLearnContext _context;
    private readonly DataTableService _dataTable;

    public PagoRepository(NexaLearnContext context, DataTableService dataTable)
    {
        _context = context;
        _dataTable = dataTable;
    }

    public Task<DataTableResponse<PagoHistorialDto>> GetAllAsync(DataTableRequest? request = null,
        CancellationToken cancellationToken = default)
        => _dataTable.GetPagedDataAsync(_context.PagoVs.AsNoTracking()
            .Select(entity => new PagoHistorialDto(entity.CursoId, entity.Curso, entity.GuidId,
                entity.Cliente, entity.MontoPagado, entity.Moneda, entity.Estado, entity.FechaPago)), request ?? new());

    public async Task<PagoDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Pagos.AsNoTracking().SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<PagoDto> CreateAsync(GuardarPagoDto input, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(input, 0, cancellationToken);
        var entity = new Pago();
        Apply(entity, input);
        _context.Pagos.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, GuardarPagoDto input, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Pagos.FindAsync([id], cancellationToken);
        if (entity is null) return false;
        await ValidateAsync(input, id, cancellationToken);
        Apply(entity, input);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Pagos.FindAsync([id], cancellationToken);
        if (entity is null) return false;
        _context.Pagos.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateAsync(GuardarPagoDto input, int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Validator.ValidateObject(input, new ValidationContext(input), validateAllProperties: true);
        if (!await _context.Inscripcions.AnyAsync(entity => entity.Id == input.InscripcionId, cancellationToken))
            throw new ValidationException("La inscripción no existe.");
        var orderId = Normalize(input.PayPalOrderId);
        var captureId = Normalize(input.PayPalCaptureId);
        if (orderId is not null && await _context.Pagos.AnyAsync(entity =>
            entity.PayPalOrderId == orderId && entity.Id != id, cancellationToken))
            throw new ConflictoException("Ya existe un pago con ese PayPalOrderId.");
        if (captureId is not null && await _context.Pagos.AnyAsync(entity =>
            entity.PayPalCaptureId == captureId && entity.Id != id, cancellationToken))
            throw new ConflictoException("Ya existe un pago con ese PayPalCaptureId.");
    }

    private static PagoDto ToDto(Pago entity) => new(entity.Id, entity.InscripcionId,
        entity.PayPalOrderId, entity.PayPalCaptureId, entity.Monto, entity.Moneda, entity.Estado, entity.FechaPago);

    private static void Apply(Pago entity, GuardarPagoDto input)
    {
        entity.InscripcionId = input.InscripcionId;
        entity.PayPalOrderId = Normalize(input.PayPalOrderId);
        entity.PayPalCaptureId = Normalize(input.PayPalCaptureId);
        entity.Monto = input.Monto;
        entity.Moneda = input.Moneda.ToUpperInvariant();
        entity.Estado = input.Estado.Trim();
        entity.FechaPago = input.FechaPago;
    }


    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
