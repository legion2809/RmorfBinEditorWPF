using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using System.IO.Compression;

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

                Thread.Sleep(300);

                WebClient WC = new WebClient();

                pastebin = WC.DownloadString("https://pastebin.com/raw/Rn6NX04n");

                dynamic json = JsonConvert.DeserializeObject(pastebin);
                link = json["link"];

                WC.DownloadFile(link, "Rmorf.bin Editor.zip");

                ZipFile.ExtractToDirectory("Rmorf.bin Editor.zip", @"./extract");

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
