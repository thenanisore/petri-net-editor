using System;
using System.ComponentModel;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;
using PetriNetLib.NetStructure;
using NetEditor.Exceptions;
using NetEditor.ViewModels;
using NetEditor.Algorithms;

namespace NetEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _filePath = "";
        private string _titleFileName = "";

        private readonly LogViewModel _log = new LogViewModel();    // Log of the application.
        private bool _hasChanged;                                   // If hasChanged, saving file is needed.

        /// <summary>
        /// Initialize the main window.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;

            // Quit menuitem fires closing event.
            Closing += BtnQuit_OnClick;

            // Auto-layout on resizing.
            _resizeTimer.Elapsed += (sender, args) => Dispatcher.BeginInvoke(new Action(() => ResizingDone(sender, args)));
        }

        private Canvas _itemsPanelCanvas;   // Main canvas.

        /// <summary>
        /// Gets the main canvas when loaded.
        /// </summary>
        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            _itemsPanelCanvas = sender as Canvas;
        }

        /// <summary>
        /// Node dragging support.
        /// </summary>
        private void Thumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = (Thumb)sender;
            var myNode = (NodeViewModel)thumb.DataContext;

            if (myNode.X + e.HorizontalChange > 0
                && myNode.Y + e.VerticalChange > 0
                && myNode.X + myNode.Width + e.HorizontalChange < MainControl.ActualWidth
                && myNode.Y + myNode.Size + e.VerticalChange < MainControl.ActualHeight) {
                myNode.X += e.HorizontalChange;
                myNode.Y += e.VerticalChange;
            }
        }

        /// <summary>
        /// Loads a Petri net from the file.
        /// </summary>
        private void BtnOpenFile_OnClick(object sender, RoutedEventArgs e)
        {
            var openFile = new OpenFileDialog {
                Filter = "Petri Nets Files|*.npnets|XML Files|*.xml|All Files|*.*",
                DefaultExt = ".npnets",
                RestoreDirectory = true
            };

            if (openFile.ShowDialog() == true) {
                try
                {
                    var netDocument = XDocument.Load(openFile.FileName);
                    var npnets = netDocument.Root.Name.Namespace;
                    var netElement = netDocument.Element(npnets + "NPnetMarked").Element("net");
                    var netDiagram = netElement.Element("netDiagram");
                    var mvm = new MainViewModel(new Net(netElement, getMarking: true));

                    DataContext = mvm;
                    _filePath = openFile.FileName;
                    _titleFileName = openFile.SafeFileName;
                    this.Title = _titleFileName + " - Petri Net Editor";
                    RegisterEvent(_filePath + " opened.", "File Opened", false, true);

                    if (netDiagram != null)
                        try
                        {
                            mvm.NetVM.DeserializeDiagram(netDiagram);
                        }
                        catch (NetDiagramXmlException)
                        {
                            AutoLayout_OnClick(sender, e);
                        }
                    else
                        AutoLayout_OnClick(sender, e);
                }
                catch (XmlException ex)
                {
                    MessageBox.Show("Error: " + ex.Message + Environment.NewLine, "XML Error");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: cannot open the file." + Environment.NewLine, "Open File Error");
                }
            }
        }

        /// <summary>
        /// Saves file to the default location for this file.
        /// </summary>
        private void BtnSaveFile_OnClick(object sender, EventArgs e)
        {
            try {
                var mv = DataContext as MainViewModel;
                if (mv != null) {
                    if (_filePath != "") {
                        mv.NetVM.Serialize(true, true).Save(_filePath);
                        RegisterEvent("File saved as " + _filePath + ".", "File Saved", false, false);
                    } else
                        BtnSaveFileAs_OnClick(sender, e);
                }
            } catch (Exception ex) {
                MessageBox.Show("Error: " + ex.Message, "Save File Error");
            }
        }

        /// <summary>
        /// Saves file in a certain location.
        /// </summary>
        private void BtnSaveFileAs_OnClick(object sender, EventArgs e)
        {
            if (_titleFileName == "")
                _titleFileName = "mynet";

            var sfd = new SaveFileDialog {
                OverwritePrompt = true,
                FileName = _titleFileName,
                AddExtension = true,
                DefaultExt = "npnets",
                Filter = "Petri Nets Files|*.npnets|XML Files|*.xml|All Files|*.*"
            };

            if (_filePath != "")
                sfd.InitialDirectory = System.IO.Path.GetDirectoryName(_filePath);

            if (sfd.ShowDialog() == true) {
                try {
                    var mv = DataContext as MainViewModel;
                    if (mv != null) mv.NetVM.Serialize(true, true).Save(sfd.FileName);
                    _filePath = sfd.FileName;
                    RegisterEvent("File saved as " + _filePath + ".", "File Saved", false, false);
                    _titleFileName = System.IO.Path.GetFileNameWithoutExtension(sfd.SafeFileName);
                    this.Title = _titleFileName + " - " + this.Title;
                } catch (Exception ex) {
                    MessageBox.Show("Error: " + ex.Message, "Save File Error");
                }
            }
        }

        /// <summary>
        /// Closes the current net and open a new file.
        /// </summary>
        private void BtnNewFile_OnClick(object sender, RoutedEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null && !mv.NetVM.IsEmpty) {
                if (_hasChanged)
                {
                    var messageBoxResult =
                        MessageBox.Show("Save file" + (_titleFileName != "" ? " \"" + _titleFileName + "\"" : "") + "?",
                            "New File", System.Windows.MessageBoxButton.YesNoCancel);
                    if (messageBoxResult == MessageBoxResult.Yes || messageBoxResult == MessageBoxResult.No)
                    {
                        if (messageBoxResult == MessageBoxResult.Yes)
                            BtnSaveFileAs_OnClick(sender, e);
                        MakeNewFile();
                    }
                }
                else
                    MakeNewFile();
            }
        }

        /// <summary>
        /// Makes a new file.
        /// </summary>
        private void MakeNewFile()
        {
            DataContext = new MainViewModel();
            _titleFileName = "";
            Title = "Petri Net Editor";
            RegisterEvent("New file created.", "New File Created", false, true);
        }

        private void OpenExample_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Exports the net to a PNG image.
        /// </summary>
        private void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null) {
                if (mv.NetVM.IsEmpty) {
                    MessageBox.Show("Error: Nothing to export.", "Export Error");
                    return;
                }
                mv.NetVM.SelectedNode = null;
                mv.IsGrid = false;

                var _imageFileName = _titleFileName == "" ? "mynet" : _titleFileName;

                var sfd = new SaveFileDialog {
                    Title = "Import As",
                    OverwritePrompt = true,
                    FileName = _imageFileName,
                    AddExtension = true,
                    DefaultExt = "png",
                    Filter = "PNG Image|*.png|All Files|*.*"
                };

                if (_filePath != "")
                    sfd.InitialDirectory = System.IO.Path.GetDirectoryName(_filePath);

                if (sfd.ShowDialog() == true) {
                    try {
                        ExportImage(sfd.FileName);
                        RegisterEvent("Diagram exported as a PNG image to " + sfd.FileName + ".",
                            "Diagram Exported as PNG", _hasChanged, false);
                        mv.IsGrid = true;
                    } catch (Exception ex) {
                        MessageBox.Show("Error: " + ex.Message, "Export Error");
                    }
                }
            }
        }

        /// <summary>
        /// Quits from the application.
        /// </summary>
        private void BtnQuit_OnClick(object sender, EventArgs e)
        {
            if (sender is MenuItem) {
                Close();
                return;
            }

            var mv = DataContext as MainViewModel;
            if (mv != null && !mv.NetVM.IsEmpty && _hasChanged) {
                var messageBoxResult =
                    MessageBox.Show("Save file" + (_titleFileName != "" ? " \"" + _titleFileName + "\"" : "") + "?",
                        "Quit", System.Windows.MessageBoxButton.YesNoCancel);
                if (messageBoxResult == MessageBoxResult.Yes || messageBoxResult == MessageBoxResult.No) {
                    if (messageBoxResult == MessageBoxResult.Yes)
                        BtnSaveFile_OnClick(sender, e);
                    _log.MakeRecord("Application closed.");
                } else if (messageBoxResult == MessageBoxResult.Cancel) {
                    var args = e as CancelEventArgs;
                    if (args != null) {
                        args.Cancel = true;
                    }
                }
            }
        }

        #region netToolbar

        /// <summary>
        /// Deletes the selected node.
        /// </summary>
        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null)
            {
                var id = mv.NetVM.SelectedNode.Node.Id;
                var type = mv.NetVM.IsSelectedPlace ? "Place" : "Transition";
                mv.NetVM.DeleteSelectedNode();
                RegisterEvent(type + " deleted. Id: " + id + ".", type + " Deleted", true, true);
            }
        }

        /// <summary>
        /// Adds token to the selected place.
        /// </summary>
        private void BtnAddToken_OnClick(object sender, RoutedEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null) {
                var selected = mv.NetVM.SelectedNode as PlaceViewModel;
                if (selected != null)
                {
                    mv.NetVM.AddToken(selected);
                    RegisterEvent("Token added {" + selected.Tokens + "}. Place id: " +
                        selected.Node.Id + ".", "Token added", true, false);
                }
            }
        }

        /// <summary>
        /// Removes token of the selected place.
        /// </summary>
        private void BtnRemoveToken_OnClick(object sender, RoutedEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null) {
                var selected = mv.NetVM.SelectedNode as PlaceViewModel;
                if (selected != null)
                {
                    mv.NetVM.RemoveToken(selected);
                    RegisterEvent("Token removed {" + selected.Tokens + "}. Place id: " +
                        selected.Node.Id + ".", "Token Removed", true, false);
                }
            }
        }

        /// <summary>
        /// Clears the marking of the net.
        /// </summary>
        private void BtnClearMarking_OnClick(object sender, RoutedEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null) {
                mv.NetVM.RemoveMarking();
                RegisterEvent("Marking cleared.", "Marking Cleared", true, false);
            }
        }

        /// <summary>
        /// Fires transition.
        /// </summary>
        private void BtnFireTransition_OnClick(object sender, RoutedEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null) {
                var selected = mv.NetVM.SelectedNode as TransitionViewModel;
                if (selected != null) {
                    if (mv.NetVM.FireTransition(selected))
                        RegisterEvent("Transition fired. Id: " + selected.Node.Id + ".", "Transition fired.", true, false);
                }
            }
        }

        #endregion

        private object _clickSender;

        /// <summary>
        /// Remembers the sender of the mouse_down
        /// for some click imitation for nodes.
        /// </summary>
        private void Node_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _clickSender = sender;
        }

        /// <summary>
        /// Intercepts the right_mouse_down event if connect/disconnect nodes is selected.
        /// </summary>
        private void Node_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null && (mv.NetVM.IsConnectNode || mv.NetVM.IsDisconnectNode)) {
                e.Handled = true;
            }
            _clickSender = null;
        }

        /// <summary>
        /// Select node by right-clicking.
        /// </summary>
        private void Node_OnPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == _clickSender) {
                var mv = DataContext as MainViewModel;
                if (mv != null) {
                    var node = ((System.Windows.FrameworkElement)sender).DataContext as NodeViewModel;
                    mv.NetVM.SelectedNode = mv.NetVM.SelectedNode == node ? null : node;
                }
            }
        }

        /// <summary>
        /// Intercepts the mouse_down event if any toggle was selected.
        /// </summary>
        private void MainControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null && (mv.NetVM.IsNewPlace || mv.NetVM.IsNewTransition
                || mv.NetVM.IsConnectNode || mv.NetVM.IsDisconnectNode)) {
                _clickSender = sender;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Adds a new node to the main control.
        /// </summary>
        private void MainControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null && sender == _clickSender) {
                var p = e.GetPosition(this);
                var x = p.X;
                var y = p.Y;
                if (mv.NetVM.IsNewPlace) {
                    var np = mv.NetVM.AddPlace(x, y);
                    if (mv.IsGrid) mv.NetVM.GridNodes();
                    RegisterEvent("Place added. Id: " + np.Node.Id + ".", "Place Added", true, true);
                } else if (mv.NetVM.IsNewTransition) {
                    var nt =  mv.NetVM.AddTransition(x, y);
                    if (mv.IsGrid) mv.NetVM.GridNodes();
                    RegisterEvent("Transition added. Id: " + nt.Node.Id + ".", "Transition Added", true, true);
                }
            }
            _clickSender = null;
        }

        /// <summary>
        /// Connects or disconnects nodes.
        /// </summary>
        private void Node_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null && (mv.NetVM.IsConnectNode || mv.NetVM.IsDisconnectNode)) {
                var targetNode = ((System.Windows.FrameworkElement)sender).DataContext as NodeViewModel;
                if (mv.NetVM.IsConnectNode)
                    try {
                        mv.NetVM.ConnectNode(mv.NetVM.SelectedNode, targetNode);
                        RegisterEvent("Arc added. Source node id: " + mv.NetVM.SelectedNode.Node.Id +
                                        ". Target node id: " + targetNode.Node.Id + ".", "Nodes Connected", true, true);
                        mv.NetVM.IsConnectNode = false;
                    }
                    catch {
                         UpdateStatudBar("Nodes of the same type cannot be connected.");
                    }
                if (mv.NetVM.IsDisconnectNode)
                    try {
                        mv.NetVM.DisconnectNode(mv.NetVM.SelectedNode, targetNode);
                        RegisterEvent("Arc removed. Source node id: " + mv.NetVM.SelectedNode.Node.Id +
                            ". Target node id: " + targetNode.Node.Id + ".", "Nodes Disconnected", true, true);
                        mv.NetVM.IsDisconnectNode = false;
                    } catch {
                        UpdateStatudBar("Nodes of the same type cannot be connected.");
                    }
            }

            _clickSender = null;
        }

        /// <summary>
        /// Registers a certain event by making a log record,
        /// updating the status bar message and marking the file as changed.
        /// </summary>
        private void RegisterEvent(string logMessage, string statusBarMessage, bool hasChanged, bool reanalyze)
        {
            _log.MakeRecord(logMessage);
            UpdateStatudBar(statusBarMessage);
            _hasChanged = hasChanged;
            var mv = DataContext as MainViewModel;
            if (reanalyze && mv != null)
                mv.NetVM.IsAnalyzed = false;
        }

        /// <summary>
        /// Exports the diagram as an image file.
        /// </summary>
        private void ExportImage(string path)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null)
            {
                if (path == null) return;

                // Get the size of canvas
                Size size = new Size(_itemsPanelCanvas.ActualWidth, _itemsPanelCanvas.ActualHeight);

                var scale = 1;
                var bmp = new RenderTargetBitmap((int) (scale*(size.Width)),
                    (int) (scale*(size.Height)),
                    scale*96,
                    scale*96,
                    PixelFormats.Default);
                bmp.Render(_itemsPanelCanvas);
                var netArea = mv.NetVM.GetNetArea(size.Width, size.Height);
                var crop = new CroppedBitmap(bmp, netArea);

                // Save image to file
                var enc = new System.Windows.Media.Imaging.PngBitmapEncoder();
                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(crop));

                using (var stm = System.IO.File.Create(path))
                {
                    enc.Save(stm);
                }
            }
        }

        /// <summary>
        /// Timer for a status bar resetting.
        /// </summary>
        private readonly System.Timers.Timer _statusBarTimer = new System.Timers.Timer(2000);

        /// <summary>
        /// Updates status bar with a certain message.
        /// </summary>
        /// <param name="isPermanent">If false, message resets in 5 seconds.</param>
        private void UpdateStatudBar(string message, bool isPermanent = false)
        {
            _statusBarTimer.Enabled = false;
            StatusBlock.Text = message;
            if (isPermanent) return;
            _statusBarTimer.Elapsed += (sender, args) => Dispatcher.BeginInvoke(new Action(() => { StatusBlock.Text = "Ready"; }));
            _statusBarTimer.Enabled = true;
        }

        /// <summary>
        /// Opens a log window if checked.
        /// </summary>
        private void Log_OnChecked(object sender, RoutedEventArgs e)
        {
            var lw = new LogWindow(_log);
            // Prevent closing the main window with the log window opened.
            Closed += (o, args) => lw.Close();
            LogItem.Unchecked += (o, args) => lw.Close();
            lw.Closed += (o, args) => { LogItem.IsChecked = false; };
            lw.Show();
        }


        /* * * * * * * * * * * * * * * * * *
         *   Shortcuts support.
         * Here is the full list:
         * New File             -  Ctrl-N
         * Open File            -  Ctrl-O
         * Save File            -  Ctrl-S
         * Save File As         -  Ctrl-Shift-S
         * Export               -  Ctrl-E
         * Quit                 -  Ctrl-Q
         * Delete               -  Del
         * New Place            -  P
         * New Transition       -  T
         * Connect Node         -  C
         * Disconnect Node      -  Ctrl-C
         * Add Token            -  F
         * Remove Token         -  Ctrl-F
         * Clear Marking        -  Ctrl-M
         * Fire Transition      -  H
         * Open Application Log -  Ctrl-L
         * * * * * * * * * * * * * * * * * */
        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            switch (e.Key)
            {
                case Key.N:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl)) BtnNewFile_OnClick(sender, e);
                    break;
                case Key.O:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl)) BtnOpenFile_OnClick(sender, e);
                    break;
                case Key.S:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        if (Keyboard.IsKeyDown(Key.LeftShift))
                            BtnSaveFileAs_OnClick(sender, e);
                        else
                            BtnSaveFile_OnClick(sender, e);
                    }
                    break;
                case Key.E:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl)) BtnExport_OnClick(sender, e);
                    break;
                case Key.Q:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl)) BtnQuit_OnClick(QuitItem, e);
                    break;
                case Key.Delete:
                    if (BtnDelete.IsEnabled) BtnDelete_OnClick(sender, e);
                    break;
                case Key.P:
                    if (BtnAddPlace.IsEnabled) BtnAddPlace.IsChecked = !BtnAddPlace.IsChecked;
                    break;
                case Key.T:
                    if (BtnAddTransition.IsEnabled) BtnAddTransition.IsChecked = !BtnAddTransition.IsChecked;
                    break;
                case Key.C:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        if (BtnDisconnectNode.IsEnabled) BtnDisconnectNode.IsChecked = !BtnDisconnectNode.IsChecked;
                    } else
                        if (BtnConnectNode.IsEnabled) BtnConnectNode.IsChecked = !BtnConnectNode.IsChecked;
                    break;
                case Key.F:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        if (BtnRemoveToken.IsEnabled) BtnRemoveToken_OnClick(sender, e);
                    } else
                        if (BtnAddToken.IsEnabled) BtnAddToken_OnClick(sender, e);
                    break;
                case Key.M:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl)) BtnClearMarking_OnClick(sender, e);
                    break;
                case Key.L:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) && LogItem.IsEnabled) LogItem.IsChecked = !LogItem.IsChecked;
                    break;
                case Key.H:
                    if (BtnFireTransition.IsEnabled) BtnFireTransition_OnClick(sender, e);
                        break;
                case Key.Escape:
                    if (mv != null) mv.NetVM.ResetSelection();
                    break;
            }
        }

        private Thread currentLayoutThread;

        /// <summary>
        /// Automatically layouts the net.
        /// </summary>
        private void AutoLayout_OnClick(object sender, RoutedEventArgs e)
        {
            if (currentLayoutThread != null && currentLayoutThread.IsAlive)
                currentLayoutThread.Abort();

            var mv = DataContext as MainViewModel;
            if (mv != null && !mv.NetVM.IsEmpty)
            {
                mv.NetVM.UngridNodes();
                // Algorithm works in a different thread to improve perfomance
                // somewhat and animate the simulation.
                currentLayoutThread = new Thread(x => ForceBasedAlgorithm.Arrange(mv.NetVM,
                    _itemsPanelCanvas.ActualWidth, _itemsPanelCanvas.ActualHeight,
                    mv.IsGrid, _itemsPanelCanvas.ActualWidth, animate: true));
                currentLayoutThread.Start();
            }
        }

        /// <summary>
        /// Centralizes the net by shifting it to the center.
        /// </summary>
        private void Centralize_OnClick(object sender, RoutedEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null && !mv.NetVM.IsEmpty) {
                var fba = new Thread(x => mv.NetVM.CentralizeLayout(
                    _itemsPanelCanvas.ActualWidth, _itemsPanelCanvas.ActualHeight));
                fba.Start();
            }
        }

        /// <summary>
        /// Runs an analize and decomposition of the net.
        /// </summary>
        private void Analyze_Click(object sender, RoutedEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null && !mv.NetVM.IsEmpty) {
                var results = mv.NetVM.Analyze(BadHandlesCheckBox.IsChecked ?? false);

                // Show results of the analysis.
                MessageBox.Show(results, "Net analysis results", MessageBoxButton.OK, MessageBoxImage.Information);
                RegisterEvent(results, "Analysis completed.", _hasChanged, false);
            }
        }

        private TreeView _lastSelected;
        /// <summary>
        /// Get the last active treeview for proper highlighting selection.
        /// </summary>
        private void AnalysisTree_GetLastActive(object sender, RoutedEventArgs e)
        {
            _lastSelected = sender as TreeView;
        }

        /// <summary>
        /// Highlights a certain subnet.
        /// </summary>
        private void HighlightSCC_OnClick(object sender, RoutedEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null && mv.NetVM.IsAnalyzed)
            {
                var component = _lastSelected.SelectedItem as SubnetViewModel;
                if (component != null) mv.NetVM.HighlightSubnet(component, circuit: HighlightCheckBox.IsChecked ?? false);
            }
        }

        /// <summary>
        /// Export a decomposition of the net to a file.
        /// </summary>
        private void ExportAnalysis_OnClick(object sender, RoutedEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv != null && mv.NetVM.IsAnalyzed)
            {
                if (mv.NetVM.IsEmpty)
                {
                    MessageBox.Show("Error: Nothing to export.", "Export Error");
                    return;
                }

                var analysisFileName = _titleFileName == "" ? "mydecomposition" : System.IO.Path.GetFileNameWithoutExtension(_titleFileName);

                var sfd = new SaveFileDialog
                {
                    Title = "Import As",
                    OverwritePrompt = true,
                    FileName = analysisFileName,
                    AddExtension = true,
                    DefaultExt = "npdec",
                    Filter = "Net Decomposition Files|*.npdec|XML Files|*.xml|All Files|*.*"
                };

                if (_filePath != "")
                    sfd.InitialDirectory = System.IO.Path.GetDirectoryName(_filePath);

                if (sfd.ShowDialog() == true)
                {
                    try
                    {
                        mv.NetVM.SerializeAnalysis().Save(sfd.FileName);
                        RegisterEvent("Net decomposition exported to " + sfd.FileName + ".",
                            "Decomposition Exported", _hasChanged, false);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, "Export Error");
                    }
                }
            }
        }

        /// <summary>
        /// A timer to simulate OnResizeEnd event.
        /// </summary>
        private readonly System.Timers.Timer _resizeTimer = new System.Timers.Timer(100);

        /// <summary>
        /// Automatically rearrange the net if the window size has been decreased.
        /// </summary>
        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height < e.PreviousSize.Height || e.NewSize.Width < e.PreviousSize.Width)
            {
                _resizeTimer.Stop();
                _resizeTimer.Start();
            }
            else
                Centralize_OnClick(sender, e);
        }

        /// <summary>
        /// Automatically rearrange the net if resizing is done.
        /// </summary>
        private void ResizingDone(object sender, ElapsedEventArgs e)
        {
            _resizeTimer.Stop();
            AutoLayout_OnClick(sender, new RoutedEventArgs());
        }

        /// <summary>
        /// Opens the about window.
        /// </summary>
        private void About_OnClick(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow();
            about.ShowDialog();
        }
    }
}
