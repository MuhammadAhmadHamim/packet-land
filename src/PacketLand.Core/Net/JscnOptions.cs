using System.Text.Json;
using System.Text.Json.Serialization;

namespace PacketLand.Core.Net;

public static class JsonOptions{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web){
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
