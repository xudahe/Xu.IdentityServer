using System.Collections.Generic;

namespace Xu.IdentityServer.Models.ViewModel
{
    public class AccessApiDateView
    {
        public int today { get; set; }
        public string[] columns { get; set; }
        public List<ApiDate> rows { get; set; }
    }

    public class ApiDate
    {
        public string date { get; set; }
        public int count { get; set; }
    }
}