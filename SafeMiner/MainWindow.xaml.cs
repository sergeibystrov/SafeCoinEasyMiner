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
using SafeMiner.Enumerations;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;

namespace SafeMiner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<MiningPools> pools;
        List<Process> procs;
        public ObservableCollection<ComboBoxItem> cbItems { get; set; }
        public ComboBoxItem SelectedcbItem { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            pools = new List<MiningPools>();
            procs = new List<Process>();
            getPools();

            cbItems = new ObservableCollection<ComboBoxItem>();
            foreach( var p in pools.OrderBy(x => x.ID))
                cbItems.Add(new ComboBoxItem { Content = p.Name + " - DEV FEE: " + p.fee });
            SelectPoolComboBox.ItemsSource = cbItems;
            if (!File.Exists(Directory.GetCurrentDirectory() + "\\settings.txt"))
            {
                File.Create(Directory.GetCurrentDirectory() + "\\settings.txt");
            }
            else
            {
                StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "\\settings.txt");
                string line = sr.ReadLine();
                sr.Close();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    walletAddressTextBox.Text = line.Split(' ')[0];
                    SelectPoolComboBox.SelectedIndex = int.Parse(line.Split(' ')[1]);
                }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\settings.txt", String.Empty);
            StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "\\settings.txt");
            sw.WriteLine(walletAddressTextBox.Text + " " + SelectPoolComboBox.SelectedIndex.ToString());
            sw.Close();

            var cards = interrogateCards();

            var pDSTM = StartDSTM(cards.Where(x => x.Type == GraphicsCardType.NVidia_New).Select(x => x).ToList());
            var pClaymore = StartClaymore(cards.Where(x => x.Type == GraphicsCardType.AMD).Select(x => x).ToList());

            if (pDSTM != null)
                procs.Add(pDSTM);
            if (pClaymore != null)
                procs.Add(pClaymore);
        }
        

        private List<GraphicsCard> interrogateCards()
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
            
            List<GraphicsCard> cards = new List<GraphicsCard>();

            for(var i = 0; i < output.Count; i++)
            {
                cards.Add(new GraphicsCard
                {
                    CardName = output[i],
                    CardNumber = i,
                    Type = output[i].Contains("GTX") ? GraphicsCardType.NVidia_New : GraphicsCardType.AMD
                });
            }

            return cards;
        }

        private Process StartDSTM(List<GraphicsCard> Cards)
        {
            if (Cards == null || Cards.Count == 0)
                return null;
            var pool = pools[SelectPoolComboBox.SelectedIndex];

            var cardNumbers = "";
            foreach (var c in Cards)
                cardNumbers = c.CardNumber.ToString() + ",";
            cardNumbers = cardNumbers.Remove(cardNumbers.Length - 1);

            var proc = new Process();
            proc.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "Miners\\zm_0.6_win\\zm.exe");
            proc.StartInfo.Arguments = "zm --server " + pool.uri + " --port " + pool.port + " --user "+walletAddressTextBox.Text+".EasyMiner --pass x --dev " + cardNumbers +
                " > "+ Directory.GetCurrentDirectory()+"\\DSTMLog.txt";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = false;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            proc.Start();
            return proc;
        }
        private Process StartClaymore(List<GraphicsCard> Cards)
        {
            if (Cards == null || Cards.Count == 0)
                return null;
            var pool = pools[SelectPoolComboBox.SelectedIndex];
            var cardNumbers = "";
            foreach (var c in Cards)
                cardNumbers = c.CardNumber.ToString() + ",";
            cardNumbers = cardNumbers.Remove(cardNumbers.Length - 1);
            
            var proc = new Process();
            proc.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "Miners\\claymore_amd_v12.6\\ZecMiner64.exe");
            proc.StartInfo.Arguments = "ZecMiner64.exe -zpool stratum+tcp://" + pool.uri + ":" + pool.port + " -zwal " + walletAddressTextBox.Text + ".EasyMiner -zpsw x -di " + cardNumbers;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = false;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            proc.Start();
            return proc;
        }


        private List<MiningPools> getPools()
        {
            
            pools.Add(new MiningPools
            {
                ID = 0,
                Name = "Equipool USA",
                fee = "0.2%",
                port = "50111",
                uri = "mine.equipool.1ds.us"
            });
            pools.Add(new MiningPools
            {
                ID = 1,
                Name = "Cats Pool EU",
                fee = "0.5%",
                port = "3432",
                uri = "safecoin.catspool.org"
            });

            return pools;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var p in procs)
                p.Kill();
        }
    }
}
