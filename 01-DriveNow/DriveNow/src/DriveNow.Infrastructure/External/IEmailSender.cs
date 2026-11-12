namespace DriveNow.Infrastructure.External;

public interface IEmailSender
{
    void Send(string to, string subject, string body);
}
