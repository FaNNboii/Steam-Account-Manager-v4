using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommonDatabase.SQLite;
using CommonDatabase;
using System.IO;
using System.Reflection;

namespace AccountManager
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private const string DATASOURCE = "data.sqlite";
        public App()
        {
            try
            {
                PrepareDatabase("Data Source=" + DATASOURCE + ";Version=3;");
            }
            catch
            {
                MessageBox.Show("An error occured while creating the database");
                App.Current.Shutdown(1);
            }
        }
        public void PrepareDatabase(string connectionString)
        {
            if (!File.Exists(DATASOURCE))
            {
                var data = Assembly.GetExecutingAssembly().GetManifestResourceStream("AccountManager.dataorg.sqlite");
                using (BinaryReader reader = new BinaryReader(data))
                {
                    File.WriteAllBytes(DATASOURCE, reader.ReadBytes((int)reader.BaseStream.Length));
                }

            }
            Database<SQLiteDatabase>.ConnectionString = connectionString;

        }
    }
}
