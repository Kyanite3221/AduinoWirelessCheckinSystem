using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPServer {
    class SQLHelper {
        //private static readonly string readerConnection = $"Host={"25.18.190.184"};Username={"reader"};Password={"safeT1st"};Database={"postgres"}";
        private static readonly string writerConnection = $"Host={"25.18.190.184"};Username={"editor"};Password=\"{"gZf<1xQ'E&.;(O9=1hL8"}\";Database={"postgres"}";

        public byte ReadGroups() {
            int result = 0;

            try {
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();

                string query = "SELECT * FROM public.grouplookup ORDER BY groupid";

                using (NpgsqlConnection conn = new NpgsqlConnection(writerConnection)) {
                    conn.Open();

                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                    da.Fill(ds);
                    dt = ds.Tables[0];

                    foreach (DataRow dataRow in dt.Rows) {
                        result = result * 2 + ((bool)dataRow["present"] ? 1 : 0);
                    }
                }
            }catch(Exception ex) {
                Console.Error.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}: {ex.GetType().FullName} {ex.Message}:\n{ex.StackTrace}");
            }

            return (byte)result;
        }

        public void LogRFID(string rfid) {
            try {
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();

                string query = $"SELECT * FROM public.rfidlookup AS RL " +
                    $"LEFT JOIN public.rfidgrouptable AS GT ON GT.rfid = RL.rfid " +
                    $"LEFT JOIN public.grouplookup AS GL ON GL.groupid = GT.groupid " +
                    $"WHERE RL.rfid = {rfid}";

                using (NpgsqlConnection conn = new NpgsqlConnection(writerConnection)) {
                    conn.Open();

                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                    da.Fill(ds);
                    dt = ds.Tables[0];

                    bool isPresent = (bool)dt.Rows[0]["present"];
                    int groupId = (int)dt.Rows[0]["groupid"];

                    Console.Out.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}: {dt.Rows[0]["ownername"]} set group {dt.Rows[0]["groupname"]} presences to {!isPresent}");

                    query = $"UPDATE public.grouplookup AS GL SET present = {!isPresent} WHERE GL.groupid = {groupId}";
                    if (new NpgsqlCommand(query, conn).ExecuteNonQuery() != 1) {
                        throw new Exception("No rows updated");
                    }
                }
            } catch (Exception ex) {
                Console.Error.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}: {ex.GetType().FullName} {ex.Message}:\n{ex.StackTrace}");
            }
        }
    }
}
