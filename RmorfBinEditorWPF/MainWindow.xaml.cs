using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace RmorfBinEditorWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        RmorfBinHead rhead;
        List<RmorfBinGroup> rgroup;

        byte[] fSize = { 0x00, 0x00, 0x00, 0x00 };
        byte[] key = { 0xCD, 0xCC, 0xCC, 0x3D };
        byte[] aGroupCount = { 0x00, 0x00, 0x00, 0x00 };
        byte[] endofFile = { 0x00, 0x00, 0x00 };

        private string path; // For general use (save file, open file, create file, etc.)
        private uint headfS, headKey, headaGC; // RmorfBinHead preferences
        private uint grMc, grTOA, grFrq, grU3, grU4, grU5; // RmorfBinGroup preferences
        private List<string> obj_nameslist; // For storing object names' list

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateFile_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog cofd = new CommonOpenFileDialog();
            cofd.IsFolderPicker = true;
            cofd.Title = "Choose path to create a file";

            if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                path = cofd.FileName + "\\rmorf.bin";

                try
                {
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        headfS = BitConverter.ToUInt32(fSize, 0);
                        headKey = BitConverter.ToUInt32(key, 0);
                        headaGC = BitConverter.ToUInt32(aGroupCount, 0);
                        rhead = new RmorfBinHead(headfS, headKey, headaGC);
                        rgroup = new List<RmorfBinGroup>();

                        bw.Write(headfS);
                        bw.Write(headKey);
                        bw.Write(headaGC);

                        // Writing size of file on its begin and save it
                        bw.Seek(0, SeekOrigin.End);
                        bw.Write(endofFile);
                        bw.Seek(0, SeekOrigin.Begin);

                        FileInfo file = new FileInfo(path);
                        long size = file.Length;
                        byte[] sizeout = BitConverter.GetBytes(size);
                        bw.Write(sizeout, 0, 4);

                    }
                } catch (FormatException)
                {
                    MessageBox.Show("Wrong type of data!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "rmorf.bin file(*.bin)| *.bin | All Files(*.*) | *.* ";
            ofd.Title = "Open File";

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Filter = "rmorf.bin file(*.bin)| *.bin | All Files(*.*) | *.* ";

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

            }
        }

        private void VisualizeGroup()
        {
            ObjectsList.Items.Clear();

            if (rgroup != null)
            {
                for (int i = 0; i < rgroup.Count; i++)
                {
                    ObjectsList.Items.Add((i + 1).ToString());
                }
            }

            CountOfObjects.Text = string.Empty;
            TypeOfAnim.Text = string.Empty;
            CountOfObjects.Text = string.Empty;
            CountOfObjects.Text = string.Empty;
            CountOfObjects.Text = string.Empty;
        }

        private void Authors_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"rmorf.bin Editor v1.0\nAuthors: Firefox3860, Smelson and Legion.\n(c) {DateTime.Now.Year}. From Russia and Kazakhstan with love!", "About Us",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
