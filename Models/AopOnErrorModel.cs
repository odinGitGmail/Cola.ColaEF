using SqlSugar;

namespace Cola.ColaEF.Models;

public class AopOnErrorModel
{
    public string? ConfigId { get; set; }
    public Action<SqlSugarException>? AopOnError { get; set; }
}