namespace Fiap.Payment.Common.Http.CRM.Models
{
    public record UserResponse
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public TypeUser Type { get; set; }
        public bool Active { get; set; }
    }

    public enum TypeUser
    {
        Admin = 1,
        User = 2
    }

    public record ApiEnvelope<T>(T Data, bool Success, string? Message);
}
