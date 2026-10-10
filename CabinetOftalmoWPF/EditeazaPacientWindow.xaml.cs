using System.Data.SQLite;
using System.Windows;
using System.Text.RegularExpressions;

namespace CabinetOftalmoWPF
{
    public partial class EditeazaPacientWindow : Window
    {
        string dbConnection = "Data Source=pacienti.db;Version=3;";
        int idPacientCurent;

        public EditeazaPacientWindow(Pacient pacient)
        {
            InitializeComponent();
            idPacientCurent = pacient.Id;
            TxtNume.Text = pacient.Nume;
            TxtTelefon.Text = pacient.Telefon;
            TxtEmail.Text = pacient.Email;
            TxtRama.Text = pacient.Rama;
            TxtTipLentila.Text = pacient.TipLentila;
            TxtObservatii.Text = pacient.Observatii;

            if (!string.IsNullOrEmpty(pacient.Dioptrii) && pacient.Dioptrii.Contains("OD:") && pacient.Dioptrii.Contains("OS:"))
            {
                var bucati = pacient.Dioptrii.Split('|');
                if (bucati.Length == 2)
                {
                    TxtOD.Text = bucati[0].Replace("OD:", "").Trim();
                    TxtOS.Text = bucati[1].Replace("OS:", "").Trim();
                }
            }
            else TxtOD.Text = pacient.Dioptrii;
        }

        private void BtnActualizeaza_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNume.Text)) { MessageBox.Show("Nume obligatoriu!", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            string dioptrii = $"OD: {TxtOD.Text.Trim()} | OS: {TxtOS.Text.Trim()}";

            using (SQLiteConnection conn = new SQLiteConnection(dbConnection))
            {
                conn.Open();
                string sql = "UPDATE Pacienti SET Nume=@Nume, Telefon=@Telefon, Email=@Email, Rama=@Rama, TipLentila=@TipLentila, Dioptrii=@Dioptrii, Observatii=@Observatii WHERE Id=@Id";
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Nume", TxtNume.Text); cmd.Parameters.AddWithValue("@Telefon", TxtTelefon.Text);
                    cmd.Parameters.AddWithValue("@Email", TxtEmail.Text); cmd.Parameters.AddWithValue("@Rama", TxtRama.Text);
                    cmd.Parameters.AddWithValue("@TipLentila", TxtTipLentila.Text); cmd.Parameters.AddWithValue("@Dioptrii", dioptrii);
                    cmd.Parameters.AddWithValue("@Observatii", TxtObservatii.Text); cmd.Parameters.AddWithValue("@Id", idPacientCurent);
                    cmd.ExecuteNonQuery();
                }
            }
            MessageBox.Show("Actualizat cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information); this.Close();
        }
    }
}