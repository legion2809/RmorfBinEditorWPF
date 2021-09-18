using System;
using System.Collections.Generic;
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

        private void Authors_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Rmorf.bin Editor v 1.0\nAuthors: Firefox3860, Smelson and Legion.\n(c) {DateTime.Now.Year}. From Russia and Kazakhstan with love!", "About Us", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
