using System.Text.Json.Serialization;

namespace LibraryMS.Core.Domain.Common.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Roles
    {
        Admin,
        User,
    }
}
