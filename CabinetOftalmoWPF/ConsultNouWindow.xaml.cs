using System;
using System.Windows;
using System.Data.SQLite;

namespace CabinetOftalmoWPF
{
    public partial class ConsultNouWindow : Window
    {
        string dbConnection = "Data Source=pacienti.db;Version=3;";
        int idPacientCurent;

        public ConsultNouWindow(Pacient pacient)
        {
            InitializeComponent();
            idPacientCurent = pacient.Id;
            TxtNume.Text = $"Consult nou pentru: {pacient.Nume}";
            TxtRama.Text = pacient.Rama; // Precompletăm cu rama curentă ca să îi fie ușor
            TxtTipLentila.Text = pacient.TipLentila;
        }

        private void BtnSalveazaConsult_Click(object sender, RoutedEventArgs e)
        {
            string dioptrii = $"OD: {TxtOD.Text.Trim()} | OS: {TxtOS.Text.Trim()}";

            using (SQLiteConnection conn = new SQLiteConnection(dbConnection))
            {
                conn.Open();
                string sqlUpdate = "UPDATE Pacienti SET Rama=@Rama, TipLentila=@TipLentila, Dioptrii=@Dioptrii, Observatii=@Observatii WHERE Id=@Id";
                using (SQLiteCommand cmd = new SQLiteCommand(sqlUpdate, conn))
                {
                    cmd.Parameters.AddWithValue("@Rama", TxtRama.Text);
                    cmd.Parameters.AddWithValue("@TipLentila", TxtTipLentila.Text);
                    cmd.Parameters.AddWithValue("@Dioptrii", dioptrii);
                    cmd.Parameters.AddWithValue("@Observatii", TxtObservatii.Text);
                    cmd.Parameters.AddWithValue("@Id", idPacientCurent);
                    cmd.ExecuteNonQuery();
                }

                string sqlInsert = "INSERT INTO IstoricPacienti (PacientId, DataInregistrare, Rama, TipLentila, Dioptrii, Observatii) VALUES (@Id, @Data, @Rama, @TipLentila, @Dioptrii, @Observatii)";
                using (SQLiteCommand cmdIst = new SQLiteCommand(sqlInsert, conn))
                {
                    cmdIst.Parameters.AddWithValue("@Id", idPacientCurent);
                    cmdIst.Parameters.AddWithValue("@Data", DateTime.Now.ToString("dd/MM/yyyy - HH:mm"));
                    cmdIst.Parameters.AddWithValue("@Rama", TxtRama.Text);
                    cmdIst.Parameters.AddWithValue("@TipLentila", TxtTipLentila.Text);
                    cmdIst.Parameters.AddWithValue("@Dioptrii", dioptrii);
                    cmdIst.Parameters.AddWithValue("@Observatii", TxtObservatii.Text);
                    cmdIst.ExecuteNonQuery();
                }
            }
            MessageBox.Show("Consult adăugat în istoric!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information); this.Close();
        }
    }
}