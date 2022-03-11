using System;

namespace Xu.IdentityServer.Models
{
    /// <summary>
    /// 角色表
    /// </summary>
    public class Role : RootEntity
    {
        /// <summary>
        /// 角色简码
        /// </summary>
        public string RoleCode { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 角色关联菜单的id或guid集合，不能同时包含两者
        /// </summary>
        public string MenuIds { get; set; }

        /// <summary>
        /// 关联菜单Xml
        /// </summary>
        public string MenuInfoXml { get; set; }
    }
}