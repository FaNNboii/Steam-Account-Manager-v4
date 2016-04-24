using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Models
{
    internal class PlayerBanData
    {
        public string ProfileID { get; set; }
        public bool VACBan { get; set; }
        public bool CommunityBan { get; set; }
        public string EconomyBan { get; set; }
    }
}
