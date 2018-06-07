using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using NetEditor.ViewModels;

namespace NetEditor
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        /// <summary>
        /// Initializes a log window.
        /// </summary>
        public LogWindow(LogViewModel lvm)
        {
            InitializeComponent();
            DataContext = lvm;
        }

        /// <summary>
        /// Closes window.
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Clears the current log.
        /// </summary>
        private void Clear_OnClick(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = MessageBox.Show("Are you sure? All the records will be deleted.",
                    "Clear Log", MessageBoxButton.OKCancel);
            if (messageBoxResult == MessageBoxResult.OK)
            {
                var lvm = DataContext as LogViewModel;
                if (lvm != null)
                {
                    lvm.ClearLog();
                }
            }
        }

        /// <summary>
        /// Exports the log to a file.
        /// </summary>
        private void Export_OnClick(object sender, RoutedEventArgs e)
        {
            var lvm = DataContext as LogViewModel;
            var sfd = new SaveFileDialog {
                OverwritePrompt = true,
                FileName = "pn_log",
                AddExtension = true,
                DefaultExt = "log",
                Filter = "Log Files|*.log|Text Files|*.txt|All Files|*.*"
            };

            if (sfd.ShowDialog() == true) {
                try {
                    using (var sw = new StreamWriter(sfd.FileName))
                    {
                        sw.Write(LogTextBox.Text);
                        if (lvm != null) {
                            lvm.MakeRecord("Log exported to " + sfd.FileName + ".");
                        }
                    }
                } catch (Exception ex) {
                    MessageBox.Show("Error: " + ex.Message, "Export Error");
                }

            }
        }
    }
}
