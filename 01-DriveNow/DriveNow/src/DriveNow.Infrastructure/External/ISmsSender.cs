namespace DriveNow.Infrastructure.External;

public interface ISmsSender
{
    void Send(string phone, string text);
}
