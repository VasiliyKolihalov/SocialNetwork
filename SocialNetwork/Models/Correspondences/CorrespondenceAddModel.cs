using System.ComponentModel.DataAnnotations;
using SocialNetwork.Models.Messages;

namespace SocialNetwork.Models.Correspondences;

public class CorrespondenceAddModel
{
    [Required] public string Name { get; set; }
    [Required] public MessageAddModel MessageAddModel { get; set; }
    [Required] public List<int> ParticipantsId { get; set; }
}