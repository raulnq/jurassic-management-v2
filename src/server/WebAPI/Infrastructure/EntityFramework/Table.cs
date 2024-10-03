namespace WebAPI.Infrastructure.EntityFramework;

public class Table
{
    private readonly string _name;

    private readonly string? _alias;

    public Table(string name, string? alias = null)
    {
        _name = string.IsNullOrEmpty(alias) ? name : $"{name} AS {alias}";

        _alias = alias;
    }

    public string Field(string field) => string.IsNullOrEmpty(_alias) ? $"{_name}.{field}" : $"{_alias}.{field}";

    public string Field(string field, string alias) => $"{Field(field)} AS {alias}";

    public string AllFields => $"{_name}.*";

    public static implicit operator string(Table table) => table.ToString();

    public override string ToString() => _name;
}