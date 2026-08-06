namespace AssignmentSystem.Application.Abstractions.Persistence;

public interface IRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> Query();

    void Add(TEntity entity);

    void AddRange(IEnumerable<TEntity> entities);

    void Remove(TEntity entity);
}
