using System;

namespace Xu.IdentityServer.Models
{
    /// <summary>
    /// 用户跟角色关联表
    /// </summary>
    public class UserRole : RootEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        public int RoleId { get; set; }
    }
}