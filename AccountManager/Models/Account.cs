using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lib = MVVMLibv2;
using CommonDatabase;
using System.Data.Common;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AccountManager.Models
{
    public class Account : lib.DataModelBase, IDataErrorInfo
    {
        public Account()
        {

        }

        private int id;
        [lib.IgnoreModify]
        public int ID
        {
            get { return id; }
            set { id = value; OnChange(); }
        }

        private string nick;
        public string Nickname
        {
            get { return nick; }
            set { nick = value; OnChange(); }
        }

        private string username="";
        public string Username
        {
            get { return username; }
            set { username = value; OnChange(); }
        }

        private string password="";
        public string Password
        {
            get { return password; }
            set { password = value; OnChange(); }
        }

        private string email="";
        public string Email
        {
            get { return email; }
            set { email = value; OnChange(); }
        }

        private Rank rank;
        public Rank Rank
        {
            get { return rank; }
            set { rank = value; OnChange(); }
        }


        private string steamID="";
        public string SteamID
        {
            get { return steamID; }
            set
            {
                steamID = value;
                CalculateIDs(value);
            }
        }

        private string profID="";
        public string ProfileID
        {
            get { return profID; }
            set
            {
                profID = value;
                CalculateIDs(value);
            }
        }

        private string wins = "";
        public string Wins
        {
            get { return wins; }
            set { wins = value; OnChange(); }
        }

        private TimeSpan playableIn;

        [lib.IgnoreModify]
        public TimeSpan PlayableIn
        {
            get { return playableIn; }
            set { playableIn = value; OnChange(); }
        }


        private void CalculateIDs(string newID)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(newID))
                {
                    if (Models.CommonSteamMethods.IsValidSteamID(newID))
                    {
                        steamID = newID;
                        profID = CommonSteamMethods.CalculateProfId(newID);
                    }
                    else if(Models.CommonSteamMethods.IsValidProfileID(newID))
                    {
                        profID = newID;
                        steamID = CommonSteamMethods.CalculateSteamID(newID);
                    }

                    OnChange("ProfileID");
                    OnChange("SteamID");
                }
            }
            catch
            {

            }
        }

        private bool vacban;
        public bool VACBan
        {
            get { return vacban; }
            set { vacban = value; OnChange(); }
        }

        private bool communityban;
        public bool CommunityBan
        {
            get { return communityban; }
            set { communityban = value; OnChange(); }
        }

        private string economyBan="none";
        public string EconomyBan
        {
            get { return economyBan; }
            set { economyBan = value; OnChange(); }
        }

        private DateTime lastLogin;
        public DateTime LastPlayed
        {
            get { return lastLogin; }
            set { lastLogin = value; OnChange();}
        }

        private string authKey="";
        public string AuthKey
        {
            get { return authKey; }
            set { authKey = value; OnChange(); }
        }

        private BitmapImage avatar;
        public BitmapImage Avatar
        {
            get { return avatar; }
            set { avatar = value; OnChange(); }
        }

        private string profileURL;
        public string ProfileURL
        {
            get { return profileURL; }
            set { profileURL = value; OnChange(); }
        }

        private bool online;
        public bool Online
        {
            get { return online; }
            set { online = value; OnChange(); }
        }

        private bool ingame;

        public bool IsIngame
        {
            get { return ingame; }
            set { ingame = value; OnChange(); }
        }

        #region IDataErrorInfo
        private string errorFlag = null;
        [lib.IgnoreModify]
        public string Error
        {
            get { return errorFlag; }
            set { errorFlag = value; OnChange(); }
        }
        public string this[string columnName]
        {
            get { return CheckProperty(columnName); }
        }
        #endregion

        private string CheckProperty(string propname)
        {
            string result = null;

            if (propname == "AuthKey" && !string.IsNullOrEmpty(AuthKey) && !CommonSteamMethods.IsBase64(AuthKey))
            {
                result = "Shared Secret is not valid";
            }
            else if (propname == "Email" && !string.IsNullOrEmpty(Email) && !System.Text.RegularExpressions.Regex.IsMatch(Email, @"[\w-]+@[\w-]+\.(\w{2,6})+"))
                result = "Email must be filled correctly or left blank";
            else if (propname == "Username" && string.IsNullOrWhiteSpace(Username))
                result = "Username is obligatory";
            else if (propname == "ProfileID" && !string.IsNullOrWhiteSpace(ProfileID) && !Models.CommonSteamMethods.IsValidProfileID(ProfileID))
                result = "ProfileID is not valid, must have at least 18 digits";
            else if (propname == "SteamID" && !string.IsNullOrWhiteSpace(SteamID) && !Models.CommonSteamMethods.IsValidSteamID(SteamID))
                result = "SteamID is not valid. Has to be like STEAM_0:1:1337";


            Error = result != null ? "Error" : null;
            return result;
        }

        public override bool Save()
        {
            var result = DBAccount.SaveOrUpdate(this);
            if (result)
                result |= base.Save();

            return result;
        }
        public bool Delete()
        {
            var result = DBAccount.Delete(this);
            if (result)
                this.ID = 0;

            return result;
        }
        public static List<Account> GetAll()
        {
            return DBAccount.GetAll();
        }

        public override bool Equals(object obj)
        {
            Account r = obj as Account;
            if (r == null)
                return false;

            return r.ID == this.ID;

        }
    }

    internal class DBAccount
    {
        public static bool SaveOrUpdate(Account a)
        {
            if (a.ID > 0)
                return Update(a);
            else
                return Insert(a);
        }
        private static bool Insert(Account a)
        {
            var db = Database<CommonDatabase.SQLite.SQLiteDatabase>.GetInstance();
            string sql = "INSERT INTO account (username,password,email,profileid,rankid,lastlogin,authkey) VALUES(@username,@pw,@email,@profid,@rankid,@lastlogin,@authkey)";
            var parameters = new List<Parameter>();
            parameters.Add(new Parameter("@username", a.Username));
            parameters.Add(new Parameter("@pw", a.Password));
            parameters.Add(new Parameter("@email", a.Email));
            parameters.Add(new Parameter("@profid", a.ProfileID));
            parameters.Add(new Parameter("@rankid", a.Rank == null ? -1 : a.Rank.ID));
            parameters.Add(new Parameter("@lastlogin", a.LastPlayed));
            parameters.Add(new Parameter("@authkey", a.AuthKey));

            int newID;
            var result = db.ExecuteNonQuery(sql, parameters, out newID) == 1;
            db = null;

            if (result)
                a.ID = newID;

            return result;

        }
        private static bool Update(Account a)
        {
            var db = Database<CommonDatabase.SQLite.SQLiteDatabase>.GetInstance();
            string sql = "UPDATE account SET username=@username,password=@pw,email=@email,profileid=@profid,rankid=@rankid,lastlogin=@lastlogin,authkey=@authkey where id=@id";
            var parameters = new List<Parameter>();
            parameters.Add(new Parameter("@username", a.Username));
            parameters.Add(new Parameter("@pw", a.Password));
            parameters.Add(new Parameter("@email", a.Email));
            parameters.Add(new Parameter("@profid", a.ProfileID));
            parameters.Add(new Parameter("@rankid", a.Rank == null ? -1 : a.Rank.ID));
            parameters.Add(new Parameter("@lastlogin", a.LastPlayed));
            parameters.Add(new Parameter("@authkey", a.AuthKey));
            parameters.Add(new Parameter("@id", a.ID));

            var result = db.ExecuteNonQuery(sql, parameters) == 1;
            db = null;
            return result;

        }
        public static bool Delete(Account a)
        {
            var db = Database<CommonDatabase.SQLite.SQLiteDatabase>.GetInstance();
            string sql = "DELETE from account WHERE id=" + a.ID;
            var result = db.ExecuteNonQuery(sql, null) == 1;
            db = null;
            return result;
        }
        public static List<Account> GetAll()
        {
            var db = Database<CommonDatabase.SQLite.SQLiteDatabase>.GetInstance();
            string sql = "SELECT id,username,password,email,profileid,rankid,lastlogin,authkey from account order by id";
            var reader = db.ExecuteReader(sql, null);

            var output = new List<Account>(30);

            while (reader.Read())
            {
                Account a = new Account();
                Mapping(reader, a);
                output.Add(a);
            }

            db.CloseReader(reader);
            db = null;
            return output;


        }

        private static void Mapping(DbDataReader reader, Account a)
        {
            a.BeginInitialize();
            a.ID = reader.GetInt32(0);
            a.Username = reader.GetString(1);
            a.Password = reader.GetString(2);
            a.Email = reader.GetString(3);
            a.ProfileID = reader.GetString(4);
            a.Rank = new Rank(reader.GetInt32(5));
            a.LastPlayed = reader.GetDateTime(6);
            a.AuthKey = reader.GetString(7);
            a.EndInitialize();
        }
    }
}
