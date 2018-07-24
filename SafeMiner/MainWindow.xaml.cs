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
using SafeMiner.Models;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;

namespace SafeMiner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Window wnd;
        List<MiningPool> pools;
        List<Process> procs;
        List<GraphicsCard> AmdGPUs;
        List<GraphicsCard> NvidiaGPUs;
        public ObservableCollection<ComboBoxItem> cbItems { get; set; }
        public ComboBoxItem SelectedcbItem { get; set; }
        bool mining = false;

        public MainWindow()
        {
            InitializeComponent();
            ViewWorkerTextBlock.Visibility = Visibility.Collapsed;
            wnd = this;
            //Initialize all the of the lists.
            pools = new List<MiningPool>();
            procs = new List<Process>();
            NvidiaGPUs = new List<GraphicsCard>();
            pools = MiningPool.Get();

            //Load the Drop Down Menu.
            cbItems = new ObservableCollection<ComboBoxItem>();
            foreach (var p in pools.OrderBy(x => x.ID))
            {
                cbItems.Add(new ComboBoxItem { Content = p.Name });
            }
            SelectPoolComboBox.ItemsSource = cbItems;

            //Get the users default settings.
            walletAddressTextBox.Text = Properties.Settings.Default.WalletAddress;
            SelectPoolComboBox.SelectedIndex = Properties.Settings.Default.PoolSelection;
        }

        //On click start mining.
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!mining)
            {
                //Save the wallet and pool info into settings
                Properties.Settings.Default.WalletAddress = walletAddressTextBox.Text;
                Properties.Settings.Default.PoolSelection = SelectPoolComboBox.SelectedIndex;
                Properties.Settings.Default.Save();

                //Start DSTM
                if (NvidiaGPUs != null && NvidiaGPUs.Count > 0)
                    procs.Add(StartEWBF());

                MiningButton.Content = "Stop Mining!";
                ViewWorkerTextBlock.Visibility = Visibility.Visible;
                ViewWorkerHyperLink.NavigateUri = new Uri(pools[SelectPoolComboBox.SelectedIndex].WorkerPage.Replace("|ADDR|",walletAddressTextBox.Text));
                mining = true;
            }
            else
            {
                foreach (var p in procs)
                {
                    try//Try to kill
                    {
                        p.Kill();
                    }
                    catch//If error assume process has been killed & keep going.
                    {
                        continue;
                    }
                }
                MiningButton.Content = "Start Mining!";
                ViewWorkerTextBlock.Visibility = Visibility.Collapsed;


                mining = false;
            }
        }

        //On window closing exit the mining programs.
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var p in procs)
            {
                try//Try to kill
                {
                    p.Kill();
                }
                catch//If error assume process has been killed & keep going.
                {
                    continue;
                }
            }
        }
        //Go get a wallet from safecoin.org
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        
        private Process StartEWBF()
        {
            var pool = pools[SelectPoolComboBox.SelectedIndex];
            
            var proc = new Process();
            proc.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "Miners\\EWBF\\miner.exe");
            proc.StartInfo.Arguments = "miner --server " + pool.uri + " --port " + pool.port + " --user " +
                walletAddressTextBox.Text + ".EasyMiner --pass x --algo 144_5 --pers Safecoin";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.OutputDataReceived += new DataReceivedEventHandler(MyProcOutputHandler);
            proc.Start();
            proc.BeginOutputReadLine();
            return proc;
        }
        private static List<string> myList = new List<string>();
        private static void MyProcOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            // Collect the sort command output. 
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MainWindow my = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    myList.Add("[" + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss tt") + "]   " + outLine.Data);
                    if (myList.Count > 14)
                        myList.RemoveAt(0);
                    my.ConsoleOutRichTextBox.Text = string.Join("\n",myList.ToArray());
                }));
            }
        }
    }
}
