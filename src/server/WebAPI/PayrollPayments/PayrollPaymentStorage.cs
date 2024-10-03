using Infrastructure;
using System.Net.Mime;

namespace WebAPI.PayrollPayments;

public class PayrollPaymentStorage : AzureBlobStorage
{
    public PayrollPaymentStorage(string connectionString) : base(connectionString, "payroll")
    {
    }

    public Task<string> Upload(string name, Stream stream)
    {
        return Upload(name, stream, MediaTypeNames.Application.Pdf);
    }
}
