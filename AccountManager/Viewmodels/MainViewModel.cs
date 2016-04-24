using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lib = MVVMLibv2;
using System.Threading;
using System.Diagnostics;
using WindowsInput;
using WindowsInput.Native;
using System.Windows;

namespace AccountManager.Viewmodels
{
    public class MainViewModel : lib.ListEditViewModel<Models.Account>
    {
        Thread refreshLockTimesThread = null;
        Thread avatarThread = null;
        public MainViewModel()
        {
#if !DEBUG

            Thread timeAlignThread = new Thread(Models.SteamTwoFactor.AlignTime);
            timeAlignThread.Start();

            Thread ranksThread = new Thread(() =>
            {
                var ranks = Models.Rank.GetAll();
                AvailableRanks = ranks;
            });
            ranksThread.Start();


            Items = new System.Collections.ObjectModel.ObservableCollection<Models.Account>(Models.Account.GetAll());

            avatarThread = new Thread(LoadAccountSummary);
            avatarThread.Start();
            
            Thread banThread = new Thread(LoadAccountBans);
            banThread.Start();

            LoginCommand = new lib.AutomatedCommand(Login, CanLogin);
            OpenURLCommand = new lib.Command(OpenURL);
            LockAccountCommand = new lib.AutomatedCommand(LockAccount, CanLockAccount);
            CopyClipboardCommand = new lib.AutomatedCommand(CopyCode,CanCopyCode);

            ranksThread.Join();
            refreshLockTimesThread = new Thread(RefreshLockTimes);
            refreshLockTimesThread.Start();
#endif
        }

        protected override void OnClosing(object o)
        {
            if(refreshLockTimesThread!=null)
                refreshLockTimesThread.Abort();

            if (avatarThread != null)
                avatarThread.Abort();
            base.OnClosing(o);

        }

        private void RefreshLockTimes()
        {
            while (true)
            {
                var now = DateTime.Now;
                foreach (var acc in Items)
                {
                    if (now.CompareTo(acc.LastPlayed) < 0)
                    {
                        acc.PlayableIn = acc.LastPlayed - now;
                    }
                }

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }


        public lib.AutomatedCommand LockAccountCommand { get; set; }

        private void LockAccount(object o)
        {
            DataObject.LastPlayed = DateTime.Now.AddHours(24);
            DataObject.Save();
        }
        private bool CanLockAccount(object o)
        {
            return DataObject != null && DateTime.Now.CompareTo(DataObject.LastPlayed)>0;
        }

        protected override bool CanRemoveItem(object o)
        {
            return base.CanRemoveItem(o) && DataObject.ID>0;
        }
        private List<Models.Rank> ranks;

        public List<Models.Rank> AvailableRanks
        {
            get { return ranks; }
            set { ranks = value; OnChange(); }
        }

        public lib.AutomatedCommand LoginCommand { get; set; }
        public lib.Command OpenURLCommand { get; set; }
        private void Login(object o)
        {
            if(!Models.CommonSteamMethods.Login(DataObject))
            {
                SendMessage("An error occured!", lib.MessageType.Error);
            }
        }
        private bool CanLogin(object o)
        {
            return DataObject != null && !string.IsNullOrWhiteSpace(DataObject.Username) && !string.IsNullOrWhiteSpace(DataObject.Password);
        }

        private lib.AutomatedCommand copyClipboardCommand;
        public lib.AutomatedCommand CopyClipboardCommand
        {
            get { return copyClipboardCommand; }
            private set { copyClipboardCommand = value; OnChange(); }
        }

        private void CopyCode(object o)
        {
            Clipboard.SetText(Models.SteamTwoFactor.GenerateSteamGuardCodeForTime(Models.SteamTwoFactor.GetSteamTime(), DataObject.AuthKey));
        }
        private bool CanCopyCode(object o)
        {
            return DataObject != null && !string.IsNullOrWhiteSpace(DataObject.AuthKey);
        }


        protected override void Save(object o)
        {
            base.Save(o);
            new Thread(() =>
            {
                LoadAccountSummary(new List<Models.Account>() { DataObject });
                LoadAccountBans(new List<Models.Account>() { DataObject });
            }).Start();
        }
        protected override void RemoveItem(object o)
        {
            if (DataObject.Delete())
                base.RemoveItem(o);
            else
                SendMessage("An error occured during deleting", lib.MessageType.Error);
        }

        private void LoadAccountBans()
        {
            LoadAccountBans(Items);
        }
        private void LoadAccountBans(IList<Models.Account> accounts)
        {
            var banData = Models.SteamAPiCalls.LoadBans(accounts);

            foreach (var item in accounts)
            {
                foreach (var ban in banData)
                {
                    if (ban.ProfileID == item.ProfileID)
                    {
                        item.BeginInitialize();
                        var app = App.Current;
                        if (app == null)
                            return;

                        app.Dispatcher.Invoke(() =>
                        {
                            item.EconomyBan = ban.EconomyBan;
                            item.CommunityBan = ban.CommunityBan;
                            item.VACBan = ban.VACBan;
                        });
                        item.EndInitialize();
                        break;
                    }
                }
            }
        }
        private void LoadAccountSummary()
        {
            LoadAccountSummary(Items);
        }

        private void LoadAccountSummary(IList<Models.Account> accounts)
        {
            int i = 0;
            while (true)
            {
                var summaryData = Models.SteamAPiCalls.LoadPlayerSummaries(accounts);

                foreach (var acc in accounts)
                {
                    foreach (var summary in summaryData)
                    {
                        if (summary.ProfileID == acc.ProfileID)
                        {
                            var app = App.Current;
                            if (app == null)
                                return;

                            app.Dispatcher.Invoke(() =>
                            {
                                acc.BeginInitialize();
                                acc.Nickname = summary.Nickname;
                                acc.Avatar = new System.Windows.Media.Imaging.BitmapImage(new Uri(summary.AvatarURL, UriKind.Absolute));
                                acc.ProfileURL = summary.ProfileURL;
                                acc.Online = summary.Online;
                                acc.IsIngame = summary.IsIngame;

                                acc.EndInitialize();
                            });
                            break;
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private void OpenURL(object url)
        {
            if(!string.IsNullOrWhiteSpace((string)url))
                Process.Start(new ProcessStartInfo((string)url));
        }

        public Action<string> ShowTwoFactorCode;
    }
}
