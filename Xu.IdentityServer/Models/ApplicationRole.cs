using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Xu.IdentityServer.Models
{
    // Add profile data for application roles by adding properties to the ApplicationRole class
    public class ApplicationRole : IdentityRole<int>
    {
        /// <summary>
        /// 删除时间（不为空代表该条数据已删除），逻辑上的删除，非物理删除
        /// </summary>
        public DateTime? DeleteTime { get; set; }

        /// <summary>
        /// 描述（备注）
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; } = DateTime.Now;

        public ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}