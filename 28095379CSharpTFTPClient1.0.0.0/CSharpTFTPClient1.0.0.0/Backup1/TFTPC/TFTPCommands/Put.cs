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

using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TFTPC
{
    internal partial class Commands
    {
        // Put implementation
        internal static bool Put
            (string LocalFilename, string RemoteFilename, string Host,
            TFTP.Modes Mode, int BlockSize, int Timeout)
        {
            int[] block = new int[2];
            int bufferSize = BlockSize;
            long fileSize, bytesSent = 0;
                        
            BinaryReader BReader = new BinaryReader(File.Open(LocalFilename,FileMode.Open));
            FileInfo sendFile = new FileInfo(LocalFilename);

            TFTP.OpCodes opCode = new TFTP.OpCodes();
            
            IPHostEntry hostInfo = Dns.GetHostEntry(Host);
            IPAddress address = hostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(address, 69);
            EndPoint localEP = (remoteEP);
            Socket UDPSock = new Socket
                (remoteEP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            // Retrieve filesize for tsize option
            fileSize = sendFile.Length;

            // Create initial request and buffer for response
            byte[] sendData = TFTPPacket.Create.Request
                (TFTP.OpCodes.WRQ, RemoteFilename, Mode, BlockSize, fileSize, Timeout);
            byte[] recvData = new byte[bufferSize];

            UDPSock.ReceiveTimeout = Timeout * 1000;

            // Send request and wait for response
            UDPSock.SendTo(sendData, remoteEP);
            UDPSock.ReceiveFrom(recvData, ref localEP);

            //Get TID
            remoteEP.Port = ((IPEndPoint)localEP).Port;

            // Fire OnConnect Event
            TFTP.FireOnConnect();

            while (true)
            {
                // Read opcode
                opCode = TFTPPacket.Read.OpCode(recvData);

                // ACK packet
                if (opCode == TFTP.OpCodes.ACK)
                {
                    block = TFTPPacket.Modify.IncrementBock(recvData, block);
                    
                    sendData = BReader.ReadBytes(bufferSize);
                    bytesSent += sendData.Length;

                    // Fire OnTransfer Event
                    TFTP.FireOnTransfer(bytesSent, fileSize);
                    
                    sendData = TFTPPacket.Create.Data(sendData, block[0], block[1]);
                    
                    // Check if this packet is the last
                    if (sendData.Length < bufferSize + 4)
                    {
                        // Send final data packet and wait for ack
                        while (true)
                        {
                            UDPSock.SendTo(sendData, remoteEP);
                            UDPSock.ReceiveFrom(recvData, ref localEP);
                            remoteEP.Port = ((IPEndPoint)localEP).Port;

                            // Check the blocks and break free if equal
                            if(TFTPPacket.Read.CheckBlock(sendData, recvData))
                                break;                            
                        }
                        
                        // Fire OnTransferFinish Event
                        TFTP.FireOnTransferFinish();
                        break;
                    }
                }

                // OACK packet
                else if(opCode == TFTP.OpCodes.OACK)
                {
                    sendData = BReader.ReadBytes(bufferSize);
                    sendData = TFTPPacket.Create.Data(sendData, 0, 1);
                    bytesSent += sendData.Length - 4;

                    // Fire OnTransfer Event
                    TFTP.FireOnTransfer(bytesSent, fileSize);

                    if(fileSize == 0)
                    {
                        // Fire OnTransferFinish Event
                        TFTP.FireOnTransferFinish();
                        break;
                    }
                    else
                    {
                        
                        // Check if this packet is the last
                        if (sendData.Length < bufferSize + 4)
                        {
                            // Send final data packet and wait for ack
                            while(true)
                            {
                                UDPSock.SendTo(sendData, remoteEP);
                                UDPSock.ReceiveFrom(recvData, ref localEP);
                                remoteEP.Port = ((IPEndPoint)localEP).Port;
                                
                                // Check the blocks and break free if equal
                                if (TFTPPacket.Read.CheckBlock(sendData, recvData))
                                    break;                                
                            }
                            // Fire OnTransferFinish Event
                            TFTP.FireOnTransferFinish();
                            break;
                        }
                    }
                }
                else if (opCode == TFTP.OpCodes.ERROR)
                {
                    TFTPPacket.Read.Error transferError = new TFTPPacket.Read.Error(recvData);
                    TFTP.FireOnTransferError
                        (transferError.Code, transferError.Message);
                    break;
                }

                // Send next packet
                UDPSock.SendTo(sendData, remoteEP);
                UDPSock.ReceiveFrom(recvData, ref localEP);
                remoteEP.Port = ((IPEndPoint)localEP).Port;
            }
            BReader.Close();
            UDPSock.Close();

            // Fire OnDisconnect Event
            TFTP.FireOnDisconnect();

            return true;
        }
    }
}