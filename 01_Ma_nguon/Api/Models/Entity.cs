namespace CongMotCua.Api.Models;
public abstract class Entity
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
}
