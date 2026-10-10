using System;
using System.Windows;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace CabinetOftalmoWPF
{
    public partial class AdaugaPacientWindow : Window
    {
        string dbConnection = "Data Source=pacienti.db;Version=3;";
        public AdaugaPacientWindow() { InitializeComponent(); }

        private void BtnSalveaza_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNume.Text)) { MessageBox.Show("Numele este obligatoriu!", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!string.IsNullOrWhiteSpace(TxtTelefon.Text) && !Regex.IsMatch(TxtTelefon.Text, @"^[\d\s\+\-]+$")) { MessageBox.Show("Telefon invalid!", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            string dioptriiCombinate = $"OD: {TxtOD.Text.Trim()} | OS: {TxtOS.Text.Trim()}";

            using (SQLiteConnection conn = new SQLiteConnection(dbConnection))
            {
                conn.Open();
                string sql = "INSERT INTO Pacienti (Nume, Telefon, Email, Rama, TipLentila, Dioptrii, Observatii) VALUES (@Nume, @Telefon, @Email, @Rama, @TipLentila, @Dioptrii, @Observatii)";
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Nume", TxtNume.Text);
                    cmd.Parameters.AddWithValue("@Telefon", TxtTelefon.Text);
                    cmd.Parameters.AddWithValue("@Email", TxtEmail.Text);
                    cmd.Parameters.AddWithValue("@Rama", TxtRama.Text);
                    cmd.Parameters.AddWithValue("@TipLentila", TxtTipLentila.Text);
                    cmd.Parameters.AddWithValue("@Dioptrii", dioptriiCombinate);
                    cmd.Parameters.AddWithValue("@Observatii", TxtObservatii.Text);
                    cmd.ExecuteNonQuery();
                }

                string sqlIstoric = "INSERT INTO IstoricPacienti (PacientId, DataInregistrare, Rama, TipLentila, Dioptrii, Observatii) VALUES (last_insert_rowid(), @Data, @Rama, @TipLentila, @Dioptrii, @Observatii)";
                using (SQLiteCommand cmdIst = new SQLiteCommand(sqlIstoric, conn))
                {
                    cmdIst.Parameters.AddWithValue("@Data", DateTime.Now.ToString("dd/MM/yyyy - HH:mm"));
                    cmdIst.Parameters.AddWithValue("@Rama", TxtRama.Text);
                    cmdIst.Parameters.AddWithValue("@TipLentila", TxtTipLentila.Text);
                    cmdIst.Parameters.AddWithValue("@Dioptrii", dioptriiCombinate);
                    cmdIst.Parameters.AddWithValue("@Observatii", TxtObservatii.Text);
                    cmdIst.ExecuteNonQuery();
                }
            }
            MessageBox.Show("Salvat cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information); this.Close();
        }
    }
}