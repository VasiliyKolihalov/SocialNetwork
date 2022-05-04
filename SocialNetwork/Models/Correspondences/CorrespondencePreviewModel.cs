﻿using SocialNetwork.Models.Messages;
using SocialNetwork.Models.Users;

namespace SocialNetwork.Models.Correspondences;

public class CorrespondencePreviewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    private List<UserViewModel> Users { get; set; } 
}