namespace DriveNow.Infrastructure.External;

public interface IPaymentGateway
{
    string Charge(int customerId, decimal amount, string description);
    void Refund(int customerId, decimal amount, string description);
}
