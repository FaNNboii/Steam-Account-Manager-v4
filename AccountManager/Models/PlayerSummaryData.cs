using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AccountManager.Models
{
    public class PlayerSummaryData
    {
        public string ProfileID { get; set; }
        public string AvatarURL { get; set; }
        public string Nickname { get; set; }
        public string ProfileURL { get; set; }
        public bool Online { get; set; }
        public bool IsIngame { get; set; }
    }
}
