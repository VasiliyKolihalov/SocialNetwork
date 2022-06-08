using System.ComponentModel.DataAnnotations;
using SocialNetwork.Constants;

namespace SocialNetwork.Models.FriendsRequests;

public class FriendRequestAddModel
{
    [Required] public int RecipientId { get; set; }
    [MaxLength(500)] public string Message { get; set; }
}