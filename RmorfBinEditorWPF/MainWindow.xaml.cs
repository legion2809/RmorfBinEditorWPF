using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.VisualBasic;
using System.Windows.Input;
using System.Windows.Controls;
using System.Net;
using System.Diagnostics;

namespace RmorfBinEditorWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string CurrentVersion = "Beta 0.1";
        string NewVersion = null;

        RmorfBinHead rhead;
        List<RmorfBinGroup> rgrouplist;

        Discord.Discord discord = new Discord.Discord();
        long time;

        byte[] fSize = { 0x00, 0x00, 0x00, 0x00 };
        byte[] key = { 0xCD, 0xCC, 0xCC, 0x3D };
        byte[] aGroupCount = { 0x00, 0x00, 0x00, 0x00 };
        byte[] endofFile = { 0x00, 0x00, 0x00 };

        private string path; // For general use (save file, open file, create file, etc.)
        private uint headfS, headKey, headaGC; // RmorfBinHead preferences
        private uint grMc, grTOA, grFrq, grU3, grU4, grU5; // RmorfBinGroup preferences
        private List<string> obj_nameslist; // For storing object names' list
        private bool isFileChanged; // For logging file's changing

        #region Shortcut keys for "Save", "Open" and etc.
        public static RoutedCommand exitCommand = new RoutedCommand();
        public static RoutedCommand saveAsCommand = new RoutedCommand();

        public static RoutedCommand insertGroup = new RoutedCommand();
        public static RoutedCommand insertObject = new RoutedCommand();

        public static RoutedCommand aboutCommand = new RoutedCommand();

        public MainWindow()
        {
            InitializeComponent();

            this.Title = $"Rmorf.bin Editor {CurrentVersion}";

            exitCommand.InputGestures.Add(new KeyGesture(Key.F4, ModifierKeys.Alt));
            aboutCommand.InputGestures.Add(new KeyGesture(Key.F1));

            insertGroup.InputGestures.Add(new KeyGesture(Key.G, ModifierKeys.Alt));
            insertObject.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Alt));
        }

        // "New File..."
        private void NewFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FileNew.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        // Open file
        private void OpenFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FileOpen.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        // Save file
        private void SaveFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Save.IsEnabled == false) {
                e.CanExecute = false;
            }
            else {
                e.CanExecute = true;
            }
        }

        private void SaveFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        // Save As
        private void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Save.IsEnabled == false) {
                e.CanExecute = false;
            }
            else {
                e.CanExecute = true;
            }
        }

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAs.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        // Insert group
        private void InsertGroup_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Save.IsEnabled == false) {
                e.CanExecute = false;
            }
            else {
                e.CanExecute = true;
            }
        }

        private void InsertGroup_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertGroup.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        // Insert object
        private void InsertObject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Save.IsEnabled == false) {
                e.CanExecute = false;
            }
            else {
                e.CanExecute = true;
            }
        }

        private void InsertObject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertObject.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        // About
        private void About_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void About_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AboutUs.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        // Exit from app
        private void Exit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Exit.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }
        #endregion

        #region Discord Rich Presence Integration and Background Images
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //CheckUpdatesMethod();

            discord.Initialize();

            discord.presence.largeImageKey = "logo";
            discord.presence.largeImageText = "Rmorf.bin Editor";

            time = DateTimeOffset.Now.ToUnixTimeSeconds();
            discord.presence.startTimestamp = time;

            discord.UpdatePresence("Idling");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            discord.Shutdown();
        }
        #endregion

        #region Creating, opening and saving a file
        private void CreateFile()
        {
            CommonOpenFileDialog cofd = new CommonOpenFileDialog();
            cofd.IsFolderPicker = true;
            cofd.Title = "Choose path to create a file";

            if (cofd.ShowDialog() == CommonFileDialogResult.Ok) {
                path = cofd.FileName + "\\rmorf.bin";

                try {
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    using (BinaryWriter bw = new BinaryWriter(fs)) {
                        headfS = BitConverter.ToUInt32(fSize, 0);
                        headKey = BitConverter.ToUInt32(key, 0);
                        headaGC = BitConverter.ToUInt32(aGroupCount, 0);
                        rhead = new RmorfBinHead(headfS, headKey, headaGC);
                        rgrouplist = new List<RmorfBinGroup>();

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
                    StatusLabel.Content = $"New file created, its location - ({path})";

                    EnableButtons();

                    discord.UpdatePresence("Editing a file");
                }
                catch (FormatException) {
                    MessageBox.Show("Wrong type of data!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    ErrorCatched();
                }
            }

            ObjectsList.Items.Clear();
            PresetsBox.Text = "Unknown/None";
            VisualizeGroup();
        }

        private void OpenFile()
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Rmorf.bin file(*.bin)|*.bin|All Files(*.*)|*.*";
            ofd.Title = "Open File";

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                rgrouplist = new List<RmorfBinGroup>();
                path = ofd.FileName;

                try {
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    using (BinaryReader br = new BinaryReader(fs)) {
                        headfS = br.ReadUInt32();
                        headKey = br.ReadUInt32();

                        if (headKey == 1036831949) {
                            try {
                                headaGC = br.ReadUInt32();
                                rhead = new RmorfBinHead(headfS, headKey, headaGC);

                                for (int animgr = 0; animgr < headaGC; animgr++) {
                                    obj_nameslist = new List<string>();
                                    grMc = br.ReadUInt32();
                                    grTOA = br.ReadUInt32();
                                    grFrq = br.ReadUInt32();
                                    grU3 = br.ReadUInt32();
                                    grU4 = br.ReadUInt32();
                                    grU5 = br.ReadUInt32();

                                    for (uint i = 0; i < grMc; i++) {
                                        List<byte> list = new List<byte>();
                                        byte[] arr;

                                        while (br.PeekChar() != '\x00') {
                                            list.Add(br.ReadByte());
                                        }

                                        arr = list.ToArray();
                                        br.BaseStream.Seek(+1, SeekOrigin.Current);
                                        string name = Encoding.ASCII.GetString(arr);
                                        obj_nameslist.Add(name);
                                    }

                                    rgrouplist.Add(new RmorfBinGroup(grMc, grTOA, grFrq, grU3, grU4, grU5, obj_nameslist));
                                }

                                StatusLabel.Content = $"File opened - ({path})";

                                EnableButtons();

                                discord.UpdatePresence("Editing a file");
                            }
                            catch {
                                MessageBox.Show("Couldn't parse!\nMaybe, you opened wrong file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                ErrorCatched();
                            }
                        }
                        else {
                            MessageBox.Show("File is wrong!.\nWrong constant.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                            ErrorCatched();
                        }
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    ErrorCatched();
                }
            }

            ObjectsList.Items.Clear();
            PresetsBox.Text = "Unknown/None";
            VisualizeGroup();
        }

        private void SaveFile()
        {
            try {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                using (BinaryWriter bw = new BinaryWriter(fs)) {
                    bw.Write(headfS);
                    bw.Write(headKey);
                    bw.Write(headaGC);

                    for (int i = 0; i < rgrouplist.Count; i++) {
                        bw.Write(rgrouplist[i].morfCount);
                        bw.Write(rgrouplist[i].animType);
                        bw.Write(rgrouplist[i].animFrequency);
                        bw.Write(rgrouplist[i].unknown3);
                        bw.Write(rgrouplist[i].unknown4);
                        bw.Write(rgrouplist[i].unknown5);

                        for (int j = 0; j < rgrouplist[i].objNames.Count; j++) {
                            string name = rgrouplist[i].objNames[j];
                            byte[] arr = Encoding.ASCII.GetBytes(name);

                            bw.Write(arr);
                            bw.Write(rgrouplist[i].nullb);
                        }
                    }

                    bw.Seek(0, SeekOrigin.End);
                    bw.Write(endofFile);
                    bw.Seek(0, SeekOrigin.Begin);

                    FileInfo info = new FileInfo(path);
                    long size = info.Length;
                    byte[] sizeout = BitConverter.GetBytes(size);

                    bw.Write(sizeout, 0, 4);
                }
                isFileChanged = false;
                StatusLabel.Content = $"File saved - ({path})";
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateFile_Click(object sender, RoutedEventArgs e)
        {
            if (isFileChanged) {
                var res = MessageBox.Show("Do you wish save changes to " + path + "?",
                    "rmorf.bin Editor", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (res) {
                    case MessageBoxResult.Yes:
                        SaveFile();
                        OpenFile();
                        break;
                    case MessageBoxResult.No:
                        CreateFile();
                        break;
                }
            }
            else {
                CreateFile();
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (isFileChanged) {
                var res = MessageBox.Show("Do you wish save changes to " + path + "?",
                    "rmorf.bin Editor", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (res) {
                    case MessageBoxResult.Yes:
                        SaveFile();
                        OpenFile();
                        break;
                    case MessageBoxResult.No:
                        OpenFile();
                        break;
                }
            }
            else {
                OpenFile();
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void SaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Filter = "rmorf.bin file(*.bin)|*.bin|All Files(*.*)|*.*";
            sfd.Title = "Save As";

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                path = sfd.FileName;
                try {
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    using (BinaryWriter bw = new BinaryWriter(fs)) {
                        bw.Write(headfS);
                        bw.Write(headKey);
                        bw.Write(headaGC);

                        for (int i = 0; i < rgrouplist.Count; i++) {
                            bw.Write(rgrouplist[i].morfCount);
                            bw.Write(rgrouplist[i].animType);
                            bw.Write(rgrouplist[i].animFrequency);
                            bw.Write(rgrouplist[i].unknown3);
                            bw.Write(rgrouplist[i].unknown4);
                            bw.Write(rgrouplist[i].unknown5);

                            for (int j = 0; j < rgrouplist[i].objNames.Count; j++) {
                                string name = rgrouplist[i].objNames[j];
                                byte[] arr = Encoding.ASCII.GetBytes(name);

                                bw.Write(arr);
                                bw.Write(rgrouplist[i].nullb);
                            }
                        }

                        bw.Seek(0, SeekOrigin.End);
                        bw.Write(endofFile);
                        bw.Seek(0, SeekOrigin.Begin);

                        FileInfo info = new FileInfo(path);
                        long size = info.Length;
                        byte[] sizeout = BitConverter.GetBytes(size);

                        bw.Write(sizeout, 0, 4);
                    }

                    isFileChanged = false;
                    StatusLabel.Content = $"File successfully saved, its location - ({path})";
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #region Enabling/Disabling control buttons (depends on result of reading a file)
        private void EnableButtons()
        {
            ApplySettingsButton.IsEnabled = true;
            Save.IsEnabled = true;
            SaveAs.IsEnabled = true;
            InsertGroup.IsEnabled = true;
            InsertObject.IsEnabled = true;
        }

        private void ErrorCatched()
        {
            rgrouplist = null;
            StatusLabel.Content = string.Empty;
            isFileChanged = false;

            ApplySettingsButton.IsEnabled = false;
            Save.IsEnabled = false;
            SaveAs.IsEnabled = false;
            InsertGroup.IsEnabled = false;
            InsertObject.IsEnabled = false;

            discord.UpdatePresence("Idling");
        }
        #endregion

        #endregion

        #region Inserting, deleting groups and objects (+ renaming objects)
        private void InsertGroup_Click(object sender, RoutedEventArgs e)
        {
            if (rgrouplist != null) {
                rgrouplist.Add(new RmorfBinGroup(0, 0, 0, 0, 0, 0, new List<string>()));
                headaGC = (uint)rgrouplist.Count;

                VisualizeGroup();

                ObjectsList.Items.Clear();
                PresetsBox.Text = "Unknown/None";
                isFileChanged = true;
                StatusLabel.Content = $"File changed - ({path}*)";

                discord.UpdatePresence("Editing a file");
            }
        }

        // Getting "Rmorf" group preferences (morf counts, type of anim, frequency of anim, etc.)
        private void GetRmorfGroupPreferences(int i)
        {
            grMc = (uint)obj_nameslist.Count;
            grTOA = rgrouplist[i].animType;
            grFrq = rgrouplist[i].animFrequency;
            grU3 = rgrouplist[i].unknown3;
            grU4 = rgrouplist[i].unknown4;
            grU5 = rgrouplist[i].unknown5;
        }

        private void InsertObject_Click(object sender, RoutedEventArgs e)
        {
            int obj = GroupsList.SelectedIndex;

            if (rgrouplist != null && obj >= 0) {
                string objname = Interaction.InputBox("Type the name of the new object:", "Insert Object", "Scene2.bin Object.Morfable Mesh");
                if (objname == "") {
                    MessageBox.Show("You haven't typed the name of the object!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else {
                    obj_nameslist = rgrouplist[obj].objNames;
                    obj_nameslist.Add(objname);
                    GetRmorfGroupPreferences(obj);
                    rgrouplist[obj] = new RmorfBinGroup(grMc, grTOA, grFrq, grU3, grU4, grU5, obj_nameslist);

                    VisualizeObject(obj);

                    isFileChanged = true;
                    StatusLabel.Content = $"File changed - ({path})*";

                    discord.UpdatePresence("Editing a file");
                }
            }
        }

        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            int group = GroupsList.SelectedIndex;

            if (rgrouplist != null && group >= 0) {
                rgrouplist.RemoveAt(group);
                headaGC = (uint)rgrouplist.Count;

                VisualizeGroup();

                ObjectsList.Items.Clear();
                PresetsBox.Text = "Unknown/None";
                isFileChanged = true;
                StatusLabel.Content = $"File changed - ({path})*";
            }
        }

        private void DeleteObject_Click(object sender, RoutedEventArgs e)
        {
            int group = GroupsList.SelectedIndex;
            int obj = ObjectsList.SelectedIndex;

            if (rgrouplist != null && group >= 0 && obj >= 0) {
                obj_nameslist = rgrouplist[group].objNames;
                obj_nameslist.RemoveAt(obj);

                GetRmorfGroupPreferences(group);
                rgrouplist[group] = new RmorfBinGroup(grMc, grTOA, grFrq, grU3, grU4, grU5, obj_nameslist);

                VisualizeObject(group);

                isFileChanged = true;
                StatusLabel.Content = $"File changed - ({path})*";
            }
        }

        private void RenameObject_Click(object sender, RoutedEventArgs e)
        {
            int group = GroupsList.SelectedIndex;
            int obj = ObjectsList.SelectedIndex;

            if (rgrouplist != null && group >= 0 && obj >= 0) {
                string new_name = Interaction.InputBox("Type the new name of the object:", "Rename Object", ObjectsList.SelectedItem.ToString());
                if (new_name == null) {
                    MessageBox.Show("You haven't typed anything!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else {
                    rgrouplist[group].objNames[obj] = new_name;
                    VisualizeObject(group);
                    isFileChanged = true;
                    StatusLabel.Content = $"File changed - ({path})*";
                }
            }
        }
        #endregion

        #region Changing preferences in textboxes by the chosen preset
        private void ApplyPresetButton_Click(object sender, RoutedEventArgs e)
        {
            VisualizePresetsComboBox();
        }

        private void VisualizePresetsComboBox()
        {
            switch (PresetsBox.Text) {
                case "Flag":
                    TypeOfAnim.Text = "128";
                    AnimFrq.Text = "160";
                    Unk1.Text = "1";
                    Unk2.Text = "1";
                    Unk3.Text = "1";
                    break;

                case "Flag (Parnik)":
                    TypeOfAnim.Text = "128";
                    AnimFrq.Text = "170";
                    Unk1.Text = "1";
                    Unk2.Text = "1";
                    Unk3.Text = "1";
                    break;

                case "Flag (Parnik) #2":
                    TypeOfAnim.Text = "128";
                    AnimFrq.Text = "200";
                    Unk1.Text = "1";
                    Unk2.Text = "1";
                    Unk3.Text = "1";
                    break;

                case "Flag (Racing Circuit)":
                    TypeOfAnim.Text = "128";
                    AnimFrq.Text = "100";
                    Unk1.Text = "1";
                    Unk2.Text = "1";
                    Unk3.Text = "1";
                    break;

                case "Tree":
                    TypeOfAnim.Text = "0";
                    AnimFrq.Text = "500";
                    Unk1.Text = "1001";
                    Unk2.Text = "1";
                    Unk3.Text = "1";
                    break;

                case "Tree #2":
                    TypeOfAnim.Text = "0";
                    AnimFrq.Text = "500";
                    Unk1.Text = "1001";
                    Unk2.Text = "0";
                    Unk3.Text = "0";
                    break;

                case "Tree #3":
                    TypeOfAnim.Text = "0";
                    AnimFrq.Text = "800";
                    Unk1.Text = "401";
                    Unk2.Text = "1";
                    Unk3.Text = "1";
                    break;

                case "Spruce":
                    TypeOfAnim.Text = "0";
                    AnimFrq.Text = "400";
                    Unk1.Text = "1001";
                    Unk2.Text = "1";
                    Unk3.Text = "1";
                    break;

                case "Water/Curtain":
                    TypeOfAnim.Text = "128";
                    AnimFrq.Text = "1000";
                    Unk1.Text = "1";
                    Unk2.Text = "1";
                    Unk3.Text = "1";
                    break;

                case "Water #2":
                    TypeOfAnim.Text = "1";
                    AnimFrq.Text = "2000";
                    Unk1.Text = "1";
                    Unk2.Text = "1";
                    Unk3.Text = "1";
                    break;

                case "Clothes":
                    TypeOfAnim.Text = "129";
                    AnimFrq.Text = "1000";
                    Unk1.Text = "1001";
                    Unk2.Text = "1";
                    Unk3.Text = "1";
                    break;

                case "Clothes #2":
                    TypeOfAnim.Text = "0";
                    AnimFrq.Text = "1000";
                    Unk1.Text = "1";
                    Unk2.Text = "1";
                    Unk3.Text = "1";
                    break;

                case "Clothes (Strong Wind)":
                    TypeOfAnim.Text = "0";
                    AnimFrq.Text = "100";
                    Unk1.Text = "301";
                    Unk2.Text = "1";
                    Unk3.Text = "1";
                    break;

                case "Clothes (Strong Wind) #2":
                    TypeOfAnim.Text = "128";
                    AnimFrq.Text = "200";
                    Unk1.Text = "601";
                    Unk2.Text = "1";
                    Unk3.Text = "1";
                    break;

                case "Signboard":
                    TypeOfAnim.Text = "0";
                    AnimFrq.Text = "1000";
                    Unk1.Text = "201";
                    Unk2.Text = "1";
                    Unk3.Text = "1";
                    break;

                case "Signboard #2":
                    TypeOfAnim.Text = "0";
                    AnimFrq.Text = "800";
                    Unk1.Text = "601";
                    Unk2.Text = "1";
                    Unk3.Text = "1";
                    break;

                case "Truck (MISE09)":
                    TypeOfAnim.Text = "0";
                    AnimFrq.Text = "150";
                    Unk1.Text = "51";
                    Unk2.Text = "0";
                    Unk3.Text = "1";
                    break;

                case "Unknown/None":
                    TypeOfAnim.Text = "0";
                    AnimFrq.Text = "0";
                    Unk1.Text = "0";
                    Unk2.Text = "0";
                    Unk3.Text = "0";
                    break;
            }
        }
        #endregion

        #region Write the settings into the selected object
        private void ApplyPresetSettings_Click(object sender, RoutedEventArgs e)
        {
            ApplyPresetSettings();
        }

        private void ApplyPresetSettings()
        {
            int obj = GroupsList.SelectedIndex;

            if (obj >= 0) {
                try {
                    grMc = (uint)rgrouplist[obj].objNames.Count;
                    grTOA = uint.Parse(TypeOfAnim.Text);
                    grFrq = uint.Parse(AnimFrq.Text);
                    grU3 = uint.Parse(Unk1.Text);
                    grU4 = uint.Parse(Unk2.Text);
                    grU5 = uint.Parse(Unk3.Text);

                    obj_nameslist = rgrouplist[obj].objNames;
                    rgrouplist[obj] = new RmorfBinGroup(grMc, grTOA, grFrq, grU3, grU4, grU5, obj_nameslist);

                    VisualizeComboBox();

                    isFileChanged = true;
                    StatusLabel.Content = $"File changed - ({path})*";
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        #region Visualizing groups, preset and objects values onto GUI

        // When you're switching between created groups
        private void GroupsList_SelectionChanged(object sender, RoutedEventArgs e)
        {
            VisualizeObject(GroupsList.SelectedIndex);
        }

        private void VisualizeGroup()
        {
            GroupsList.Items.Clear();

            if (rgrouplist != null) {
                for (int i = 0; i < rgrouplist.Count; i++) {
                    GroupsList.Items.Add("Group №" + (i + 1).ToString());
                }
            }

            CountOfObjects.Text = string.Empty;
            TypeOfAnim.Text = string.Empty;
            AnimFrq.Text = string.Empty;
            Unk1.Text = string.Empty;
            Unk2.Text = string.Empty;
            Unk3.Text = string.Empty;
        }

        private void VisualizeObject(int selected)
        {
            ObjectsList.Items.Clear();
            PresetsBox.Text = "Unknown/None";

            if (selected >= 0 && rgrouplist[selected].objNames != null) {
                for (int i = 0; i < rgrouplist[selected].objNames.Count; i++) {
                    ObjectsList.Items.Add(rgrouplist[selected].objNames[i]);
                }

                CountOfObjects.Text = rgrouplist[selected].morfCount.ToString();
                TypeOfAnim.Text = rgrouplist[selected].animType.ToString();
                AnimFrq.Text = rgrouplist[selected].animFrequency.ToString();
                Unk1.Text = rgrouplist[selected].unknown3.ToString();
                Unk2.Text = rgrouplist[selected].unknown4.ToString();
                Unk3.Text = rgrouplist[selected].unknown5.ToString();

                VisualizeComboBox();
            }
        }

        private void VisualizeComboBox()
        {
            if (TypeOfAnim.Text == "128" && AnimFrq.Text == "160" && Unk1.Text == "1" && Unk2.Text == "1" && Unk3.Text == "1") {
                PresetsBox.Text = "Flag";
            }

            if (TypeOfAnim.Text == "128" && AnimFrq.Text == "170" && Unk1.Text == "1" && Unk2.Text == "1" && Unk3.Text == "1") {
                PresetsBox.Text = "Flag (Parnik)";
            }

            if (TypeOfAnim.Text == "128" && AnimFrq.Text == "200" && Unk1.Text == "1" && Unk2.Text == "1" && Unk3.Text == "1") {
                PresetsBox.Text = "Flag (Parnik) #2";
            }

            if (TypeOfAnim.Text == "128" && AnimFrq.Text == "100" && Unk1.Text == "1" && Unk2.Text == "1" && Unk3.Text == "1") {
                PresetsBox.Text = "Flag (Racing Circuit)";
            }

            if (TypeOfAnim.Text == "0" && AnimFrq.Text == "500" && Unk1.Text == "1001" && Unk2.Text == "0" && Unk3.Text == "0") {
                PresetsBox.Text = "Tree";
            }

            if (TypeOfAnim.Text == "0" && AnimFrq.Text == "500" && Unk1.Text == "1001" && Unk2.Text == "1" && Unk3.Text == "1") {
                PresetsBox.Text = "Tree #2";
            }

            if (TypeOfAnim.Text == "0" && AnimFrq.Text == "800" && Unk1.Text == "401" && Unk2.Text == "1" && Unk3.Text == "1") {
                PresetsBox.Text = "Tree #3";
            }

            if (TypeOfAnim.Text == "0" && AnimFrq.Text == "400" && Unk1.Text == "1001" && Unk2.Text == "1" && Unk3.Text == "1") {
                PresetsBox.Text = "Spruce";
            }

            if (TypeOfAnim.Text == "128" && AnimFrq.Text == "1000" && Unk1.Text == "1" && Unk2.Text == "1" && Unk3.Text == "1") {
                PresetsBox.Text = "Water/Curtain";
            }

            if (TypeOfAnim.Text == "1" && AnimFrq.Text == "2000" && Unk1.Text == "1" && Unk2.Text == "1" && Unk3.Text == "1") {
                PresetsBox.Text = "Water #2";
            }

            if (TypeOfAnim.Text == "129" && AnimFrq.Text == "1000" && Unk1.Text == "1001" && Unk2.Text == "1" && Unk3.Text == "1") {
                PresetsBox.Text = "Clothes";
            }

            if (TypeOfAnim.Text == "0" && AnimFrq.Text == "1000" && Unk1.Text == "1" && Unk2.Text == "1" && Unk3.Text == "1") {
                PresetsBox.Text = "Clothes #2";
            }

            if (TypeOfAnim.Text == "0" && AnimFrq.Text == "100" && Unk1.Text == "301" && Unk2.Text == "1" && Unk3.Text == "1") {
                PresetsBox.Text = "Clothes (Strong Wind)";
            }

            if (TypeOfAnim.Text == "128" && AnimFrq.Text == "200" && Unk1.Text == "601" && Unk2.Text == "1" && Unk3.Text == "1") {
                PresetsBox.Text = "Clothes (Strong Wind) #2";
            }

            if (TypeOfAnim.Text == "0" && AnimFrq.Text == "1000" && Unk1.Text == "201" && Unk2.Text == "1" && Unk3.Text == "1") {
                PresetsBox.Text = "Signboard";
            }

            if (TypeOfAnim.Text == "0" && AnimFrq.Text == "800" && Unk1.Text == "601" && Unk2.Text == "1" && Unk3.Text == "1") {
                PresetsBox.Text = "Signboard #2";
            }

            if (TypeOfAnim.Text == "0" && AnimFrq.Text == "150" && Unk1.Text == "51" && Unk2.Text == "0" && Unk3.Text == "1") {
                PresetsBox.Text = "Truck (MISE09)";
            }
        }
        #endregion

        #region "About us", "Check for updates" and "Exit"

        // "About us" section
        private void Authors_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Rmorf.bin Editor {CurrentVersion}\nAuthors: Firefox3860, Smelson and Legion.\n(c) {DateTime.Now.Year}. From Russia and Kazakhstan with love!", "About Us",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            CheckUpdatesMethod();
        }

        private void CheckUpdatesMethod()
        {
            try
            {
                WebClient WC = new WebClient();

                NewVersion = WC.DownloadString("https://pastebin.com/raw/Rn6NX04n");

                if (CurrentVersion != NewVersion)
                {
                    var DialogResult = MessageBox.Show($"Version {NewVersion} has been found! Do you want to update now?", $"Update to {NewVersion}", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    switch (DialogResult)
                    {
                        case MessageBoxResult.Yes:
                            Process.Start("Updater");
                            Application.Current.Shutdown();
                            break;

                        case MessageBoxResult.No:
                            break;
                    }
                }

                else
                {
                    MessageBox.Show($"You're using the latest version ({CurrentVersion}) of the program!", $"Rmorf.bin Editor {CurrentVersion}", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            catch
            {

            }
        }

        // Shutdown the app
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion
    }
}
