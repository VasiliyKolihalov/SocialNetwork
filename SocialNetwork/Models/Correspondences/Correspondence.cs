using SocialNetwork.Models.Messages;
using SocialNetwork.Models.Users;
using SocialNetwork.Repository;

namespace SocialNetwork.Models.Correspondences;

public class Correspondence
{
    public int Id { get; set; }
    public string Name { get; set; }
    public User Admin { get; set; }
    public List<User> Users { get; set; }
}