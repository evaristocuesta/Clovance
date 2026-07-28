namespace Clovance.ApiService.Domain.Transactions;

public class Transfer
{
    public required Transaction From { get; set; }
    public required Transaction To { get; set; }
}
