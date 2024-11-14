using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Globalization;

namespace RmorfBinEditorWPF
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly List<CultureInfo> m_Languages = new List<CultureInfo>();

        // Global exception handling
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"An error occured: {e.Exception.Message.ToString()}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        #region App localization-related code
        public static List<CultureInfo> Languages {
            get {
                return m_Languages;
            }
        }

        public static event EventHandler LanguageChanged;

        public App()
        {
            InitializeComponent();
            LanguageChanged += App_LanguageChanged;

            m_Languages.Clear();
            m_Languages.Add(new CultureInfo("en-US"));
            m_Languages.Add(new CultureInfo("ru-RU"));

            Language = RmorfBinEditorWPF.Properties.Settings.Default.DefaultLanguage;
        }

        public static CultureInfo Language {
            get {
                return System.Threading.Thread.CurrentThread.CurrentUICulture;
            }
            set {
                if (value == null) throw new ArgumentNullException("value");
                if (value == System.Threading.Thread.CurrentThread.CurrentUICulture) return;

                // Changing the app language:
                System.Threading.Thread.CurrentThread.CurrentUICulture = value;

                // Creating ResourceDictionary for new culture
                ResourceDictionary dict = new ResourceDictionary();
                switch (value.Name) {
                    case "ru-RU":
                        dict.Source = new Uri(string.Format("Resources/Lang.{0}.xaml", value.Name), UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("Resources/Lang.xaml", UriKind.Relative);
                        break;
                }

                // Finding old ResourceDictoinary, deleting it and adding new ResourceDictionary
                ResourceDictionary oldDict = (from d in Application.Current.Resources.MergedDictionaries
                                              where d.Source != null && d.Source.OriginalString.StartsWith("Resources/Lang.")
                                              select d).First();
                if (oldDict != null) {
                    int ind = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                    Application.Current.Resources.MergedDictionaries.Insert(ind, dict);
                }
                else {
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                }

                // Calling the event for alerting every window in app
                LanguageChanged(Application.Current, new EventArgs());
            }
        }

        private void App_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Language = RmorfBinEditorWPF.Properties.Settings.Default.DefaultLanguage;
        }

        private void App_LanguageChanged(object sender, EventArgs e)
        {
            RmorfBinEditorWPF.Properties.Settings.Default.DefaultLanguage = Language;
            RmorfBinEditorWPF.Properties.Settings.Default.Save();
        }
        #endregion
    }
}
