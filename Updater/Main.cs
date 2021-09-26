using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Ionic.Zip;

namespace Updater
{
    public partial class Main : Form
    {
        string pastebin;
        string link;
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            try 
            {
                this.Visible = false;

                WebClient WC = new WebClient();

                pastebin = WC.DownloadString("https://pastebin.com/raw/Rn6NX04n");

                dynamic json = JsonConvert.DeserializeObject(pastebin);
                link = json["link"];

                WC.DownloadFile(link, "Rmorf.bin Editor.zip");

                using (ZipFile zip = new ZipFile("Rmorf.bin Editor.zip")) {
                    zip.ExtractAll(@".", ExtractExistingFileAction.OverwriteSilently);
                }

                File.Delete("Rmorf.bin Editor.zip");

                Process.Start("Rmorf.bin Editor.exe");

                Application.Exit();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
    }
}
