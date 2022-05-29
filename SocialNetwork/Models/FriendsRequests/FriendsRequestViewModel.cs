using SocialNetwork.Models.Users;

namespace SocialNetwork.Models.FriendsRequests;

public class FriendsRequestViewModel
{
    public int Id { get; set; }
    public string Message { get; set; }
    public UserPreviewModel Sender { get; set; }
}