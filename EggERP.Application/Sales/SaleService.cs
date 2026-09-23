using EggERP.Application.Flocks;
using EggERP.Application.Inventory;
using EggERP.Application.Products;
using EggERP.Domain.Entities;

namespace EggERP.Application.Sales;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IInventoryService _inventoryService;
    private readonly IProductRepository _productRepository;
    private readonly IFlockRepository _flockRepository;

    public SaleService(
        ISaleRepository saleRepository,
        IInventoryService inventoryService,
        IProductRepository productRepository,
        IFlockRepository flockRepository)
    {
        _saleRepository = saleRepository;
        _inventoryService = inventoryService;
        _productRepository = productRepository;
        _flockRepository = flockRepository;
    }

    public Task<List<Sale>> GetSalesAsync(Guid businessId)
    {
        return _saleRepository.GetByBusinessIdAsync(businessId);
    }

    public Task<Sale?> GetSaleByIdAsync(Guid businessId, Guid id)
    {
        return _saleRepository.GetByIdAsync(businessId, id);
    }

    public Task<List<SaleItem>> GetSaleItemsAsync(Guid saleId)
    {
        return _saleRepository.GetItemsBySaleIdAsync(saleId);
    }

    public async Task<SaleCreationResult> CreateSaleAsync(
        Guid businessId,
        Guid? customerId,
        List<CreateSaleItemRequest> items,
        string paymentMethod,
        string? paymentSource,
        string? referenceNumber,
        string status)
    {
        PaymentValidation.EnsureValid(paymentMethod, referenceNumber, paymentSource);

        if (items is null || items.Count == 0)
        {
            throw new InvalidOperationException("A sale must have at least one item.");
        }

        if (status != "Paid" && status != "Pending")
        {
            throw new InvalidOperationException("Status must be 'Paid' or 'Pending'.");
        }

        var saleItems = new List<SaleItem>();
        var adjustments = new List<SaleItemAdjustment>();

        foreach (var i in items)
        {
            var inventory = await _inventoryService.GetByProductIdAsync(businessId, i.ProductId);
            var available = inventory?.QuantityOnHand ?? 0;

            if (available <= 0)
            {
                adjustments.Add(new SaleItemAdjustment
                {
                    ProductId = i.ProductId,
                    RequestedQuantity = i.Quantity,
                    SoldQuantity = 0,
                    WasDropped = true
                });
                continue;
            }

            var quantityToSell = Math.Min(i.Quantity, available);

            if (quantityToSell < i.Quantity)
            {
                adjustments.Add(new SaleItemAdjustment
                {
                    ProductId = i.ProductId,
                    RequestedQuantity = i.Quantity,
                    SoldQuantity = quantityToSell,
                    WasDropped = false
                });
            }

            saleItems.Add(new SaleItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                Quantity = quantityToSell,
                UnitPrice = i.UnitPrice,
                DiscountAmount = 0,
                TaxAmount = 0,
                TotalAmount = quantityToSell * i.UnitPrice
            });
        }

        if (saleItems.Count == 0)
        {
            throw new InvalidOperationException("No items could be sold: all requested products are out of stock.");
        }

        var subtotal = saleItems.Sum(si => si.TotalAmount);

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            CustomerId = customerId,
            SaleDateUtc = DateTime.UtcNow,
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

        var createdSale = await _saleRepository.CreateAsync(sale, saleItems);

        foreach (var item in saleItems)
        {
            await _inventoryService.AdjustQuantityAsync(businessId, item.ProductId, -item.Quantity);

            var product = await _productRepository.GetByIdAsync(businessId, item.ProductId);
            if (product is null)
            {
                continue;
            }

            var qty = (int)item.Quantity;

            if (product.Group == "Flock")
            {
                var flock = await _flockRepository.GetByLinkedProductIdAsync(businessId, item.ProductId);
                if (flock is not null)
                {
                    await _flockRepository.AddFlockMovementAsync(new FlockMovement
                    {
                        Id = Guid.NewGuid(),
                        FlockId = flock.Id,
                        MovementDate = sale.SaleDateUtc,
                        Direction = "Out",
                        Reason = "Sold",
                        Quantity = qty,
                        Notes = $"From Sale #{sale.Id}",
                        CreatedAtUtc = DateTime.UtcNow
                    });
                }
            }
            else if (product.Group == "Egg")
            {
                await _flockRepository.AddEggMovementAsync(new EggMovement
                {
                    Id = Guid.NewGuid(),
                    BusinessId = businessId,
                    ProductId = item.ProductId,
                    FlockId = null,
                    MovementDate = sale.SaleDateUtc,
                    Direction = "Out",
                    Reason = "Sold",
                    Quantity = qty,
                    Notes = $"From Sale #{sale.Id}",
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
        }

        return new SaleCreationResult
        {
            Sale = createdSale,
            Adjustments = adjustments
        };
    }
}