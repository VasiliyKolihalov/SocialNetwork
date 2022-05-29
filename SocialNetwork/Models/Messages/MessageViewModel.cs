using SocialNetwork.Models.Users;

namespace SocialNetwork.Models.Messages;

public class MessageViewModel
{
    public long Id { get; set; }
    public string Text { get; set; }
    public DateTime DateTime { get; set; }
    public bool IsEdited { get; set; }
    public UserPreviewModel Sender { get; set; }
}