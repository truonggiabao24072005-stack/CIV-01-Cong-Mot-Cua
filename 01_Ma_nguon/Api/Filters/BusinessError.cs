namespace CongMotCua.Api.Filters;
public class BusinessError(string message, int status = 400) : Exception(message)
{
    public int Status => status;
}
