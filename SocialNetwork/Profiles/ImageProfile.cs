using AutoMapper;
using SocialNetwork.Models.Images;

namespace SocialNetwork.Profiles;

public class ImageProfile : Profile
{
    public ImageProfile()
    {
        CreateMap<Image, ImageViewModel>().ForMember(nameof(ImageViewModel.ImageData), opt =>
            opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));
        CreateMap<ImageAddModel, Image>().ForMember(nameof(Image.ImageData), opt =>
            opt.MapFrom(x => Convert.FromBase64String(x.ImageData)));
    }
   
}