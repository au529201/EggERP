using EggERP.Application.Inventory;
using EggERP.Application.Transactions;
using EggERP.Application.Customers;
using EggERP.Domain.Entities;
namespace EggERP.Application.Sales;
public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IInventoryService _inventoryService;
    private readonly ICustomerRepository _customerRepository;
    public SaleService(
        ISaleRepository saleRepository,
        IInventoryService inventoryService,
        ICustomerRepository customerRepository,
        ITransactionManager transactionManager)
    {
        _saleRepository = saleRepository;
        _inventoryService = inventoryService;
        _customerRepository = customerRepository;
        _transactionManager = transactionManager;
    }
    public Task<List<Sale>> GetSalesAsync(Guid businessId)
    {
        return _saleRepository.GetByBusinessIdAsync(businessId);
    }
    public Task<Sale?> GetSaleByIdAsync(Guid businessId, Guid id)
    {
        return _saleRepository.GetByIdAsync(businessId, id);
    }
    public Task<List<SaleItem>> GetSaleItemsAsync(
        Guid businessId,
        Guid saleId)
    {
        return _saleRepository.GetItemsBySaleIdAsync(
            businessId,
            saleId);
    }
    public async Task<Sale> CreateSaleAsync(
        Guid businessId,
        Guid? customerId,
        List<CreateSaleItemRequest> items,
        string paymentMethod,
        string? paymentSource,
        string? referenceNumber,
        string status,
        DateTime? saleDate = null,
        string? notes = null)
    {
        PaymentValidation.EnsureValid(paymentMethod, referenceNumber, paymentSource);
        if (customerId.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(
                businessId,
                customerId.Value);

            if (customer is null)
            {
                throw new InvalidOperationException(
                    "The selected customer does not belong to this business.");
            }
        }

        if (items is null || items.Count == 0)
        {
            throw new InvalidOperationException("A sale must have at least one item.");
        }

        if (businessId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "A valid Business ID is required.");
        }

        if (status != "Paid" && status != "Pending")
        {
            throw new InvalidOperationException(
                "Status must be 'Paid' or 'Pending'.");
        }

        foreach (var item in items)
        {
            if (item.ProductId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Each sale item must have a valid product.");
            }

            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException(
                    "Sale item quantity must be greater than zero.");
            }

            if (item.UnitPrice < 0)
            {
                throw new InvalidOperationException(
                    "Sale item price cannot be negative.");
            }
        }

        var duplicateProduct = items
            .GroupBy(i => i.ProductId)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateProduct is not null)
        {
            throw new InvalidOperationException(
                "The same product cannot appear more than once " +
                "in a sale. Combine its quantities into one line.");
        }

        var stockErrors = new List<string>();

        foreach (var i in items)
        {
            var inventory = await _inventoryService
                .GetByProductIdAsync(businessId, i.ProductId);

            if (inventory is null)
            {
                stockErrors.Add(
                    $"Product {i.ProductId} has no inventory record " +
                    "for this business.");

                continue;
            }

            var available = inventory.QuantityOnHand;

            if (i.Quantity > available)
            {
                stockErrors.Add(
                    $"Product {i.ProductId}: only {available:N2} " +
                    $"in stock, but {i.Quantity:N2} requested.");
            }
        }

        if (stockErrors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot complete sale: {string.Join("; ", stockErrors)}. Adjust the quantity or remove the affected item(s) before continuing.");
        }

        var saleItems = items.Select(i => new SaleItem
        {
            Id = Guid.NewGuid(),
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            DiscountAmount = 0,
            TaxAmount = 0,
            TotalAmount = i.Quantity * i.UnitPrice
        }).ToList();

        var subtotal = saleItems.Sum(si => si.TotalAmount);

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            CustomerId = customerId,
            SaleDateUtc = saleDate.HasValue
    ? DateTime.SpecifyKind(
        saleDate.Value.Date,
        DateTimeKind.Local).ToUniversalTime()
    : DateTime.UtcNow,

            Notes = string.IsNullOrWhiteSpace(notes)
    ? null
    : notes.Trim(),
            Subtotal = subtotal,
            TaxAmount = 0,
            DiscountAmount = 0,
            TotalAmount = subtotal,
            PaymentMethod = paymentMethod,
            PaymentSource = paymentSource,
            ReferenceNumber = referenceNumber,
            Status = status,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var item in saleItems)
        {
            item.SaleId = sale.Id;
        }

        await _transactionManager.ExecuteAsync(async () =>
        {
            await _saleRepository.CreateAsync(sale, saleItems);

            foreach (var item in saleItems)
            {
                await _inventoryService.AdjustQuantityAsync(
                    businessId,
                    item.ProductId,
                    -item.Quantity);
            }
        });

        return sale;
    }


    public async Task<(bool Succeeded, string? Error)> MarkAsPaidAsync(
        Guid businessId,
        Guid saleId,
        string paymentMethod,
        string? paymentSource,
        string? referenceNumber)
    {
        if (businessId == Guid.Empty || saleId == Guid.Empty)
        {
            return (false, "A valid business and sale are required.");
        }

        var sale = await _saleRepository.GetByIdAsync(
            businessId,
            saleId);

        if (sale is null)
        {
            return (false, "Sale not found.");
        }

        if (sale.Status == "Paid")
        {
            return (false, "This sale is already paid.");
        }

        if (sale.Status != "Pending")
        {
            return (false, "Only pending sales can be marked as paid.");
        }

        try
        {
            PaymentValidation.EnsureValid(
                paymentMethod,
                referenceNumber,
                paymentSource);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }

        var updated = await _saleRepository.TryMarkAsPaidAsync(
            businessId,
            saleId,
            paymentMethod,
            paymentSource,
            referenceNumber);

        if (!updated)
        {
            return (
                false,
                "The sale was already updated or is no longer pending. " +
                "Refresh the sales list and try again.");
        }

        return (true, null);
    }

    Guid businessId,
        Guid saleId,
        string paymentMethod,
        string? paymentSource,
        string? referenceNumber)
    {
        var sale = await _saleRepository.GetByIdAsync(
            businessId,
            saleId);

        if (sale is null)
        {
            return (false, "Sale not found.");
        }

        if (sale.Status == "Paid")
        {
            return (false, "This sale is already paid.");
        }

        if (sale.Status != "Pending")
        {
            return (false, "Only pending sales can be marked as paid.");
        }

        try
        {
            PaymentValidation.EnsureValid(
                paymentMethod,
                referenceNumber,
                paymentSource);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }

        sale.Status = "Paid";
        sale.PaymentMethod = paymentMethod;
        sale.PaymentSource = paymentSource;
        sale.ReferenceNumber = referenceNumber;
        sale.UpdatedAtUtc = DateTime.UtcNow;

        await _saleRepository.UpdateAsync(sale);

        return (true, null);
    }
}