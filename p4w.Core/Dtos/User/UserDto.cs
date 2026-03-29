namespace p4w.Core.Interfaces.Services.Auth
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; } 
        public string mediaLinkUrl { get; set; } = null!;
    }
}