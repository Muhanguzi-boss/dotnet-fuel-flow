namespace PetrolStationMS;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        try { Db.LoadConfig(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

        while (true)
        {
            using (var login = new LoginForm())
                if (login.ShowDialog() != DialogResult.OK) return;
            var main = new MainForm();
            Application.Run(main);
            if (!main.LogoutRequested) return;
        }
    }
}
