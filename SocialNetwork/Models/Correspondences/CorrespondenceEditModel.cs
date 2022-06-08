using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Correspondences;

public class CorrespondenceEditModel
{
    [Required] public int Id { get; set; }
    [Required] [MaxLength(50)] public string Name { get; set; }
}