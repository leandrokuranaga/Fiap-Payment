namespace Fiap.Payment.Common.Http.CRM.Models
{
    public class UserResponse
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
}
