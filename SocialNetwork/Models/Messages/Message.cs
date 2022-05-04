using SocialNetwork.Models.Correspondences;
using SocialNetwork.Models.Users;

namespace SocialNetwork.Models.Messages;

public class Message
{
    public long Id { get; set; }
    public string Text { get; set; }
    public DateTime DateTime { get; set; }
    public bool IsEdited { get; set; }
    public User Sender { get; set; }
    public Correspondence Correspondence { get; set; }
}