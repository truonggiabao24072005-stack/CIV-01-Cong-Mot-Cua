using System.Text.Json;

namespace CongMotCua.Api.Filters;
public class UtcDateConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => r.GetDateTime().ToUniversalTime();
    public override void Write(Utf8JsonWriter w, DateTime v, JsonSerializerOptions o) => w.WriteStringValue(DateTime.SpecifyKind(v, DateTimeKind.Utc));
}
