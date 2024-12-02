using CPW_ExpandedCityworksDB.Configuration;
using Microsoft.EntityFrameworkCore;
using XTI_Core;
using XTI_Core.EF;

namespace CPW_ExpandedCityworksDB;

public sealed class ExpandedCityworksDbContext : DbContext
{
    private readonly UnitOfWork unitOfWork;

    public ExpandedCityworksDbContext(DbContextOptions<ExpandedCityworksDbContext> options) : base(options)
    {
        unitOfWork = new UnitOfWork(this);
        ExpandedFees = new EfDataRepository<ExpandedFeeEntity>(this);
        ExpandedReceivableLineItems = new EfDataRepository<ExpandedReceivableLineItemEntity>(this);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ExpandedFeeEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ExpandedReceivableLineItemConfiguration());
    }

    public DataRepository<ExpandedFeeEntity> ExpandedFees { get; }
    public DataRepository<ExpandedReceivableLineItemEntity> ExpandedReceivableLineItems { get; }

    public Task Transaction(Func<Task> action) => unitOfWork.Execute(action);

    public Task<T> Transaction<T>(Func<Task<T>> action) => unitOfWork.Execute(action);
}
