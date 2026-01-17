using System.Text.Json.Serialization;

namespace LibraryMS.Core.Domain.Common.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserStatus
    {
        Pending = 1,
        Approved = 2,
        Blocked = 3
    }

}
