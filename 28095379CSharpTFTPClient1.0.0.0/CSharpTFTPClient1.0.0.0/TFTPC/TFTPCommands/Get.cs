//--------------------------------------------------------------------------------//
//                                                                                //
// Copyright ?2007 John Leitch                                                   //
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

using System.IO;
using System.Net;
using System.Net.Sockets;

//using System.Windows.Forms;

namespace TFTPC
{
    internal partial class Commands
    {        
        // Get implementation
        internal static bool Get
            (string LocalFilename, string RemoteFilename, string Host,
            TFTP.Modes Mode, int BlockSize, int Timeout)            
        {
            int recvLen, remoteFileSize = 0, buffer = BlockSize + 4;
            long bytesReceived = 0;            
            
            BinaryWriter BWriter = new BinaryWriter(File.Open(LocalFilename, FileMode.Create));
            
            TFTP.OpCodes opCode = new TFTP.OpCodes();
            
            IPHostEntry hInfo = Dns.GetHostEntry(Host);
            IPAddress address = hInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(address, 69);
            EndPoint localEP = (remoteEP);
            Socket UDPSock = new Socket
                (remoteEP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            // Create initial request and buffer for response
            byte[] sendData = TFTPPacket.Create.Request
                (TFTP.OpCodes.RRQ, RemoteFilename, Mode, BlockSize, 0, Timeout);
            byte[] recvData = new byte[BlockSize + 4];

            UDPSock.ReceiveTimeout = Timeout * 1000;

            // Send request and wait for response
            UDPSock.SendTo(sendData, remoteEP);
            recvLen = UDPSock.ReceiveFrom(recvData, ref localEP);
            
            // Get TID
            remoteEP.Port = ((IPEndPoint)localEP).Port;

            // Fire connected event
            TFTP.FireOnConnect();
            
            while (true)
            {
                // Read opcode
                opCode = TFTPPacket.Read.OpCode(recvData);                

                // DATA packet
                if (opCode == TFTP.OpCodes.DATA)
                {
                    bytesReceived += recvLen - 4;               


                    
                    // Fire OnTransfer Event and pass along bytesReceived
                    TFTP.FireOnTransfer(bytesReceived, remoteFileSize);

                    for (int h = 4; h < recvLen; h++)
                    {   
                        BWriter.Write(recvData[h]);
                    }
                                        
                    sendData = TFTPPacket.Create.Ack(recvData[2], recvData[3]);

                    // Check if this packet is the last
                    if (recvLen < buffer)
                    {
                        // Send final ACK
                        UDPSock.SendTo(sendData, remoteEP);

                        // Fire OnTransferFinish Event
                        TFTP.FireOnTransferFinish();
                        break;
                    }
                }

                // OACK packet
                else if (opCode == TFTP.OpCodes.OACK)
                {
                    remoteFileSize = TFTPPacket.Read.TSize(recvData);                    
                    sendData = TFTPPacket.Create.Ack(0, 0);                    
                }

                // ERROR packet
                else if (opCode == TFTP.OpCodes.ERROR)
                {
                    TFTPPacket.Read.Error transferError = new TFTPPacket.Read.Error(recvData);
                    TFTP.FireOnTransferError
                        (transferError.Code, transferError.Message);
                    break;
                }
               
                // Send next packet
                UDPSock.SendTo(sendData, remoteEP);
                recvLen = UDPSock.ReceiveFrom(recvData, ref localEP);
                remoteEP.Port = ((IPEndPoint)localEP).Port;
            }
            
            BWriter.Close();
            UDPSock.Close();

            // Fire OnDisconnect Event
            TFTP.FireOnDisconnect();
            return true;            
        }        
    }
}
