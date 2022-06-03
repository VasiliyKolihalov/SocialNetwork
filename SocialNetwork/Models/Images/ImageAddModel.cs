using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Images;

public class ImageAddModel
{
    [Required] public string ImageData { get; set; }
}