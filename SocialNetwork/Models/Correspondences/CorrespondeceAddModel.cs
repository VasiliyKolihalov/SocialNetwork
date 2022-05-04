using System.ComponentModel.DataAnnotations;
using SocialNetwork.Models.Messages;

namespace SocialNetwork.Models.Correspondences;

public class CorrespondeceAddModel
{
    [Required] public MessageAddModel MessageAddModel { get; set; }
    [Required] public List<int> ParticipantsId { get; set; }
}