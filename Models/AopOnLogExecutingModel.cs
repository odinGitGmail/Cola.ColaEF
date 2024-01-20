using SqlSugar;

namespace Cola.ColaEF.Models;

public class AopOnLogExecutingModel
{
    public string? ConfigId { get; set; }
    public Action<string, SugarParameter[]>? AopOnLogExecuting { get; set; }
}