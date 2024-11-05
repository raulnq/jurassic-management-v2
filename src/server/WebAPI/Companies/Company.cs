using WebAPI.CollaboratorPayments;
using WebAPI.PayrollPayments;

namespace WebAPI.Companies
{
    public class Company
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? DocumentNumber { get; set; }
        public string[] CcEmails { get; set; } = [];
        public string FromEmail { get; set; } = string.Empty;
        public string AccountantEmail { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public RegisterCollaboratorPayment.Command[]? CollaboratorPayments { get; set; }
        public RegisterPayrollPayment.Command[]? PayrollPayments { get; set; }
    }
}
