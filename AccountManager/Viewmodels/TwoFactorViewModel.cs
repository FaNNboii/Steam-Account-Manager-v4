using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using lib = MVVMLibv2;

namespace AccountManager.Viewmodels
{
    public class TwoFactorViewModel : lib.ViewModelBase
    {
        DispatcherTimer t = null;
        public TwoFactorViewModel()
        {
            new Thread(() => Models.SteamTwoFactor.AlignTime()).Start();
          
            CopyClipboardCommand = new lib.Command((o) => Clipboard.SetText(string.IsNullOrEmpty(Code) ? "" : Code));

            t = new DispatcherTimer();
            t.Interval = TimeSpan.FromSeconds(1);
            t.Tick += ((o, e) => DoAsyncWork());
            t.Start();
        }


        #region Properties

        public string Secret { get; set; }

        private string code;
        public string Code
        {
            get { return code; }
            private set { code = value; OnChange(); }
        }

        private int remainingsecs = 0;
        public int RemainingSeconds
        {
            get { return remainingsecs; }
            set { remainingsecs = value; OnChange(); }
        }

        #endregion


        #region Commands

        private lib.Command copyClipboardCommand;
        public lib.Command CopyClipboardCommand
        {
            get { return copyClipboardCommand; }
            private set { copyClipboardCommand = value; OnChange(); }
        }

        #endregion


        #region Methods
        private void DoAsyncWork()
        {
            if (string.IsNullOrWhiteSpace(Secret))
                return;

            long time = Models.SteamTwoFactor.GetSteamTime();
            RemainingSeconds = 30 - (int)(time % 30);
            Code = Models.SteamTwoFactor.GenerateSteamGuardCodeForTime(time, Secret);
            CopyClipboardCommand.Execute();
        }

        protected override void OnClosing(object o)
        {
            t.Stop();
            t = null;
        }

        #endregion
    }
}
