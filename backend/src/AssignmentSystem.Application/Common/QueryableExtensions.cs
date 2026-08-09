using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Common;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, page, pageSize, totalCount);
    }

    public static IQueryable<T> ApplyOrdering<T>(
        this IQueryable<T> source,
        string? sort,
        string defaultColumn = "Id")
    {
        string columnName = defaultColumn;
        bool isDescending = false;

        if (!string.IsNullOrWhiteSpace(sort))
        {
            var parts = sort.Trim().Split(':', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 0)
                columnName = parts[0];

            if (parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase))
                isDescending = true;

        }

        return source.OrderByProperty(columnName, isDescending, defaultColumn);
    }

    private static IQueryable<T> OrderByProperty<T>(
        this IQueryable<T> source,
        string propertyName,
        bool isDescending,
        string fallbackProperty)
    {
        var type = typeof(T);

        var property = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                       ?? type.GetProperty(fallbackProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
            return source;

        var parameter = Expression.Parameter(type, "x");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        string methodName = isDescending ? "OrderByDescending" : "OrderBy";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            [type, property.PropertyType],
            source.Expression,
            Expression.Quote(orderByExpression)
        );

        return source.Provider.CreateQuery<T>(resultExpression);
    }
}
