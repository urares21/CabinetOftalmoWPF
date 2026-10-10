using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;

namespace CabinetOftalmoWPF
{
    public class InregistrareIstoric
    {
        public string DataInregistrare { get; set; }
        public string Rama { get; set; }
        public string TipLentila { get; set; }
        public string Dioptrii { get; set; }
        public string Observatii { get; set; }
    }

    public partial class IstoricWindow : Window
    {
        string dbConnection = "Data Source=pacienti.db;Version=3;";
        int idPacient;

        public IstoricWindow(Pacient pacient)
        {
            InitializeComponent();
            idPacient = pacient.Id;
            TxtNumePacient.Text = $"Istoric modificări pentru: {pacient.Nume}";
            IncarcaIstoric();
        }

        private void IncarcaIstoric()
        {
            List<InregistrareIstoric> istoric = new List<InregistrareIstoric>();
            using (SQLiteConnection conn = new SQLiteConnection(dbConnection))
            {
                conn.Open();
                string sql = "SELECT DataInregistrare, Rama, TipLentila, Dioptrii, Observatii FROM IstoricPacienti WHERE PacientId = @Id ORDER BY Id DESC";
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", idPacient);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            istoric.Add(new InregistrareIstoric
                            {
                                DataInregistrare = reader["DataInregistrare"].ToString(),
                                Rama = reader["Rama"] != DBNull.Value ? reader["Rama"].ToString() : "",
                                TipLentila = reader["TipLentila"] != DBNull.Value ? reader["TipLentila"].ToString() : "",
                                Dioptrii = reader["Dioptrii"].ToString(),
                                Observatii = reader["Observatii"] != DBNull.Value ? reader["Observatii"].ToString() : ""
                            });
                        }
                    }
                }
            }
            TabelIstoric.ItemsSource = istoric;
        }
    }
}