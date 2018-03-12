//--------------------------------------------------------------------------------//
//                                                                                //
// Copyright © 2007 John Leitch                                                   //
//                                                                                //
// Distributed under the terms of the GNU General Public License                  //
//                                                                                //
// This file is part of Open Source TFTP Client.                                  //
//                                                                                //
// Open Source TFTP Client is free software: you can redistribute it and/or       //
// modify it under the terms of the GNU General Public License version 3 as       //
// published by the Free Software Foundation.                                     //
//                                                                                //
// Open Source TFTP Client is distributed in the hope that it will be useful,     //
// but WITHOUT ANY WARRANTY; without even the implied warranty of                 //
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General      //
// Public License for more details.                                               //
//                                                                                //
// You should have received a copy of the GNU General Public License              //
// along with Open Source TFTP Client.  If not, see http://www.gnu.org/licenses/. //
//                                                                                //
//--------------------------------------------------------------------------------//

using System;
using System.Threading;
using System.Windows.Forms;
using System.Resources;
using TFTPC;

namespace TFTPClient
{
    public partial class TFTPFrm : Form
    {
        private static TFTPFrm _instance;

        public static TFTPFrm Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TFTPFrm();
                lock (_instance)
                    return TFTPFrm._instance;
            }
            set
            {
                if (_instance == null)
                    _instance = new TFTPFrm();
                lock (_instance)
                TFTPFrm._instance = value;
            }
        }

        public TFTPFrm()
        {
            InitializeComponent();
        }

        TFTP TFTPC = new TFTP();

        ////////////////////////
        //      Delegates     //
        ////////////////////////

        public delegate void TransferDelegate(Type Action, string LocalFilename,
            string RemoteFilename, string Host);
        public delegate void ProgressBarDelegate(int Maximum, int Value);
        public delegate void TransferBtnDelegate(bool Enabled);        

        ////////////////////////
        // Delegate Functions //
        ////////////////////////

        public void ProgressBarDelegateFunction(int Maximum, int Value)
        {
            lock (progressBar)
            {
                try
                {

                    progressBar.Maximum = Maximum;
                    progressBar.Value = Value;
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }
        public void TransferBtnDelegateFunction(bool Enabled)
        {
            lock (TransferBtn)
            {
                TransferBtn.Enabled = Enabled;
            }
        }

        ////////////////////////
        //   Transfer Events  //
        ////////////////////////

        private void TFTP_OnConnect()
        {
            TransferBtnDelegate tBtnDel = new TransferBtnDelegate(TransferBtnDelegateFunction);
            TransferBtn.Invoke(tBtnDel, false);

            Console.WriteLine("Connected");
        }
        private void TFTP_OnTransfer(long BytesTransferred, long BytesTotal)
        {
            if (BytesTotal != 0)
            {
                ProgressBarDelegate progressBarDel = new ProgressBarDelegate(ProgressBarDelegateFunction);
                progressBar.Invoke(progressBarDel,
                    new object[2] { (int)(BytesTotal / 10), (int)(BytesTransferred / 10) });

                Console.Write("{0}/{1} Bytes Transferred\r", BytesTransferred, BytesTotal);
            }
            else
            {
                Console.Write(".");
            }
        }
        private void TFTP_OnTransferError(short ErrorCode, string ErrorMessage)
        {
            Console.WriteLine("Error {0}: {1}", ErrorCode, ErrorMessage);
        }
        private void TFTP_OnTransferFinish()
        {
            ProgressBarDelegate progressBarDel = new ProgressBarDelegate(ProgressBarDelegateFunction);
            progressBar.Invoke(progressBarDel, new object[2] { 0, 0 });

            Console.WriteLine("\nTransfer Finished");
            
            MessageBox.Show("Transfer Complete", "TFTP Client",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void TFTP_OnDisconnect()
        {
            TransferBtnDelegate tBtnDel = new TransferBtnDelegate(TransferBtnDelegateFunction);
            TransferBtn.Invoke(tBtnDel, true);

            Console.WriteLine("Disconnected\n");
        }

        ////////////////////////
        //         UI         //
        ////////////////////////

        private void MainFrm_Load(object sender, EventArgs e)
        {
            BlockSizeCombo.SelectedIndex = 0;
            ModeCombo.SelectedIndex = 0;

            TFTP.OnConnect += new TFTP.OnConnectDelegate(TFTP_OnConnect);
            TFTP.OnTransfer += new TFTP.OnTransferDelegate(TFTP_OnTransfer);
            TFTP.OnTransferError += new TFTP.OnTransferErrorDelegate(TFTP_OnTransferError);
            TFTP.OnTransferFinish += new TFTP.OnTransferFinishDelegate(TFTP_OnTransferFinish);
            TFTP.OnDisconnect += new TFTP.OnDisconnectDelegate(TFTP_OnDisconnect);
        }
        private void TransferBtn_Click(object sender, EventArgs e)
        {
            progressBar.Value = 0;

            Transfer.Options tOptions = new Transfer.Options();            
            tOptions.LocalFilename = LocalFileNameTxt.Text;
            tOptions.RemoteFilename = RemoteFileNameTxt.Text;
            tOptions.Host = HostTxt.Text;

            Thread tThread = new Thread(new ParameterizedThreadStart(TransferThread));
            tThread.IsBackground = true;

            if (getRadio.Checked == true)
            {
                tOptions.Action = Transfer.Type.Get;
                tThread.Start(tOptions);
            }
            else
            {
                tOptions.Action = Transfer.Type.Put;
                tThread.Start(tOptions);
            }
        }
        private void TransferThread(object ScanOptions)
        {
            if (((Transfer.Options)ScanOptions).Action == Transfer.Type.Get)
                TFTPC.Get(ScanOptions);                
            else
                TFTPC.Put(ScanOptions);            
        }
        private void HostTxt_Leave(object sender, EventArgs e)
        {
            if (HostTxt.Text == "")
                HostTxt.Text = "Host";
        }
        private void HostTxt_Click(object sender, EventArgs e)
        {
            if (HostTxt.Text == "Host")
                HostTxt.Clear();
        }
        private void HostTxt_TextChanged(object sender, EventArgs e)
        {
            TFTPC.Host = HostTxt.Text;
        }
        private void LocalFileNameTxt_Leave(object sender, EventArgs e)
        {
            if (LocalFileNameTxt.Text == "")
                LocalFileNameTxt.Text = "Local File";
        }
        private void LocalFileNameTxt_Click(object sender, EventArgs e)
        {
            if (LocalFileNameTxt.Text == "Local File")
                LocalFileNameTxt.Clear();
        }
        private void RemoteFileNameTxt_Leave(object sender, EventArgs e)
        {
            if (RemoteFileNameTxt.Text == "")
                RemoteFileNameTxt.Text = "Remote File";
        }
        private void RemoteFileNameTxt_Click(object sender, EventArgs e)
        {
            if (RemoteFileNameTxt.Text == "Remote File")
                RemoteFileNameTxt.Clear();
        }
        private void getRadio_Click(object sender, EventArgs e)
        {
            putRadio.Checked = false;
        }
        private void putRadio_Click(object sender, EventArgs e)
        {
            getRadio.Checked = false;
        }
        private void ModeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (ModeCombo.SelectedIndex)
            {
                case 0:
                    TFTPC.Mode = TFTP.Modes.OCTET;
                    break;
                case 1:
                    TFTPC.Mode = TFTP.Modes.NETASCII;
                    break;
            }
        }
        private void BlockSizeCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            TFTPC.BlockSize = int.Parse(BlockSizeCombo.SelectedItem.ToString());
        }        
    }
}