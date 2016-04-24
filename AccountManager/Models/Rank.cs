using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lib = MVVMLibv2;
using CommonDatabase.SQLite;
using CommonDatabase;
using System.Data.Common;

namespace AccountManager.Models
{
    public class Rank
    {
        public Rank()
        {

        }
        public Rank(int id)
        {
            this.ID = id;
            DBRank.GetSingle(this);
        }
        public int ID { get; set; }
        public string Name { get; set; }
        public int Pos { get; set; }

        public static List<Rank> GetAll()
        {
            return DBRank.GetAll();
        }

        public override bool Equals(object obj)
        {
            Rank r = obj as Rank;
            if (r == null)
                return false;

            return r.ID == this.ID;
        }
    }

    internal static class DBRank
    {
        public static bool GetSingle(Rank r)
        {
            var db = Database<SQLiteDatabase>.GetInstance();
            string sql = "SELECT id,pos,name from rank where id=" + r.ID;
            var reader = db.ExecuteReader(sql, null);
            if (reader.Read())
            {
                Mapping(reader, r);
                db.CloseReader(reader);
                db = null;
                return true;
            }
            else
            {
                db = null;
                return false;
            }
            
        }
        public static List<Rank> GetAll()
        {
            var db = Database<SQLiteDatabase>.GetInstance();

            string sql = "SELECT id,pos,name from rank order by pos desc";
            var reader = db.ExecuteReader(sql, null);

            var output = new List<Rank>(20);
            while (reader.Read())
            {
                var tmp = new Rank();
                Mapping(reader, tmp);
                output.Add(tmp);
            }

            db.CloseReader(reader);
            db = null;
            return output;

        }

        private static void Mapping(DbDataReader reader, Rank r)
        {
            r.ID = reader.GetInt32(0);
            r.Pos = reader.GetInt32(1);
            r.Name = reader.GetString(2);
        }
    }

}
