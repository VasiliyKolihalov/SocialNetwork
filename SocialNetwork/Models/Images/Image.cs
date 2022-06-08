namespace SocialNetwork.Models.Images;

public class Image
{
    public int Id { get; set; }
    public byte[] ImageData { get; set; }
    public int UserId { get; set; }
}