using EggERP.Domain.Entities;
namespace EggERP.Application.Payments;
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }
    public Task<List<Payment>> GetPaymentsAsync(Guid businessId)
    {
        return _paymentRepository.GetByBusinessIdAsync(businessId);
    }
    public Task<Payment?> GetPaymentByIdAsync(Guid businessId, Guid id)
    {
        return _paymentRepository.GetByIdAsync(businessId, id);
    }
    public async Task<Payment> CreatePaymentAsync(Payment payment)
    {
        payment.Id = Guid.NewGuid();
        payment.CreatedAtUtc = DateTime.UtcNow;
        if (payment.PaymentDateUtc == default)
        {
            payment.PaymentDateUtc = DateTime.UtcNow;
        }
        return await _paymentRepository.AddAsync(payment);
    }
}