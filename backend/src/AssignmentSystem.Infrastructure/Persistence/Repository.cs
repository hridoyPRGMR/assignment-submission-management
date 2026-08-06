using AssignmentSystem.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class Repository<TEntity>(AppDbContext dbContext) : IRepository<TEntity> where TEntity : class
{
    private readonly DbSet<TEntity> _set = dbContext.Set<TEntity>();

    public IQueryable<TEntity> Query() => _set;

    public void Add(TEntity entity) => _set.Add(entity);

    public void AddRange(IEnumerable<TEntity> entities) => _set.AddRange(entities);

    public void Remove(TEntity entity) => _set.Remove(entity);
}
