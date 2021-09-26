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

namespace Updater
{
    public partial class Main : Form
    {
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

                WC.DownloadFile("https://github.com/legion2809/RmorfBinEditorWPF/blob/main/RmorfBinEditorWPF/Version%20for%20Updater/Rmorf.bin%20Editor.exe?raw=true", "Rmorf.bin Editor New.exe");

                File.Move("Rmorf.bin Editor.exe", "Rmorf.bin Editor.exe.old");

                File.Move("Rmorf.bin Editor New.exe", "Rmorf.bin Editor.exe");

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
