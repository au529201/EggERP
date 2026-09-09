using EggERP.Domain.Entities;
namespace EggERP.Application.Payments;
public interface IPaymentRepository
{
    Task<List<Payment>> GetByBusinessIdAsync(Guid businessId);
    Task<Payment?> GetByIdAsync(Guid businessId, Guid id);
    Task<Payment> AddAsync(Payment payment);
}