using System.Text.Json.Serialization;

namespace LibraryMS_API.Core.Domain.Common.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AccountRequestStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }

}
