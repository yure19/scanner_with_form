using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace scanner_with_form
{
    public partial class ScannerForm : Form
    {
        private bool pauseOrStop;

        private String fromIp;
        private String toIp;

        private long ipsTotal;
        private long partOfIpsTotal;

        private int workersMax;
        private ScannerOp sOp;

        private int createdWorkers;

        private static String ALL_CERO_IP = "0.0.0.0";

        public ScannerForm()
        {
            InitializeComponent();

            pauseOrStop = false;
            workersMax = 50;

            sOp = new ScannerOp();
        }

        private void Button_Start_Click(object sender, EventArgs e)
        {
            if (button_Start.Text.Equals("Play"))
            {
                toIp = textBox2.Text;
                
                fromIp = textBox1.Text;

                if (ValidateFields(sOp).Equals(ValResult.Error))
                {
                    return;
                }

                ipsTotal = sOp.GetTotal(fromIp, toIp);
                partOfIpsTotal = 0;
                toolStripProgressBar1.Value = (int)(sOp.GetRelation(partOfIpsTotal, ipsTotal) * 100);

                listView1.Items.Clear();

                button_Start.Text = "Pause";
                button_Stop.Enabled = true;

                pauseOrStop = false;

                CreateWorkers();
            }
            else if (button_Start.Text.Equals("Pause"))
            {
                if (sOp.CompareFromAndToIp(fromIp, toIp) == CompUnit.Greater)
                {
                    button_Start.Text = "Play";  
                }
                else
                {
                    button_Start.Text = "Resume";
                }

                button_Stop.Enabled = false;
                pauseOrStop = true;
            }
            else if (button_Start.Text.Equals("Resume"))
            {
                button_Start.Text = "Pause";
                button_Stop.Enabled = true;
                pauseOrStop = false;

                CreateWorkers();
            }
        }

        private void Button_Stop_Click(object sender, EventArgs e)
        {
            button_Start.Text = "Play";
            button_Stop.Enabled = false;
            pauseOrStop = true;
        }

        private void CreateWorkers() 
        {
            List<string> ipGroup = new List<string>();
            while ((sOp.CompareFromAndToIp(fromIp, toIp) <= CompUnit.Equal) && ipGroup.Count < workersMax)
            {
                ipGroup.Add(fromIp);

                fromIp = sOp.GetNextIp(fromIp);

                if (fromIp.Equals(ALL_CERO_IP))
                {
                    break;
                }
            }

            createdWorkers = ipGroup.Count;

            for (int i = 0; i < ipGroup.Count; i++)
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += new DoWorkEventHandler(BackgroundWorkers_DoWork);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorkers_RunWorkerCompleted);

                bw.RunWorkerAsync(ipGroup[i]);
            }
        }

        private void BackgroundWorkers_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            String ip = e.Argument as String;

            Dictionary<string, string> scanningReport = new Dictionary<string, string>();
            scanningReport.Add("toolStripStatusLabel1.Text", ip);

            if (sOp.PingReply(ip) == 0)
            {
                scanningReport.Add("item", ip);
            }

            e.Result = scanningReport;
        }

        private void BackgroundWorkers_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Dictionary<string, string> scanningReport = e.Result as Dictionary<string, string>;

            toolStripStatusLabel1.Text = scanningReport["toolStripStatusLabel1.Text"];

            if (scanningReport.ContainsKey("item"))
            {
                listView1.Items.Add(new ListViewItem(new[] { scanningReport["item"], "Success" }));
            }

            partOfIpsTotal += 1;
            //Interlocked.Increment(ref partOfIpsTotal)
            toolStripProgressBar1.Value = (int)(sOp.GetRelation(partOfIpsTotal, ipsTotal) * 100);

            if (!pauseOrStop) 
            {
                if ((createdWorkers -= 1) == 0) 
                {
                    if (sOp.CompareFromAndToIp(fromIp, toIp) <= CompUnit.Equal && !fromIp.Equals(ALL_CERO_IP)) 
                    {
                        this.CreateWorkers();
                    }
                    else
                    {
                        button_Start.Text = "Play";
                        button_Stop.Enabled = false;
                    }
                }       
            } 
        }

        private ValResult ValidateFields(ScannerOp sOp) 
        {
            if (fromIp.Equals(""))
            {
                MessageBox.Show("Must complete the From field", "Validation Error", MessageBoxButtons.OK);
                button_Start.Text = "Play";
                textBox1.Focus();
                return ValResult.Error;
            }

            if (toIp.Equals(""))
            {
                MessageBox.Show("Must complete the To field", "Validation Error", MessageBoxButtons.OK);
                textBox2.Focus();
                return ValResult.Error;
            }

            if (fromIp.Split('.').Length != 4)
            {
                MessageBox.Show("The From field must be an IPv4", "Validation Error", MessageBoxButtons.OK);
                textBox1.Focus();
                return ValResult.Error;
            }

            if (toIp.Split('.').Length != 4)
            {
                MessageBox.Show("The To field must be an IPv4", "Validation Error", MessageBoxButtons.OK);
                textBox2.Focus();
                return ValResult.Error;
            }

            if (sOp.CompareFromAndToIp(fromIp, toIp) == CompUnit.Greater)
            {
                MessageBox.Show("The From field must be equal or lesser than To field", "Validation Error", MessageBoxButtons.OK);
                textBox1.Focus();
                return ValResult.Error;
            }

            String[] ipFromSpl = fromIp.Split('.');
            String[] ipToSpl = toIp.Split('.');
            for (int i = 0; i < ipFromSpl.Length; i++)
            {
                if (Int32.Parse(ipFromSpl[i]) > 255 || Int32.Parse(ipToSpl[i]) > 255)
                {
                    MessageBox.Show("The value in all octets must be lesser than 256", "Validation Error", MessageBoxButtons.OK);
                    textBox1.Focus();
                    return ValResult.Error;
                }
            }

            if (sOp.CompareFromAndToIp(fromIp, "1.1.1.1") == CompUnit.Lesser)
            {
                MessageBox.Show("The minimal IP allowed is 1.1.1.1", "Validation Error", MessageBoxButtons.OK);
                textBox1.Focus();
                return ValResult.Error;
            }
            return ValResult.Ok;
        }

        private void ScannerForm_Load(object sender, EventArgs e)
        {
            listView1.Columns[listView1.Columns.Count - 1].Width = -2;
        }
    }
}
