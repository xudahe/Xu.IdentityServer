using System;

namespace Xu.IdentityServer.Models
{
    public class RootEntity
    {
        /// <summary>
        /// 主键Id，领域对象标识
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Guid
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? ModifyTime { get; set; }

        /// <summary>
        /// 删除时间（不为空代表该条数据已删除），逻辑上的删除，非物理删除
        /// </summary>
        public DateTime? DeleteTime { get; set; }
    }
}