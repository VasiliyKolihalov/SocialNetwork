using SocialNetwork.Models.Users;

namespace SocialNetwork.Models.FriendsRequests;

public class FriendRequest
{
    public int Id { get; set; }
    public string Message { get; set; }
    public User Sender { get; set; }
    public int RecipientId { get; set; }
}