using System;
using System.Linq;
using Microsoft.AspNetCore.Server.IIS.Core;
using Samshit.DataModels.UserDomain;
using Samshit.DbAccess.Postgre;

namespace Samshit.AuthGateway.Extensions
{
    public static class UserModelExtensions
    {
        public static UserAccount ToDbEntity(this UserAccountModelDto dto)
        {
            return new UserAccount
            {
                Id = dto.Id,
                UserId = dto.UserId,
                SocialId = dto.SocialId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                LastLogin = dto.LastLogin,
                Created = dto.Created,
                Updated = dto.Updated ?? DateTime.Now
            };
        }

        public static UserAccountModelDto ToDomain(this UserAccount entity)
        {
            return new UserAccountModelDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                SocialId = entity.SocialId,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Email = entity.Email,
                LastLogin = entity.LastLogin,
                Created = entity.Created,
                Updated = entity.Updated
            };
        }

        public static UserModelDto ToDomain(this User entity)
        {
            var model = new UserModelDto
            {
                UserId = entity.Id,
                Created = entity.Created,
                Updated = entity.Updated,
                Login = entity.Login,
                Password = entity.Password,
                Permissions = new UserPermissionsDto(entity.PermissionsMask)
            };
            if (entity.UserAccounts != null && entity.UserAccounts.Any())
            {
                model.LinkedAccounts = entity.UserAccounts.Select(it => it.ToDomain()).ToArray();
            }
            return model;
        }

        public static UserModelSecuredDto ToDomainSecure(this User entity)
        {
            var model = new UserModelSecuredDto
            {
                UserId = entity.Id,
                Created = entity.Created,
                Updated = entity.Updated,
                Login = entity.Login,
                Permissions = new UserPermissionsDto(entity.PermissionsMask)
            };
            if (entity.UserAccounts != null && entity.UserAccounts.Any())
            {
                model.LinkedAccounts = entity.UserAccounts.Select(it => it.ToDomain()).ToArray();
            }
            return model;
        }

        public static UserAuthResult ToAuthResult(this User entity)
        {
            var model = new UserAuthResult
            {
                UserId = entity.Id,
                Created = entity.Created,
                Updated = entity.Updated,
                Login = entity.Login,
                Permissions = new UserPermissionsDto(entity.PermissionsMask),
                AccessToken = entity.AccessToken,
                RefreshToken = entity.RefreshToken
            };
            if (entity.UserAccounts != null && entity.UserAccounts.Any())
            {
                model.LinkedAccounts = entity.UserAccounts.Select(it => it.ToDomain()).ToArray();
            }
            return model;
        }
    }
}
