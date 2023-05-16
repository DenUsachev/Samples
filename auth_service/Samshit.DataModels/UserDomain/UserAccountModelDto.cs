using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Samshit.DataModels.Attributes;

namespace Samshit.DataModels.UserDomain
{
    public enum AccountType
    {
        Internal, Facebook, Vk
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserAccountModelDto
    {
        public int Id { get; set; }
        public long UserId { get; set; }

        [Required]
        public AccountType Type { get; set; }

        [SwaggerIgnore]
        public string SocialId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [SwaggerIgnore]
        public DateTime Created { get; set; }
        
        [SwaggerIgnore]
        public DateTime? Updated { get; set; }
        
        [SwaggerIgnore]
        public DateTime? LastLogin { get; set; }
        public bool IsLocked { get; set; }

    }
}
