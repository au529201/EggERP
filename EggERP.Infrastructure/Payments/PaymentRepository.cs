using EggERP.Application.Payments;
using EggERP.Domain.Entities;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace EggERP.Infrastructure.Payments;
public class PaymentRepository : IPaymentRepository
{
    private readonly EggERPDbContext _dbContext;
    public PaymentRepository(EggERPDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public Task<List<Payment>> GetByBusinessIdAsync(Guid businessId)
    {
        return _dbContext.Payments
            .Where(p => p.BusinessId == businessId)
            .OrderByDescending(p => p.PaymentDateUtc)
            .ToListAsync();
    }
    public Task<Payment?> GetByIdAsync(Guid businessId, Guid id)
    {
        return _dbContext.Payments
            .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.Id == id);
    }
    public async Task<Payment> AddAsync(Payment payment)
    {
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();
        return payment;
    }
}