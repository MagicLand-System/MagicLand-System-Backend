namespace MagicLand_System.PayLoad.Response.Users
{
    public class AccountResponse
    {
        public Guid AccountId { get; set; }
        public required string StudentName { get; set; } = string.Empty;
        public required string AccountPhone { get; set; }
    }
}
