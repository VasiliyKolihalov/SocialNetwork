﻿using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Communities;

public class CommunityEditModel
{
    [Required] public int Id { get; set; }
    [Required] public string Name { get; set; }
    [Required] public string Description { get; set; }
}