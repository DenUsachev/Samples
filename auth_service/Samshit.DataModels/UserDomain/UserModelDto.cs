using Swashbuckle.AspNetCore.Annotations;
using System;
using Newtonsoft.Json;
using Samshit.DataModels.Attributes;

namespace Samshit.DataModels.UserDomain
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserModelDto : UserModelSecuredDto
    {
        public string OldPassword { get; set; }
        public string Password { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserAuthResult : UserModelSecuredDto
    {
        public string RecoveryToken { get; set; }
        public DateTime? RecoveryTokenIssued { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserModelUpdatePasswordDto
    {
        public string RecoveryToken { get; set; }
        public string Password { get; set; }
    }


    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserModelSecuredDto
    {
        public long UserId { get; set; }
        public string Login { get; set; }
        
        [SwaggerIgnore]
        public DateTime Created { get; set; }

        [SwaggerIgnore]
        public DateTime? Updated { get; set; }
        public UserPermissionsDto Permissions { get; set; }

        [SwaggerIgnore]
        public bool? HasLogin { get; set; }

        [SwaggerIgnore]
        public bool? HasPassword { get; set; }


        public UserAccountModelDto[] LinkedAccounts { get; set; }
    }
}
