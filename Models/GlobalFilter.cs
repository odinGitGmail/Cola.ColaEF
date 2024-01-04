using SqlSugar;

namespace Cola.ColaEF.Models;

public class GlobalQueryFilter
{
    public string? ConfigId { get; set; }
    public Action<QueryFilterProvider>? QueryFilter { get; set; }
}