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

namespace TFTPC
{    
    public class TFTP : ITFTP
    {
        // Delegates
        public delegate void OnConnectDelegate();
        public delegate void OnTransferDelegate(long BytesTransferred, long BytesTotal);
        public delegate void OnTransferErrorDelegate(short ErrorCode, string ErrorMessage);
        public delegate void OnTransferFinishDelegate();
        public delegate void OnDisconnectDelegate();

        // Constructor
        public TFTP()
        {
            // Default property values
            modeProperty = Modes.OCTET;
            blockSizeProperty = 512;
            timeoutProperty = 10;
        }

        // Events
        public static event OnConnectDelegate OnConnect;
        public static event OnTransferDelegate OnTransfer;
        public static event OnTransferErrorDelegate OnTransferError;
        public static event OnTransferFinishDelegate OnTransferFinish;
        public static event OnDisconnectDelegate OnDisconnect;

        public static void FireOnConnect()
        {
            if (OnConnect != null)
                OnConnect.Invoke();
        }        
        public static void FireOnTransfer(long BytesTotal, long BytesTransferred)
        {
            if (OnTransfer != null)
                OnTransfer.Invoke(BytesTotal, BytesTransferred); 
        }
        public static void FireOnTransferError(short ErrorCode, string ErrorMessage)
        {
            if (OnTransferError != null)
                OnTransferError.Invoke(ErrorCode, ErrorMessage);
        }
        public static void FireOnTransferFinish()
        {
            if (OnTransferFinish != null)
                OnTransferFinish.Invoke();
        }
        public static void FireOnDisconnect()
        {
            if (OnDisconnect != null)
                OnDisconnect.Invoke();
        }        

        // Enumerations
        public enum Modes
        {
            NETASCII = 0,
            OCTET = 1
        }
        public enum OpCodes
        {
            RRQ = 1,    // Read Request
            WRQ = 2,    // Write Request
            DATA = 3,   // Data
            ACK = 4,    // Acknowledge
            ERROR = 5,  // Error
            OACK = 6    // Option Acknowledge
        }

        // Properties
        int blockSizeProperty, timeoutProperty;
        string hostProperty;
        Modes modeProperty;

        public string Host
        {
            get
            {
                return hostProperty;
            }
            set
            {
                hostProperty = value;
            }
        }
        public Modes Mode
        {
            get
            {
                return modeProperty;
            }
            set
            {
                modeProperty = value;
            }
        }
        public int BlockSize
        {
            get
            {
                return blockSizeProperty;
            }
            set
            {
                blockSizeProperty = value;
            }
        }
        public int Timeout
        {
            get
            {
                return timeoutProperty;
            }
            set
            {
                timeoutProperty = value;
            }
        }        
        
        // Methods
        public bool Get(object TransferOptions)
        {
            Transfer.Options tOptions = (Transfer.Options)TransferOptions;
            return Commands.Get(tOptions.LocalFilename, tOptions.RemoteFilename,
                tOptions.Host, Mode, BlockSize, Timeout);
        }
        public bool Get(string File)
        {
            return Commands.Get(File, File, Host, Mode, BlockSize, Timeout);            
        }
        public bool Get(string File, string Host)
        {
            return Commands.Get(File, File, Host, Mode, BlockSize, Timeout);
        }
        public bool Get(string LocalFile, string RemoteFile, string Host)
        {
            return Commands.Get(LocalFile, RemoteFile, Host, Mode, BlockSize, Timeout);
        }
        public bool Put(object TransferOptions)
        {
            Transfer.Options tOptions = (Transfer.Options)TransferOptions;
            return Commands.Put(tOptions.LocalFilename, tOptions.RemoteFilename,
                tOptions.Host, Mode, BlockSize, Timeout);
        }
        public bool Put(string File)
        {
            return Commands.Put(File, File, Host, Mode, BlockSize, Timeout);
        }
        public bool Put(string File, string Host)
        {
            return Commands.Put(File, File, Host, Mode, BlockSize, Timeout);
        }
        public bool Put(string LocalFile, string RemoteFile, string Host)
        {
            return Commands.Put(LocalFile, RemoteFile, Host, Mode, BlockSize, Timeout);
        }
    }
}