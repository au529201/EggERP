using EggERP.Domain.Entities;
namespace EggERP.Application.Payments;
public interface IPaymentService
{
    Task<List<Payment>> GetPaymentsAsync(Guid businessId);
    Task<Payment?> GetPaymentByIdAsync(Guid businessId, Guid id);
    Task<Payment> CreatePaymentAsync(Payment payment);
}