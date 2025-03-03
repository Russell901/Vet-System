namespace Vet_System.Models
{
    public class UserInfo
    {
        public string Username { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;

        public UserInfo(string username, string displayName)
        {
            Username = username;
            DisplayName = displayName;
        }
    }
}