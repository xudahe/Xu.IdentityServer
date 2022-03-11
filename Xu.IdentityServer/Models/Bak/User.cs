using System;

namespace Xu.IdentityServer.Models
{
    /// <summary>
    /// 用户信息表
    /// </summary>
    public class User : RootEntity
    {
        /// <summary>
        /// 登录账号(用户名)
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        public string LoginPwd { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 所属部门
        /// </summary>
        public int? DeptId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastErrTime { get; set; }

        /// <summary>
        /// 登录错误次数
        /// </summary>
        public int? ErrorCount { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime Birth { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 用户关联角色的id或guid集合，不能同时包含两者
        /// </summary>
        public string RoleIds { get; set; }

        /// <summary>
        /// 关联角色Xml
        /// </summary>
        public string RoleInfoXml { get; set; }
    }
}