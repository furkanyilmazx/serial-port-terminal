using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;

namespace teminal_fyx
{
    public partial class Form1 : Form
    {
        public static SerialPort mySerialPort;
        public static TimeSpan TodayTime;
        public static ToolTip tip;
        public static NotifyIcon nft;

        public static bool portOpenFlag = false, textbox1TextChangeFlag = false, logFileFlag = false,defaultLogfileFlag = false;
        public static int portTimerElapsed = 0;

        public static string selectedTextRichBox = "No selected text";
        public static FileStream LogFile = null;
        public static StreamWriter swLogFile = null;
        



        public Form1()
        {
            
            InitializeComponent();

            //statusLabel1.Enabled = true;

            textBox2.ReadOnly = true;
            textBox2.BackColor = Color.White;
           // copyToolStripMenuItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            contextMenuStrip1.RenderMode = ToolStripRenderMode.Professional;
            contextMenuStrip1.ShowImageMargin = false;

            contextMenuStrip_transmit.RenderMode = ToolStripRenderMode.Professional;
            contextMenuStrip_transmit.ShowImageMargin = false;

            statusLabel_mailLink.IsLink = true;
            richTex_receive.BackColor = Color.White;

            checkBox3.Checked = true;


            textBox1.Enabled = false;
            button_sendData.Enabled = false;
            button_sendFile.Enabled = false;

            textBox1.Text = "Type Here...";
            textBox1.ForeColor = Color.Gray;


            comboBox2.Items.Add("2400");
            comboBox2.Items.Add("9600");
            comboBox2.Items.Add("19200");
            comboBox2.Items.Add("57600");
            comboBox2.Items.Add("115200");
            comboBox2.Items.Add("230400");
            comboBox2.SelectedIndex = 4;

            comboBox3.Items.Add("8");
            comboBox3.Items.Add("9");
            comboBox3.SelectedIndex = 0;

            comboBox4.Items.Add("none");
            comboBox4.Items.Add("even");
            comboBox4.Items.Add("odd");
            comboBox4.Items.Add("mark");
            comboBox4.Items.Add("space");
            comboBox4.SelectedIndex = 0;

            comboBox5.Items.Add("1");
            comboBox5.Items.Add("1.5");
            comboBox5.Items.Add("2");
            comboBox5.SelectedIndex = 0;

            comboBox6.Items.Add("Append None");
            comboBox6.Items.Add("Append CR");
            comboBox6.Items.Add("Append LF");
            comboBox6.Items.Add("Append CR-LF");
            comboBox6.SelectedIndex = 0;

            comboBox7.Items.Add("None");
            comboBox7.Items.Add("XonXoff");
            comboBox7.SelectedIndex = 0;

            comboBox8.Items.Add("None");
            comboBox8.Items.Add("RTS/CTS");
            comboBox8.Items.Add("DTR/DSR");
            comboBox8.SelectedIndex = 0;
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            checkBox1.Checked = true;
            checkBox1.Checked = false;

            tip = new ToolTip();

            statusStrip1.ShowItemToolTips = true;
            statusLabelPortStatus.ToolTipText = "Portun durumunu belirtir";
            statusLabel1.ToolTipText = "Port açıldıktan sonra geçen süreyi saniye cinsinden belirtir";
            statusLabel_mailLink.ToolTipText = "Program hakkında görüş ve isteklerinizi bu adrese mail atabilirsiniz";

            nft = new NotifyIcon();

            //nft.Icon = new Icon(@"D:\Downloads\fycx.ico");
            nft.Visible = true;
            nft.Text = "FyxTerm V1.0";
            nft.BalloonTipTitle = "FYxTerm V1.0";
            nft.BalloonTipText = "Hoşgeldiniz";
            nft.BalloonTipIcon = ToolTipIcon.Info;
            nft.ShowBalloonTip(1000);
            nft.Visible = false;

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                logFileFlag = true;
                checkBox2.Enabled = true;
                groupBox6.Enabled = true;
                button_saveFile.Enabled = false;
                
            }
            else
            {
                textBox2.Clear();
                checkBox2.Checked = false;
                checkBox2.Enabled = false;
                groupBox6.Enabled = false;
                button_saveFile.Enabled = true;
                logFileFlag = false;
            }
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            richTex_receive.Clear();
            GC.Collect();
        }

        private void button_copyAll_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(richTex_receive.Text))
                MessageBox.Show("No Text found!", "Warning");
            else
            {
                Clipboard.SetText(richTex_receive.Text);
            }
            
        }

        private void button_browse_Click(object sender, EventArgs e)
        {
            SaveFileDialog savingFile = new SaveFileDialog();
            savingFile.Title = "Kaydedilecek yer";
            savingFile.InitialDirectory = @"D:\";
            savingFile.Filter = "All files (*.*)|*.*|Text Files (*.txt)|*.txt";
            savingFile.FilterIndex = 2;
            savingFile.RestoreDirectory = true;

            if (savingFile.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = savingFile.FileName;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                button_browse.Enabled = false;
                TodayTime = DateTime.Now.TimeOfDay;
                textBox2.Text = @"D:\LOG_" + TodayTime.Hours.ToString() + "_" + TodayTime.Minutes.ToString() + "_" + TodayTime.Seconds.ToString()+".txt";
            }
            else
            {
                button_browse.Enabled = true;
                textBox2.Clear();
            }
        }

        private void button_openPort_Click(object sender, EventArgs e)
        {
            if (portOpenFlag)
            {
                button_openPort.Text = "Open Port";
                mySerialPort.DataReceived -= (DataReceivedHandler);
                mySerialPort.DiscardOutBuffer();
                mySerialPort.DiscardInBuffer();
                mySerialPort.Dispose();
                mySerialPort.Close();
                portTimer.Stop();

                textBox1.ResetText();

                if (checkBox2.Checked)
                {
                    TodayTime = DateTime.Now.TimeOfDay;
                    textBox2.Text = @"D:\LOG_" + TodayTime.Hours.ToString() + "_" + TodayTime.Minutes.ToString() + "_" + TodayTime.Seconds.ToString() + ".txt";
                }

                System.Media.SystemSounds.Hand.Play();
                portTimerElapsed = 0;
                //statusLabel1.Text = "Elapsed Time: none";
                statusLabel1.Font = new Font(statusLabel1.Font, FontStyle.Regular);
                statusLabel1.ForeColor = Color.Gray;
                statusLabelPortStatus.ForeColor = Color.Red;
                label_PortColor.BackColor = Color.Red;
                //statusLabelPortStatus.Font = new Font(statusLabelPortStatus.Font, FontStyle.Regular);
                statusLabelPortStatus.Text = "Port is CLOSE";
                groupBox1.Enabled = true;
                checkBox3.Enabled = true;
                comboBox6.Enabled = true;
                groupBox3.Enabled = true;

                if (!checkBox1.Checked)
                {
                    button_saveFile.Enabled = true;
                }



                textBox1.Enabled = false;
                button_sendData.Enabled = false;
                button_sendFile.Enabled = false;

                if (logFileFlag)
                {
                    swLogFile.WriteLine(Environment.NewLine + "___THE END___" + Environment.NewLine);
                    swLogFile.Close();
                    LogFile.Close();
                }


                GC.Collect();
                GC.WaitForPendingFinalizers();
                portOpenFlag = false;
            }
            else
            {
               

                if (checkBox1.Checked && String.IsNullOrWhiteSpace(textBox2.Text) && !checkBox2.Checked)
                {
                    System.Media.SystemSounds.Beep.Play();
                    MessageBox.Show("Port didn't OPEN\nPlease Browse Log File...", "Error");
                    
                }
                else
                {
                        if ((comboBox1.SelectedItem != null) && (comboBox2.SelectedItem != null)
                            && (comboBox3.SelectedItem != null) && (comboBox4.SelectedItem != null) && (comboBox5.SelectedItem != null))
	                    {
                            try
                            {

                            

                            mySerialPort = new SerialPort(comboBox1.Text);
                            
                            switch (comboBox4.Text)
                            {
                                case "none":
                                    mySerialPort.Parity = Parity.None;
                                    break;
                                case "even":
                                    mySerialPort.Parity = Parity.Even;
                                    break;
                                case "odd":
                                    mySerialPort.Parity = Parity.Odd;
                                    break;
                                case "mark":
                                    mySerialPort.Parity = Parity.Mark;
                                    break;
                                case "space":
                                    mySerialPort.Parity = Parity.Space;
                                    break;

                            }
                            switch (comboBox5.Text)
                            {
                                case "1":
                                    mySerialPort.StopBits = StopBits.One;
                                    break;
                                case "2":
                                    mySerialPort.StopBits = StopBits.Two;
                                    break;
                                case "1.5":
                                    mySerialPort.StopBits = StopBits.OnePointFive;
                                    break;
                            }
                            switch (comboBox7.Text)
                            {
                                case "None":
                                    mySerialPort.Handshake = Handshake.None;
                                    break;
                                case "XonXoff":
                                    mySerialPort.Handshake = Handshake.XOnXOff;
                                    break;
                            }

                            switch (comboBox8.Text)
                            {
                                case "None":
                                    mySerialPort.RtsEnable = false;
                                    mySerialPort.DtrEnable = false;
                                    break;
                                case "RTS/CTS":
                                    mySerialPort.RtsEnable = true;
                                    mySerialPort.DtrEnable = false;
                                    break;
                                case "DTR/DSR":
                                    mySerialPort.RtsEnable = false;
                                    mySerialPort.DtrEnable = true;
                                    break;
                            }


                            mySerialPort.BaudRate = Int32.Parse(comboBox2.Text.ToString());

                            mySerialPort.DataBits = Int32.Parse(comboBox3.Text);
                            mySerialPort.Open();

                            if (checkBox1.Checked && !String.IsNullOrEmpty(textBox2.Text))
                            {
                                LogFile = new FileStream(textBox2.Text, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                                swLogFile = new StreamWriter(LogFile);
                                swLogFile.AutoFlush = true;
                            }

                            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                            statusLabel1.Font = new Font(statusLabel1.Font, FontStyle.Bold);
                            statusLabel1.ForeColor = Color.Green;
                            statusLabel1.Text = "Elapsed Time: 0 sec";
                            
                            
                            portTimer.Start();
                            button_openPort.Text = "Close Port";
                            statusLabelPortStatus.Text = "Port is OPEN";
                            label_PortColor.BackColor = Color.Green;
                            statusLabelPortStatus.ForeColor = Color.Green;
                            statusLabelPortStatus.Font = new Font(statusLabelPortStatus.Font, FontStyle.Bold);

                            groupBox1.Enabled = false;
                            comboBox6.Enabled = false;
                            groupBox3.Enabled = false;
                            button_saveFile.Enabled = false;

                            textBox1.Enabled = true;
                            button_sendData.Enabled = true;
                            button_sendFile.Enabled = true;
                            portOpenFlag = true;
	                    }
	                    catch (Exception yt)
	                    {
                            System.Media.SystemSounds.Beep.Play();
                            MessageBox.Show("Port already open!!\nPlease close and try again"+yt.Message.ToString(), "Error");

	                    }                            
	                }
                    else
                    {
                        System.Media.SystemSounds.Beep.Play();
                        MessageBox.Show("Please select port","Error");
                    }

                }
                
            }
        }
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string indata;
            SerialPort sp = (SerialPort)sender;
            indata = sp.ReadExisting();

            if (logFileFlag)
            {
                swLogFile.Write(indata);
            }
            if (checkBox3.Checked)
            {
                //this.BeginInvoke(new Action(() =>
                //{
                try
                {
                    richTex_receive.AppendText(indata);
                }
                catch (Exception eee)
                {
                    MessageBox.Show(eee.Message);
                    throw;
                }
                    
                //}));
            }
            //else
            //{
            //    this.BeginInvoke(new Action(() =>
            //    {
            //        richTex_receive.ResetText();
            //    }));
            //}
        }



        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            
            if (checkBox3.Checked == true)
            {
                if (mySerialPort != null)
                {
                    mySerialPort.DataReceived += DataReceivedHandler;
                }
                button_clear.Enabled = true;
                button_copyAll.Enabled = true;
                richTex_receive.BackColor = Color.White;
                
            }
            else
            {
                richTex_receive.ResetText();
                if (mySerialPort != null)
                {
                    mySerialPort.DataReceived -= DataReceivedHandler;
                }
                
                richTex_receive.BackColor = Color.LightGray;
                
                richTex_receive.ResetText();
                button_clear.Enabled = false;
                button_copyAll.Enabled = false;
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (!textbox1TextChangeFlag)
            {
                textBox1.Clear();
                textBox1.ForeColor = DefaultForeColor;
            }

        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (!textbox1TextChangeFlag)
            {
                textBox1.Text = "Type Here...";
                textBox1.ForeColor = Color.Gray;
                textbox1TextChangeFlag = false;
            }
            else
            {
                if(String.IsNullOrEmpty(textBox1.Text))
                {
                    textBox1.Text = "Type Here...";
                    textBox1.ForeColor = Color.Gray;
                    textbox1TextChangeFlag = false;
                }
            }

        }

        private void button_saveFile_Click(object sender, EventArgs e)
        {
            FileStream F;
            StreamWriter sw;
            if (checkBox1.Checked)
            {
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show("Log File Already Choosen","Warning");
            }
            else
            {
                if (String.IsNullOrEmpty(richTex_receive.Text))
                    MessageBox.Show("No Text found!", "Warning");
                else
                {
                    SaveFileDialog savingFile = new SaveFileDialog();
                    savingFile.Title = "Kaydedilecek yer";
                    savingFile.InitialDirectory = @"D:\";
                    savingFile.Filter = "All files (*.*)|*.*|Text Files (*.txt)|*.txt";
                    savingFile.FilterIndex = 2;
                    savingFile.RestoreDirectory = true;



                    if (savingFile.ShowDialog() == DialogResult.OK)
                    {
                        F = new FileStream(savingFile.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                        sw = new StreamWriter(F);
                        sw.AutoFlush = true;
                        sw.Write(richTex_receive.Text);
                        sw.Close();
                        F.Close();
                        MessageBox.Show("File Saved:\n" + savingFile.FileName, "Succesfully saved");
                    }
                    
                    

                }
                
            }
        }

        private void button_sendFile_Click(object sender, EventArgs e)
        {
            byte[] fileData = null;
            button_sendData.Enabled = false;
            OpenFileDialog sendingFile = new OpenFileDialog();

            sendingFile.Title = "Sending File Location";
            sendingFile.InitialDirectory = @"D:\";
            sendingFile.Filter = "All files (*.*)|*.*|Text Files (*.txt)|*.txt";
            sendingFile.FilterIndex = 2;
            sendingFile.RestoreDirectory = true;

            if (sendingFile.ShowDialog() == DialogResult.OK)
            {


                FileStream fs = new FileStream(sendingFile.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                BinaryReader binaryReader = new BinaryReader(fs);
                fileData = binaryReader.ReadBytes((int)fs.Length);
                groupBox1.Enabled = false;

                mySerialPort.Write(fileData, 0, fileData.Length);
                this.Enabled = true;
                MessageBox.Show("File transferred to target", "Succesfull");
                fs.Flush();
                fs.Close();
                fs.Dispose();
                binaryReader.Close();
                binaryReader.Dispose();
                sendingFile.Dispose();
                sendingFile = null;
                binaryReader = null;
                fs = null;
                fileData = null;

                mySerialPort.DiscardOutBuffer();
                GC.Collect();
                GC.WaitForPendingFinalizers();
               
                
            }
            button_sendData.Enabled = true;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            textbox1TextChangeFlag = true;
            if (e.KeyCode == Keys.Enter)
            {
                button_sendData_Click(this, new EventArgs());
            }
        }

        private void portTimer_Tick(object sender, EventArgs e)
        {
            portTimerElapsed++;
            //if (portTimerElapsed == 5)
            //{
                
            //    this.Close();
            //}
            statusLabel1.Text = "Elapsed Time: " + portTimerElapsed.ToString()+" sec";
            
        }

        private void contextMenuStrip1_Click(object sender, EventArgs e)
        {

        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(selectedTextRichBox))
            {
                Clipboard.SetText(selectedTextRichBox);
            }
            
            
        }

        private void richTex_receive_SelectionChanged(object sender, EventArgs e)
        {
            selectedTextRichBox = richTex_receive.SelectedText;
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTex_receive.SelectAll();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void statusLabel_mailLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:furkanyilmazx@gmail.com?subject=FyxTerm");
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            comboBox1.ResetText();
            string[] ports = SerialPort.GetPortNames();
            foreach (string comport in ports)
            {
                comboBox1.Items.Add(comport);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                mySerialPort.Close();
                nft.Visible = false;
                nft.Dispose();

            }
            catch (Exception)
            {

            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textbox1TextChangeFlag = true;
            textBox1.Text = Clipboard.GetText();
        }

        private void selectAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            textBox1.SelectAll();
        }

        private void button_sendData_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox1.Text) && !textBox1.Text.Contains("Type Here..."))
            {

                switch (comboBox6.Text)
                {
                    case "Append None":
                        mySerialPort.Write(textBox1.Text); 
                        textBox1.Clear();
                        break;
                    case "Append CR":
                        mySerialPort.Write(textBox1.Text+"\r");
                        textBox1.Clear();
                        break;
                    case "Append LF":
                        mySerialPort.Write(textBox1.Text+"\n");
                        textBox1.Clear();
                        break;
                    case "Append CR-LF":
                        mySerialPort.Write(textBox1.Text+"\r\n");
                        textBox1.Clear();
                        break;
                }
            }
        }

        private void comboBox1_MouseEnter(object sender, EventArgs e)
        {
            tip.SetToolTip(comboBox1, "Açılacak portu seçmek için");
        }

        private void comboBox2_MouseEnter(object sender, EventArgs e)
        {
            tip.SetToolTip(comboBox2, "Portun Baud Rate değerini seçiniz");
        }

        private void comboBox3_MouseEnter(object sender, EventArgs e)
        {
            tip.SetToolTip(comboBox3, "İletilip/Alınacak verinin bit sayısı");
        }

        private void comboBox4_MouseEnter(object sender, EventArgs e)
        {
            tip.SetToolTip(comboBox4, "Parity biti seçimi(Mark: Fixed high, Space: Fixed Low)");
        }

        private void comboBox5_MouseEnter(object sender, EventArgs e)
        {
            tip.SetToolTip(comboBox5, "Haberleşmenin sonlanması için gerekli bit sayısı");
        }

        private void comboBox7_MouseEnter(object sender, EventArgs e)
        {
            tip.SetToolTip(comboBox7, "Handshake active or passive");
        }

        private void button_clear_MouseEnter(object sender, EventArgs e)
        {
            tip.SetToolTip(button_clear, "Received Data clear");
        }

        private void button_copyAll_MouseEnter(object sender, EventArgs e)
        {
            tip.SetToolTip(button_copyAll, "Hepsini seç ve kopyala");
        }

        private void button_saveFile_MouseEnter(object sender, EventArgs e)
        {
            tip.SetToolTip(button_saveFile, "Alınan veriyi log dosyası yoksa kayıt eder");
        }

        private void checkBox3_MouseEnter(object sender, EventArgs e)
        {
            tip.SetToolTip(checkBox3, "Anlık olarak gelen veriyi gösteremek için seçiniz");
        }

        private void comboBox6_MouseEnter(object sender, EventArgs e)
        {
            tip.SetToolTip(comboBox6, "Giden verinin sonuna eklenecek parametreler (CR: Enter key, LF: New Line)");
        }

        private void button_sendData_MouseEnter(object sender, EventArgs e)
        {
            tip.SetToolTip(button_sendData, "Veri yollamak için tıklayınız");
        }

        private void button_sendFile_MouseEnter(object sender, EventArgs e)
        {
            tip.SetToolTip(button_sendFile, "Dosya yollamak için tıklayınız");
        }


        private void checkBox1_MouseEnter(object sender, EventArgs e)
        {
            tip.SetToolTip(checkBox1, "Gelen verileri dosyaya kaydetmek için seçiniz");
        }

        private void checkBox2_MouseEnter(object sender, EventArgs e)
        {
            tip.SetToolTip(checkBox2, "Kaydedilecek dosyaya otomatik isim atamak için seçiniz");
        }

        private void button_openPort_MouseEnter(object sender, EventArgs e)
        {
            if (button_openPort.Text == "Open Port")
            {
                tip.SetToolTip(button_openPort, "portu açmak için tıklayınız");
            }
            else
            {
                tip.SetToolTip(button_openPort, "Poru kapatmak için tıklayınız");
            }
        }

    }
}