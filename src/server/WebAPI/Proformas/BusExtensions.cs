using Rebus.Bus;
using static WebAPI.Proformas.IssueProforma;

namespace WebAPI.Proformas;

public static class BusExtensions
{
    public static Task SubscribeToProforma(this IBus bus)
    {
        return bus.Subscribe<ProformaIssued>();
    }
}