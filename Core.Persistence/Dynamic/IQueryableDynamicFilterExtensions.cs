using System.Linq.Dynamic.Core;
using System.Text;

namespace Core.Persistence.Dynamic;

/// <summary>
/// IQueryable sorgularını dinamik olarak filtreleme ve sıralama işlemleri için genişletme metotlarını içerir.
/// </summary>
public static class IQueryableDynamicFilterExtensions
{
    private static readonly string[] _orders = { "asc", "desc" };
    private static readonly string[] _logics = { "and", "or" };

    private static readonly IDictionary<string, string> _operators = new Dictionary<string, string>
    {
        { "eq", "=" },
        { "neq", "!=" },
        { "lt", "<" },
        { "lte", "<=" },
        { "gt", ">" },
        { "gte", ">=" },
        { "isnull", "== null" },
        { "isnotnull", "!= null" },
        { "startswith", "StartsWith" },
        { "endswith", "EndsWith" },
        { "contains", "Contains" },
        { "doesnotcontain", "Contains" }
    };
    /// <summary>
    /// Verilen dinamik sorgu kurallarına göre IQueryable sorgusunu filtreler ve sıralar.
    /// </summary>
    /// <typeparam name="T">Sorgulanacak varlık türü.</typeparam>
    /// <param name="query">Filtreleme ve sıralama işlemlerinin uygulanacağı IQueryable sorgusu.</param>
    /// <param name="dynamicQuery">Filtreleme ve sıralama kurallarını içeren dinamik sorgu nesnesi.</param>
    /// <returns>Filtrelenmiş ve sıralanmış IQueryable sorgusu.</returns>
    public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, DynamicQuery dynamicQuery)
    {
        if (dynamicQuery.Filter is not null)
            query = Filter(query, dynamicQuery.Filter);
        if (dynamicQuery.Sort is not null && dynamicQuery.Sort.Any())
            query = Sort(query, dynamicQuery.Sort);
        return query;
    }
    /// <summary>
    /// Verilen filtre kurallarına göre IQueryable sorgusunu filtreler.
    /// </summary>
    /// <typeparam name="T">Sorgulanacak varlık türü.</typeparam>
    /// <param name="queryable">Filtreleme işleminin uygulanacağı IQueryable sorgusu.</param>
    /// <param name="filter">Filtre kurallarını içeren nesne.</param>
    /// <returns>Filtrelenmiş IQueryable sorgusu.</returns>
    private static IQueryable<T> Filter<T>(IQueryable<T> queryable, Filter filter)
    {
        IList<Filter> filters = GetAllFilters(filter);
        string?[] values = filters.Select(f => f.Value).ToArray();
        string where = Transform(filter, filters);
        if (!string.IsNullOrEmpty(where) && values != null)
            queryable = queryable.Where(where, values);

        return queryable;
    }
    /// <summary>
    /// Verilen sıralama kurallarına göre IQueryable sorgusunu sıralar.
    /// </summary>
    /// <typeparam name="T">Sorgulanacak varlık türü.</typeparam>
    /// <param name="queryable">Sıralama işleminin uygulanacağı IQueryable sorgusu.</param>
    /// <param name="sort">Sıralama kurallarını içeren nesnelerin listesi.</param>
    /// <returns>Sıralanmış IQueryable sorgusu.</returns>
    private static IQueryable<T> Sort<T>(IQueryable<T> queryable, IEnumerable<Sort> sort)
    {
        foreach (Sort item in sort)
        {
            if (string.IsNullOrEmpty(item.Field))
                throw new ArgumentException("Invalid Field");
            if (string.IsNullOrEmpty(item.Dir) || !_orders.Contains(item.Dir))
                throw new ArgumentException("Invalid Order Type");
        }

        if (sort.Any())
        {
            string ordering = string.Join(separator: ",", values: sort.Select(s => $"{s.Field} {s.Dir}"));
            return queryable.OrderBy(ordering);
        }

        return queryable;
    }
    /// <summary>
    /// Verilen filtre nesnesindeki tüm alt filtreleri (nested filters) bir liste olarak döndürür.
    /// </summary>
    /// <param name="filter">Ana filtre nesnesi.</param>
    /// <returns>Tüm filtrelerin düz liste hali.</returns>
    public static IList<Filter> GetAllFilters(Filter filter)
    {
        List<Filter> filters = new();
        GetFilters(filter, filters);
        return filters;
    }
    /// <summary>
    /// Verilen filtre nesnesinden tüm alt filtreleri (nested filters) bir listeye ekler.
    /// </summary>
    /// <param name="filter">Ana filtre nesnesi.</param>
    /// <param name="filters">Tüm filtrelerin eklenmesi için kullanılan liste.</param>
    private static void GetFilters(Filter filter, IList<Filter> filters)
    {
        filters.Add(filter);
        if (filter.Filters is not null && filter.Filters.Any())
            foreach (Filter item in filter.Filters)
                GetFilters(item, filters);
    }
    /// <summary>
    /// Verilen filtre nesnesini bir sorgu ifadesine dönüştürür.
    /// </summary>
    /// <param name="filter">Dönüştürülecek filtre nesnesi.</param>
    /// <param name="filters">Tüm filtrelerin listesi.</param>
    /// <returns>Sorgu için kullanılabilir string formatında filtre ifadesi.</returns>
    /// <exception cref="ArgumentException">
    /// Geçersiz alan adı, operatör veya mantıksal operatör kullanıldığında fırlatılır.
    /// </exception>
    public static string Transform(Filter filter, IList<Filter> filters)
    {
        if (string.IsNullOrEmpty(filter.Field))
            throw new ArgumentException("Invalid Field");
        if (string.IsNullOrEmpty(filter.Operator) || !_operators.ContainsKey(filter.Operator))
            throw new ArgumentException("Invalid Operator");

        int index = filters.IndexOf(filter);
        string comparison = _operators[filter.Operator];
        StringBuilder where = new();

        if (!string.IsNullOrEmpty(filter.Value))
        {
            if (filter.Operator == "doesnotcontain")
                where.Append($"(!np({filter.Field}).{comparison}(@{index.ToString()}))");
            else if (comparison is "StartsWith" or "EndsWith" or "Contains")
                where.Append($"(np({filter.Field}).{comparison}(@{index.ToString()}))");
            else
                where.Append($"np({filter.Field}) {comparison} @{index.ToString()}");
        }
        else if (filter.Operator is "isnull" or "isnotnull")
        {
            where.Append($"np({filter.Field}) {comparison}");
        }

        if (filter.Logic is not null && filter.Filters is not null && filter.Filters.Any())
        {
            if (!_logics.Contains(filter.Logic))
                throw new ArgumentException("Invalid Logic");
            return $"{where} {filter.Logic} ({string.Join(separator: $" {filter.Logic} ", value: filter.Filters.Select(f => Transform(f, filters)).ToArray())})";
        }

        return where.ToString();
    }
}