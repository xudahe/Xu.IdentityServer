using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Xu.IdentityServer.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser<int>
    {
        public string LoginName { get; set; }

        public string RealName { get; set; }

        public int sex { get; set; } = 0;

        public int age { get; set; }

        public DateTime birth { get; set; } = DateTime.Now;

        public string addr { get; set; }

        public string FirstQuestion { get; set; }
        public string SecondQuestion { get; set; }

        /// <summary>
        /// 删除时间（不为空代表该条数据已删除），逻辑上的删除，非物理删除
        /// </summary>
        public DateTime? DeleteTime { get; set; }

        public ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}