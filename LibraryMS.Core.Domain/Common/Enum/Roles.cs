using System.Text.Json.Serialization;

namespace LibraryMS.Core.Domain.Common.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Roles
    {
        Admin,
        User,
        Demo // Just for demo purpose.
             // Represents a user who can only view books and borrow records, but cannot perform any actions like creating, updating, or deleting.
    }
}
