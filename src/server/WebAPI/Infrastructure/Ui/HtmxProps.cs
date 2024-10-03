namespace WebAPI.Infrastructure.Ui;

public class HtmxProps
{
    public string Target { get; set; } = default!;
    public string Swap { get; set; } = "innerHTML";
    public string Endpoint { get; set; } = default!;
    public string Select { get; set; } = default!;
    public string Vals { get; set; } = default!;
    public string Confirm { get; set; } = default!;
    public string HttpMethod { get; set; } = default!;
    public string OnClick { get; set; } = default!;
    public string OnChange { get; set; } = default!;
    public string Include { get; set; } = default!;
    public string Encoding { get; set; } = default!;
    public HtmxProps()
    {

    }
    public HtmxProps(string endpoint, string target)
    {
        Endpoint = endpoint;
        Target = target;
    }
    public HtmxProps(string endpoint, string target, string swap) : this(endpoint, target)
    {
        Swap = swap;
    }

    public HtmxProps(string endpoint, string target, string swap, string select) : this(endpoint, target, swap)
    {
        Select = select;
    }
}
