using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.FriendsRequests;

public class FriendRequestAddModel
{
    [Required] public int RecipientId { get; set; }
    public string Message { get; set; }
}