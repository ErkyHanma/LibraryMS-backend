namespace LibraryMS.Infrastructure.Identity.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public required string Token { get; set; }
        public required string UserId { get; set; }
        public DateTime Expires { get; set; }

        public User? User { get; set; }
    }
}
