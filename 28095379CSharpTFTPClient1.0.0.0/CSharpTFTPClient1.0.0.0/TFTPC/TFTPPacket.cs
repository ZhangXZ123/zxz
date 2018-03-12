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
    internal class TFTPPacket
    {
        internal class Read
        {
            internal static TFTP.OpCodes OpCode(byte[] ReceivedData)
            {
                TFTP.OpCodes opCode = new TFTP.OpCodes();
                switch (ReceivedData[1])
                {
                    case 3:
                        opCode = TFTP.OpCodes.DATA;
                        break;
                    case 4:
                        opCode = TFTP.OpCodes.ACK;
                        break;
                    case 5:
                        opCode = TFTP.OpCodes.ERROR;
                        break;
                    case 6:
                        opCode = TFTP.OpCodes.OACK;
                        break;
                }
                return opCode;
            }            
            internal static int TSize(byte[] ReceivedData)
            {
                int h, tSize = 0;
                string searchStr, decPacket = System.Text.Encoding.ASCII.GetString(ReceivedData);
                char[] splitChar = {'\0'};
                string[] splitPacket = decPacket.Split(splitChar);

                for(h=0; h < splitPacket.Length - 1; h++)
                {
                    searchStr = splitPacket[h].ToLower();
                    if (searchStr == "tsize")
                    {
                        tSize = int.Parse(splitPacket[h + 1]);
                    }
                }                
                return tSize;
            }
            internal static bool CheckBlock(byte[] SentData, byte[] ReceivedData)
            {
                if (ReceivedData[2] == SentData[2] && ReceivedData[3] == SentData[3])
                {               
                    return true;
                }
                return false;
            }

            internal class Error
            {
                short codeProperty;
                string messageProperty;

                internal Error(byte[] ReceivedData)
                {
                    string code;
                    code = ReceivedData[2].ToString() + ReceivedData[3].ToString();
                    Code = short.Parse(code);

                    Message = "";
                    for (int h = 4; h < ReceivedData.Length; h++)
                    {
                        if (ReceivedData[h] == 0)
                        {
                            break;
                        }
                        Message += (char)ReceivedData[h];
                    }                    
                }
                internal short Code
                {
                    get
                    {
                        return codeProperty;
                    }
                    set
                    {
                        codeProperty = value;
                    }
                }
                internal string Message
                {
                    get
                    {
                        return messageProperty;
                    }
                    set
                    {
                        messageProperty = value;
                    }
                }
            }
        }
        internal class Create
        {
            internal static byte[] Request
                (TFTP.OpCodes OpCode, string RemoteFileName, TFTP.Modes Mode, int BlockSize,
                long TransferSize, int Timeout)
            {
                // Request packet structure
                // -----------------------------------------------------------------------------
                // |OpCode|FileName|0|Mode|0|BlkSize|0|BSVal|0|TSize|0|TSVal|0|Timeout|0|TVal|0|
                // -----------------------------------------------------------------------------
                int len;
                
                string packetStr = "";
                string mode = Mode.ToString().ToLower();
                string blockSize = BlockSize.ToString();
                string nullChar = "\0";
                
                byte[] packet;                

                // Create packet as a string
                switch (OpCode)
                {
                    case TFTP.OpCodes.RRQ:
                        packetStr = nullChar + (char)1;
                        break;
                    case TFTP.OpCodes.WRQ:
                        packetStr = nullChar + (char)2;
                        break;
                }

                packetStr += RemoteFileName + nullChar + mode + nullChar + "blksize" +
                    nullChar + BlockSize.ToString() + nullChar + "tsize" + nullChar +
                    TransferSize.ToString() + nullChar + "timeout" + nullChar +
                    Timeout.ToString() + nullChar ;
                
                len = packetStr.Length;
                packet = new byte[len];

                // Encode packet as ASCII bytes
                packet = System.Text.Encoding.ASCII.GetBytes(packetStr);                                
                return packet;
            }            
            internal static byte[] Ack(int Block1, int Block2)
            {
                // ACK packet structure
                // ----------
                // |04|Block|
                // ----------
                byte[] packet = new byte[4];
                packet[0] = 0;
                packet[1] = (byte)TFTP.OpCodes.ACK;
                packet[2] = (byte)Block1;
                packet[3] = (byte)Block2;
                return packet;
            }
            internal static byte[] Data(byte[] SendData, int Block1, int Block2)
            {
                // DATA packet structure
                // ----------
                // |03|Block|
                // ----------
                byte[] packet = new byte[SendData.Length + 4];
                //packet[0] = 0;
                packet[1] = (byte)TFTP.OpCodes.DATA;
                packet[2] = (byte)Block1;
                packet[3] = (byte)Block2;
                for(int h = 4; h < SendData.Length + 4; h++)
                {
                    packet[h] = SendData[h - 4];
                }
                return packet;
            }
        }
        internal class Modify
        {
            internal static int[] IncrementBock(byte[] ReceivedData, int[] Block)
            {
                if (ReceivedData[3] == 255)
                {
                    if (ReceivedData[2] < 255)
                    {
                        Block[0] = (int)ReceivedData[2] + 1; Block[1] = 0;
                    }
                    else
                    {
                        Block[0] = 0; Block[1] = 0;
                    }
                }
                else
                {
                    Block[1] = (int)ReceivedData[3] + 1;
                }
                return Block;
            }
        }
    }
}