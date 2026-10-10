using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Microsoft.Win32;

namespace CabinetOftalmoWPF
{
    public class Pacient
    {
        public int Id { get; set; }
        public string Nume { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }
        public string Rama { get; set; }
        public string TipLentila { get; set; }
        public string Dioptrii { get; set; }
        public string Observatii { get; set; }
    }

    public partial class MainWindow : Window
    {
        string dbConnection = "Data Source=pacienti.db;Version=3;";

        public MainWindow()
        {
            InitializeComponent();
            InitializareBazaDeDate();
            IncarcaPacienti();
        }

        private void InitializareBazaDeDate()
        {
            using (SQLiteConnection conn = new SQLiteConnection(dbConnection))
            {
                conn.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS Pacienti (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT, Nume TEXT NOT NULL, Telefon TEXT, Email TEXT, 
                                Rama TEXT, TipLentila TEXT, Dioptrii TEXT, Observatii TEXT)";
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn)) { cmd.ExecuteNonQuery(); }

                string sqlIstoric = @"CREATE TABLE IF NOT EXISTS IstoricPacienti (
                                        Id INTEGER PRIMARY KEY AUTOINCREMENT, PacientId INTEGER, DataInregistrare TEXT, 
                                        Rama TEXT, TipLentila TEXT, Dioptrii TEXT, Observatii TEXT)";
                using (SQLiteCommand cmdIst = new SQLiteCommand(sqlIstoric, conn)) { cmdIst.ExecuteNonQuery(); }

                // Upgrade automat structură pentru baza de date existentă
                try { using (SQLiteCommand cmd = new SQLiteCommand("ALTER TABLE Pacienti RENAME COLUMN CodRame TO Rama", conn)) { cmd.ExecuteNonQuery(); } } catch { }
                try { using (SQLiteCommand cmd = new SQLiteCommand("ALTER TABLE Pacienti ADD COLUMN TipLentila TEXT", conn)) { cmd.ExecuteNonQuery(); } } catch { }
                try { using (SQLiteCommand cmd = new SQLiteCommand("ALTER TABLE Pacienti ADD COLUMN Observatii TEXT", conn)) { cmd.ExecuteNonQuery(); } } catch { }

                try { using (SQLiteCommand cmd = new SQLiteCommand("ALTER TABLE IstoricPacienti RENAME COLUMN CodRame TO Rama", conn)) { cmd.ExecuteNonQuery(); } } catch { }
                try { using (SQLiteCommand cmd = new SQLiteCommand("ALTER TABLE IstoricPacienti ADD COLUMN TipLentila TEXT", conn)) { cmd.ExecuteNonQuery(); } } catch { }
                try { using (SQLiteCommand cmd = new SQLiteCommand("ALTER TABLE IstoricPacienti ADD COLUMN Observatii TEXT", conn)) { cmd.ExecuteNonQuery(); } } catch { }
            }
        }

        private void IncarcaPacienti(string textCautat = "")
        {
            List<Pacient> listaPacienti = new List<Pacient>();
            using (SQLiteConnection conn = new SQLiteConnection(dbConnection))
            {
                conn.Open();
                string sql = "SELECT * FROM Pacienti";
                if (!string.IsNullOrWhiteSpace(textCautat)) sql = "SELECT * FROM Pacienti WHERE Nume LIKE @TextCautat";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    if (!string.IsNullOrWhiteSpace(textCautat)) cmd.Parameters.AddWithValue("@TextCautat", "%" + textCautat + "%");

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listaPacienti.Add(new Pacient
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Nume = reader["Nume"].ToString(),
                                Telefon = reader["Telefon"].ToString(),
                                Email = reader["Email"].ToString(),
                                Rama = reader["Rama"] != DBNull.Value ? reader["Rama"].ToString() : "",
                                TipLentila = reader["TipLentila"] != DBNull.Value ? reader["TipLentila"].ToString() : "",
                                Dioptrii = reader["Dioptrii"].ToString(),
                                Observatii = reader["Observatii"] != DBNull.Value ? reader["Observatii"].ToString() : ""
                            });
                        }
                    }
                }
            }
            TabelPacienti.ItemsSource = listaPacienti;
            TxtStatus.Text = $"Total pacienți afișați: {listaPacienti.Count}";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) { IncarcaPacienti(SearchBox.Text); }

        private void BtnAdaugaPacient_Click(object sender, RoutedEventArgs e) { new AdaugaPacientWindow().ShowDialog(); IncarcaPacienti(SearchBox.Text); }
        private void BtnConsultNou_Click(object sender, RoutedEventArgs e)
        {
            if (TabelPacienti.SelectedItem is Pacient p) { new ConsultNouWindow(p).ShowDialog(); IncarcaPacienti(SearchBox.Text); }
            else MessageBox.Show("Selectează un pacient din tabel!", "Atenție", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void BtnEditeazaPacient_Click(object sender, RoutedEventArgs e)
        {
            if (TabelPacienti.SelectedItem is Pacient p) { new EditeazaPacientWindow(p).ShowDialog(); IncarcaPacienti(SearchBox.Text); }
            else MessageBox.Show("Selectează un pacient din tabel!", "Atenție", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void BtnStergePacient_Click(object sender, RoutedEventArgs e)
        {
            if (TabelPacienti.SelectedItem is Pacient p)
            {
                if (MessageBox.Show($"Ștergi pacientul {p.Nume}?", "Confirmare", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(dbConnection))
                    {
                        conn.Open();
                        using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Pacienti WHERE Id = @Id", conn)) { cmd.Parameters.AddWithValue("@Id", p.Id); cmd.ExecuteNonQuery(); }
                        using (SQLiteCommand cmd2 = new SQLiteCommand("DELETE FROM IstoricPacienti WHERE PacientId = @Id", conn)) { cmd2.Parameters.AddWithValue("@Id", p.Id); cmd2.ExecuteNonQuery(); }
                    }
                    IncarcaPacienti(SearchBox.Text);
                }
            }
        }
        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog { Filter = "Fișiere Bază de Date (*.db)|*.db", FileName = $"backup_oftalmo_{DateTime.Now:dd_MM_yyyy}.db" };
                if (sfd.ShowDialog() == true) { File.Copy("pacienti.db", sfd.FileName, true); MessageBox.Show("Salvat cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog { Filter = "Fișiere Bază de Date (*.db)|*.db" };
                if (ofd.ShowDialog() == true && MessageBox.Show("Acțiunea suprascrie datele! Continui?", "Confirmare", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    SQLiteConnection.ClearAllPools();
                    File.Copy(ofd.FileName, "pacienti.db", true);
                    IncarcaPacienti(SearchBox.Text);
                    MessageBox.Show("Încărcat cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void TabelPacienti_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TabelPacienti.SelectedItem is Pacient p) new IstoricWindow(p).ShowDialog();
        }
    }
}