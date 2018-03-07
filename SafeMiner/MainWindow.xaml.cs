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
            wnd = this;
            //Initialize all the of the lists.
            pools = new List<MiningPool>();
            procs = new List<Process>();
            AmdGPUs = new List<GraphicsCard>();
            NvidiaGPUs = new List<GraphicsCard>();
            pools = MiningPool.Get();
            interrogateCards();

            //Load the Drop Down Menu.
            cbItems = new ObservableCollection<ComboBoxItem>();
            foreach (var p in pools.OrderBy(x => x.ID))
            {
                Ping myPing = new Ping();
                PingReply reply = myPing.Send(p.uri, 1000);
                if(reply.Status != IPStatus.TimedOut)
                    cbItems.Add(new ComboBoxItem { Content = p.Name + ", Ping: " + reply.RoundtripTime + ", Pool Fee: " + p.fee });
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
                    procs.Add(StartDSTM());
                //Start Claymore
                if (AmdGPUs != null && AmdGPUs.Count > 0)
                    procs.Add(StartClaymore());

                MiningButton.Content = "Stop Mining!";
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


        //Get what cards the system has installed.
        private void interrogateCards()
        {
            var proc = new Process();
            proc.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "Miners\\Optiminer\\Optiminer.exe");
            proc.StartInfo.Arguments = "--list-devices";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Start();
            var output = proc.StandardOutput.ReadToEnd().Split(':').ToList().Where(x => x.ToUpper().Contains("GEFORCE") || x.ToUpper().Contains("AMD"))
                .Select(x => x.Substring(0, x.IndexOf("\r")).ToUpper().Trim()).ToList();

            proc.WaitForExit();
            var exitCode = proc.ExitCode;
            proc.Close();
            
            for(var i = 0; i < output.Count; i++)
            {
                if (output[i].Contains("GTX"))
                    NvidiaGPUs.Add(new GraphicsCard
                    {
                        Index = i,
                        CardName = output[i]
                    });
                else
                    AmdGPUs.Add(new GraphicsCard
                    {
                        Index = i,
                        CardName = output[i]
                    });
            }
            
        }

        private Process StartDSTM()//DSTM is for the Nvidia Cards
        {
            var pool = pools[SelectPoolComboBox.SelectedIndex];

            var CardIndexes = "";
            foreach (var c in NvidiaGPUs)
                CardIndexes = c.Index.ToString() + ",";
            CardIndexes = CardIndexes.Remove(CardIndexes.Length - 1);

            var proc = new Process();
            proc.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "Miners\\zm_0.6_win\\zm.exe");
            proc.StartInfo.Arguments = "zm --server " + pool.uri + " --port " + pool.port + " --user " + 
                walletAddressTextBox.Text + ".EasyMiner --pass x --dev " + CardIndexes; ;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.OutputDataReceived += new DataReceivedEventHandler(MyProcOutputHandler);
            proc.Start();
            proc.BeginOutputReadLine();
            return proc;
        }
        private Process StartClaymore()//Claymore is for the AMD cards
        {
            var pool = pools[SelectPoolComboBox.SelectedIndex];
            var CardIndexes = "";
            foreach (var c in AmdGPUs)
                CardIndexes = c.Index.ToString() + ",";
            CardIndexes = CardIndexes.Remove(CardIndexes.Length - 1);
            
            var proc = new Process();
            proc.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "Miners\\claymore_amd_v12.6\\ZecMiner64.exe");
            proc.StartInfo.Arguments = "ZecMiner64.exe -zpool stratum+tcp://" + pool.uri + ":" + pool.port + 
                " -zwal " + walletAddressTextBox.Text + ".EasyMiner -zpsw x -di " + CardIndexes;
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
