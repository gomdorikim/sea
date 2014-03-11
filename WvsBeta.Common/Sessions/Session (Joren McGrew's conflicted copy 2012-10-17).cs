using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using System.Net;
using System.Net.Sockets;

using System.Security.Cryptography;

namespace WvsBeta.Common.Sessions
{
    public class Session
    {
        /// <summary>
        /// Socket we use
        /// </summary>
        private Socket _socket;

        #region Data and encryption

        /// <summary>
        /// IV used for header generation and AES decryption
        /// </summary>
        private byte[] _decryptIV;

        /// <summary>
        /// IV used for header generation and AES encryption
        /// </summary>
        private byte[] _encryptIV;


        /// <summary>
        /// Buffer used for receiving packets.
        /// </summary>
        private byte[] _buffer;

        /// <summary>
        /// Position for receiving data.
        /// </summary>
        private int _bufferpos;

        /// <summary>
        /// Lenght of packet to receive.
        /// </summary>
        private int _bufferlen;

        private bool _header;
        private bool _encryption = false;
        private bool _receivingFromServer = true;

        private ushort _mapleVersion;
        private string _maplePatchLocation;
        private byte _mapleLocale;

        public ushort MapleVersion { get { return _mapleVersion; } }
        public string MaplePatchLocation { get { return _maplePatchLocation; } }
        public byte MapleLocale { get { return _mapleLocale; } }

        public bool Disconnected { get; private set; }

        public string TypeName { get; private set; }

        public string IP { get; private set; }
        public ushort Port { get; private set; }

        #endregion

        /// <summary>
        /// Creates a new instance of Session.
        /// </summary>
        /// <param name="pSocket">The socket we use</param>
        public Session(Socket pSocket, string tn)
        {
            TypeName = tn;
            Disconnected = false;
            _socket = pSocket;
            _receivingFromServer = false;

            IPEndPoint remoteIpEndPoint = _socket.RemoteEndPoint as IPEndPoint;
            IP = remoteIpEndPoint.Address.ToString();
            Port = (ushort)remoteIpEndPoint.Port;

            StartReading(4, true);
        }

        /// <summary>
        /// Connects to the server with the given IP and Port
        /// </summary>
        /// <param name="pIP">IP address to connect to.</param>
        /// <param name="pPort">Port to connect to.</param>
        public Session(string pIP, ushort pPort, string tn)
        {
            TypeName = tn;
            IP = pIP;
            Port = pPort;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Disconnected = true;
            _mapleVersion = 0;
            try
            {
                _socket.BeginConnect(pIP, pPort, new AsyncCallback(EndConnect), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(TypeName + " [ERROR] Could not connect to server @ {0}:{1}: {2}", pIP, pPort, ex.Message);
                throw ex;
            }
        }

        void EndConnect(IAsyncResult pIAR)
        {
            try
            {
                _socket.EndConnect(pIAR);
            }
            catch (Exception ex)
            {
                Console.WriteLine(TypeName + " [ERROR] Could not connect to server: {0}", ex.Message);
                return;
            }
            Console.WriteLine(TypeName + " Connected with server!");
            Disconnected = false;
            StartReading(2, true);
        }

        public void Disconnect()
        {
            if (Disconnected) return;
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }
            Console.WriteLine(TypeName + " Manual disconnection!");
            OnDisconnectINTERNAL();
        }

        /// <summary>
        /// Starts the reading mechanism.
        /// </summary>
        /// <param name="pLength">Amount of bytes to receive</param>
        /// <param name="pHeader">Do we receive a header?</param>
        private void StartReading(int pLength, bool pHeader = false)
        {
            if (Disconnected) return;
            _header = pHeader;
            _buffer = new byte[pLength];
            _bufferlen = pLength;
            _bufferpos = 0;
            ContinueReading();
        }

        /// <summary>
        /// Calls Socket.BeginReceive to receive more data.
        /// </summary>
        private void ContinueReading()
        {
            if (Disconnected) return;
            try
            {
                _socket.BeginReceive(_buffer, _bufferpos, _bufferlen - _bufferpos, SocketFlags.None, new AsyncCallback(EndReading), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(TypeName + " [ERROR] ContinueReading(): {0}", ex.ToString());
                OnDisconnectINTERNAL();
            }
        }

        /// <summary>
        /// Used as IAsyncResult parser for ContinueReading().
        /// </summary>
        /// <param name="pIAR">The result AsyncCallback makes</param>
        private void EndReading(IAsyncResult pIAR)
        {
            int amountReceived = 0;
            try
            {
                amountReceived = _socket.EndReceive(pIAR);
            }
            catch (Exception ex)
            {
                Console.WriteLine(TypeName + " : " + ex.ToString());
                amountReceived = 0;
            }
            if (amountReceived == 0)
            {
                // We got a disWvsBeta.Common.Sessions here!
                OnDisconnectINTERNAL();
                return;
            }

            // Add amount of bytes received to _bufferpos so we know if we got everything.
            _bufferpos += amountReceived;

            try
            {

                // Check if we got all data. There is _no_ way we would have received more bytes than needed. Period.
                if (_bufferpos == _bufferlen)
                {
                    // It seems we have all data we need
                    // Now check if we got a header
                    if (_header)
                    {
                        if (!_encryption && _receivingFromServer)
                        {
                            // Unencrypted packets have a short header with plain length.
                            ushort length = (ushort)(_buffer[0] | _buffer[1] << 8);
                            StartReading(length);
                        }
                        else
                        {
                            int length = GetHeaderLength(_buffer);
                            StartReading(length);
                        }
                    }
                    else
                    {
                        Packet packet;
                        if (_encryption)
                        {
                            _buffer = Decrypt(_buffer);
                            packet = new Packet(_buffer);

                            if (MasterThread.Instance != null)
                            {
                                MasterThread.Instance.AddCallback((date) =>
                                {
                                    try
                                    {
                                        OnPacketInbound(packet);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Handling Packet Error: {0}", ex.ToString());
                                    }
                                });
                            }
                            else
                            {
                                try
                                {
                                    OnPacketInbound(packet);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Handling Packet Error: {0}", ex.ToString());
                                }
                            }
                        }
                        else
                        {
                            _encryption = true; // First packet received or sent is unencrypted. All others are.
                            packet = new Packet(_buffer);

                            _mapleVersion = packet.ReadUShort();
                            _maplePatchLocation = _maplePatchLocation = packet.ReadString();
                            _encryptIV = packet.ReadBytes(4);
                            _decryptIV = packet.ReadBytes(4);
                            _mapleLocale = packet.ReadByte();
                            Console.WriteLine(TypeName + " MapleVersion: {0}; Patch Location: {1}; Locale: {2}", _mapleVersion, _maplePatchLocation, _mapleLocale);

                            packet.Reset();
                            if (MasterThread.Instance != null)
                            {
                                MasterThread.Instance.AddCallback((date) =>
                                {
                                    try
                                    {
                                        OnHandshakeInbound(packet);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Handling Packet Error: {0}", ex.ToString());
                                    }
                                });
                            }
                            else
                            {
                                try
                                {
                                    OnHandshakeInbound(packet);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Handling Packet Error: {0}", ex.ToString());
                                }
                            }
                        }

                        StartReading(4, true);
                    }
                }
                else
                {
                    ContinueReading();
                }

            }
            catch (SocketException socketException)
            {
                Console.WriteLine(TypeName + " Socket Exception while receiving data: {0}", socketException.Message);
                OnDisconnectINTERNAL();
            }
            catch (Exception ex)
            {
                Console.WriteLine(TypeName + " [ERROR] EndReading(): {0}", ex.ToString());
                OnDisconnectINTERNAL();
            }
        }

        public virtual void SendPacket(Packet pPacket)
        {
            SendData(pPacket.ToArray());
        }

        /// <summary>
        /// Sends bytes to the other side
        /// </summary>
        /// <param name="pData">Data to encrypt and send</param>
        public void SendData(byte[] pData)
        {
            if (!_encryption && !_receivingFromServer)
            {
                byte[] data = new byte[pData.Length + 2];
                data[0] = (byte)pData.Length;
                data[1] = (byte)((pData.Length >> 8) & 0xFF);
                Buffer.BlockCopy(pData, 0, data, 2, pData.Length);
                pData = data;
                _encryption = true; // First packet received or sent is unencrypted. All others are.
            }
            else
            {
                pData = Encrypt(pData);
            }
            try
            {
                int sent = _socket.Send(pData);
                if (sent != pData.Length)
                {
                    Console.WriteLine(TypeName + " [ERROR] Error while sending data: not all bytes transferred: only {0} of {1} sent!", sent, pData.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(TypeName + " [ERROR] Failed sending: {0}", ex.ToString());
                OnDisconnectINTERNAL();
            }
        }

        public string PrintPacket(Packet pPacket, string pWhat, bool ret = false) { return PrintPacket(pWhat, pPacket.ToArray(), false); }
        public string PrintPacket(string pWhat, byte[] pData, bool ret = false)
        {
            /*
            DateTime now = DateTime.Now;
            string buf = "[" + now.ToString("R") + "][" + TypeName + "] " + "(len: " + pData.Length + ")" + pWhat + " [";
            foreach (byte b in pData)
            {
                buf += string.Format("{0:X2} ", b);
            }
            buf = buf.Trim();
            buf += "]";
            Console.WriteLine(buf);
            */
            return "";
        }

        Random rnd = new Random();
        public void SendHandshake(ushort pVersion, string pPatchLocation, byte pLocale)
        {
            _encryptIV = new byte[4];
            rnd.NextBytes(_encryptIV);
            _decryptIV = new byte[4];
            rnd.NextBytes(_decryptIV);

            Packet packet = new Packet();
            packet.WriteUShort(pVersion);
            packet.WriteString(pPatchLocation);
            packet.WriteBytes(_decryptIV);
            packet.WriteBytes(_encryptIV);
            packet.WriteByte(pLocale);
            SendPacket(packet);
            _mapleVersion = pVersion;
            _maplePatchLocation = pPatchLocation;
            _mapleLocale = pLocale;
        }

        public virtual void OnPacketInbound(Packet pPacket)
        {
            Console.WriteLine(TypeName + " No Handler for 0x{0:X4}", pPacket.ReadUShort());
        }

        public virtual void OnHandshakeInbound(Packet pPacket)
        {
            Console.WriteLine(TypeName + " No Handshake Handler.");
        }

        private void OnDisconnectINTERNAL()
        {
            if (Disconnected) return;
            Disconnected = true;
            Console.WriteLine(TypeName + " Called by:");
            Console.WriteLine(Environment.StackTrace);
            OnDisconnect();
        }

        public virtual void OnDisconnect()
        {
            if (Disconnected) return;
            Disconnected = true;
            Console.WriteLine(TypeName + " Called by:");
            Console.WriteLine(Environment.StackTrace);
            Console.WriteLine(TypeName + " No Disconnect Handler.");
        }

        #region Encryption Stuff
        /// <summary>
        /// 256 bytes long shift key, used for MapleStory cryptography and header generation.
        /// </summary>
        private static byte[] sShiftKey = new byte[] {
            0xEC, 0x3F, 0x77, 0xA4, 0x45, 0xD0, 0x71, 0xBF, 0xB7, 0x98, 0x20, 0xFC, 0x4B, 0xE9, 0xB3, 0xE1,
            0x5C, 0x22, 0xF7, 0x0C, 0x44, 0x1B, 0x81, 0xBD, 0x63, 0x8D, 0xD4, 0xC3, 0xF2, 0x10, 0x19, 0xE0,
            0xFB, 0xA1, 0x6E, 0x66, 0xEA, 0xAE, 0xD6, 0xCE, 0x06, 0x18, 0x4E, 0xEB, 0x78, 0x95, 0xDB, 0xBA,
            0xB6, 0x42, 0x7A, 0x2A, 0x83, 0x0B, 0x54, 0x67, 0x6D, 0xE8, 0x65, 0xE7, 0x2F, 0x07, 0xF3, 0xAA,
            0x27, 0x7B, 0x85, 0xB0, 0x26, 0xFD, 0x8B, 0xA9, 0xFA, 0xBE, 0xA8, 0xD7, 0xCB, 0xCC, 0x92, 0xDA,
            0xF9, 0x93, 0x60, 0x2D, 0xDD, 0xD2, 0xA2, 0x9B, 0x39, 0x5F, 0x82, 0x21, 0x4C, 0x69, 0xF8, 0x31,
            0x87, 0xEE, 0x8E, 0xAD, 0x8C, 0x6A, 0xBC, 0xB5, 0x6B, 0x59, 0x13, 0xF1, 0x04, 0x00, 0xF6, 0x5A,
            0x35, 0x79, 0x48, 0x8F, 0x15, 0xCD, 0x97, 0x57, 0x12, 0x3E, 0x37, 0xFF, 0x9D, 0x4F, 0x51, 0xF5,
            0xA3, 0x70, 0xBB, 0x14, 0x75, 0xC2, 0xB8, 0x72, 0xC0, 0xED, 0x7D, 0x68, 0xC9, 0x2E, 0x0D, 0x62,
            0x46, 0x17, 0x11, 0x4D, 0x6C, 0xC4, 0x7E, 0x53, 0xC1, 0x25, 0xC7, 0x9A, 0x1C, 0x88, 0x58, 0x2C,
            0x89, 0xDC, 0x02, 0x64, 0x40, 0x01, 0x5D, 0x38, 0xA5, 0xE2, 0xAF, 0x55, 0xD5, 0xEF, 0x1A, 0x7C,
            0xA7, 0x5B, 0xA6, 0x6F, 0x86, 0x9F, 0x73, 0xE6, 0x0A, 0xDE, 0x2B, 0x99, 0x4A, 0x47, 0x9C, 0xDF,
            0x09, 0x76, 0x9E, 0x30, 0x0E, 0xE4, 0xB2, 0x94, 0xA0, 0x3B, 0x34, 0x1D, 0x28, 0x0F, 0x36, 0xE3,
            0x23, 0xB4, 0x03, 0xD8, 0x90, 0xC8, 0x3C, 0xFE, 0x5E, 0x32, 0x24, 0x50, 0x1F, 0x3A, 0x43, 0x8A,
            0x96, 0x41, 0x74, 0xAC, 0x52, 0x33, 0xF0, 0xD9, 0x29, 0x80, 0xB1, 0x16, 0xD3, 0xAB, 0x91, 0xB9,
            0x84, 0x7F, 0x61, 0x1E, 0xCF, 0xC5, 0xD1, 0x56, 0x3D, 0xCA, 0xF4, 0x05, 0xC6, 0xE5, 0x08, 0x49
        };

        /// <summary>
        /// Encrypts the given data, and updates the Encrypt IV
        /// </summary>
        /// <param name="pData">Data to be encrypted (without header!)</param>
        /// <returns>Encrypted data (with header!)</returns>
        private byte[] Encrypt(byte[] pData)
        {
            // Include header
            byte[] data = new byte[pData.Length + 4];

            GenerateHeader(data, _encryptIV, pData.Length, _mapleVersion, _receivingFromServer);

            EncryptMSCrypto(pData);
            // AESTransform(pData, _encryptIV);
            NextIV(_encryptIV);
            Buffer.BlockCopy(pData, 0, data, 4, pData.Length);

            return data;
        }

        /// <summary>
        /// Decrypts given data, and updates the Decrypt IV
        /// </summary>
        /// <param name="pData">Data to be decrypted</param>
        /// <returns>Decrypted data</returns>
        private byte[] Decrypt(byte[] pData)
        {
            // AESTransform(pData, _decryptIV);
            NextIV(_decryptIV);
            DecryptMSCrypto(pData);
            return pData;
        }
    
        /// <summary>
        /// Rolls the value left. Port from NLS (C++) _rotl8
        /// </summary>
        /// <param name="value">Value to be shifted</param>
        /// <param name="shift">Position to shift to</param>
        /// <returns>Shifted value</returns>
        private static byte RollLeft(byte value, int shift)
        {
            uint overflow = ((uint)value) << (shift % 8);
            return (byte)((overflow & 0xFF) | (overflow >> 8));
        }

        /// <summary>
        /// Rolls the value right. Port from NLS (C++) _rotr8
        /// </summary>
        /// <param name="value">Value to be shifted</param>
        /// <param name="shift">Position to shift to</param>
        /// <returns>Shifted value</returns>
        private static byte RollRight(byte value, int shift)
        {
            uint overflow = (((uint)value) << 8) >> (shift % 8);
            return (byte)((overflow & 0xFF) | (overflow >> 8));
        }

        /// <summary>
        /// Encrypts given data with the MapleStory cryptography
        /// </summary>
        /// <param name="pData">Unencrypted data</param>
        private static void EncryptMSCrypto(byte[] pData)
        {
            int length = pData.Length, j;
            byte a, c;
            for (var i = 0; i < 3; i++)
            {
                a = 0;
                for (j = length; j > 0; j--)
                {
                    c = pData[length - j];
                    c = RollLeft(c, 3);
                    c = (byte)(c + j);
                    c ^= a;
                    a = c;
                    c = RollRight(a, j);
                    c ^= 0xFF;
                    c += 0x48;
                    pData[length - j] = c;
                }
                a = 0;
                for (j = length; j > 0; j--)
                {
                    c = pData[j - 1];
                    c = RollLeft(c, 4);
                    c = (byte)(c + j);
                    c ^= a;
                    a = c;
                    c ^= 0x13;
                    c = RollRight(c, 3);
                    pData[j - 1] = c;
                }
            }
        }

        /// <summary>
        /// Decrypts given data with the MapleStory cryptography
        /// </summary>
        /// <param name="pData"></param>
        private static void DecryptMSCrypto(byte[] pData)
        {
            int length = pData.Length, j;
            byte a, b, c;
            for (var i = 0; i < 3; i++)
            {
                a = 0;
                b = 0;
                for (j = length; j > 0; j--)
                {
                    c = pData[j - 1];
                    c = RollLeft(c, 3);
                    c ^= 0x13;
                    a = c;
                    c ^= b;
                    c = (byte)(c - j);
                    c = RollRight(c, 4);
                    b = a;
                    pData[j - 1] = c;
                }
                a = 0;
                b = 0;
                for (j = length; j > 0; j--)
                {
                    c = pData[length - j];
                    c -= 0x48;
                    c ^= 0xFF;
                    c = RollLeft(c, j);
                    a = c;
                    c ^= b;
                    c = (byte)(c - j);
                    c = RollRight(c, 3);
                    b = a;
                    pData[length - j] = c;
                }
            }
        }

        /// <summary>
        /// Generates a new IV code for AES and header generation. It will reset the oldIV with the newIV automatically.
        /// </summary>
        /// <param name="pOldIV">The old IV that is used already.</param>
        private static void NextIV(byte[] pOldIV)
        {
            byte[] newIV = new byte[] { 0xF2, 0x53, 0x50, 0xC6 };
            for (var i = 0; i < 4; i++)
            {
                byte input = pOldIV[i];
                byte tableInput = sShiftKey[input];
                newIV[0] += (byte)(sShiftKey[newIV[1]] - input);
                newIV[1] -= (byte)(newIV[2] ^ tableInput);
                newIV[2] ^= (byte)(sShiftKey[newIV[3]] + input);
                newIV[3] -= (byte)(newIV[0] - tableInput);

                uint val = BitConverter.ToUInt32(newIV, 0);
                uint val2 = val >> 0x1D;
                val <<= 0x03;
                val2 |= val;
                newIV[0] = (byte)(val2 & 0xFF);
                newIV[1] = (byte)((val2 >> 8) & 0xFF);
                newIV[2] = (byte)((val2 >> 16) & 0xFF);
                newIV[3] = (byte)((val2 >> 24) & 0xFF);
            }
            Buffer.BlockCopy(newIV, 0, pOldIV, 0, 4);
        }

        /// <summary>
        /// Retrieves length of content from the header
        /// </summary>
        /// <param name="pBuffer">Buffer containing the header</param>
        /// <returns>Length of buffer</returns>
        private static int GetHeaderLength(byte[] pBuffer) {
            int length = (int)pBuffer[0] |
                         (int)(pBuffer[1] << 8) |
                         (int)(pBuffer[2] << 16) |
                         (int)(pBuffer[3] << 24);
            length = (length >> 16) ^ (length & 0xFFFF);
            return (ushort)length;
        }

        /// <summary>
        /// Generates header for packets
        /// </summary>
        /// <param name="pBuffer">Buffer</param>
        /// <param name="pIV">IV</param>
        /// <param name="pLength">Packet Length - Header Length</param>
        /// <param name="pVersion">MapleStory Version</param>
        /// <param name="pToServer">Is to server?</param>
        private static void GenerateHeader(byte[] pBuffer, byte[] pIV, int pLength, ushort pVersion, bool pToServer)
        {
            int a = (pIV[3] << 8) | pIV[2];
            byte[] header = new byte[4];
            if (pToServer)
            {
                a = a ^ (pVersion);
                int b = a ^ pLength;
                header[0] = (byte)(a % 0x100);
                header[1] = (byte)(a / 0x100);
                header[2] = (byte)(b % 0x100);
                header[3] = (byte)(b / 0x100);
                Buffer.BlockCopy(header, 0, pBuffer, 0, 4);
            }
            else
            {
                a ^= -(pVersion + 1);
                int b = a ^ pLength;
                header[0] = (byte)(a % 0x100);
                header[1] = (byte)((a - header[0]) / 0x100);
                header[2] = (byte)(b ^ 0x100);
                header[3] = (byte)((b - header[2]) / 0x100);
            }
            Buffer.BlockCopy(header, 0, pBuffer, 0, 4);
        }

        #region AES

        /// <summary>
        /// 'Secret' 8 * 4 byte long key used for AES encryption.
        /// </summary>
        private static byte[] sSecretKey = new byte[] {
            0x13, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0xB4, 0x00, 0x00, 0x00,
            0x1B, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0x52, 0x00, 0x00, 0x00 
        };

        private static RijndaelManaged _rijndaelAES = new RijndaelManaged();
        private static ICryptoTransform _transformer = null;

        /// <summary>
        /// Transforms given buffer with AES + given IV
        /// </summary>
        /// <param name="pData">Data to be transformed</param>
        /// <param name="pIV">IV used to transform data</param>
        private static void AESTransform(byte[] pData, byte[] pIV)
        {
            if (_transformer == null)
            {
                _rijndaelAES.Key = sSecretKey;
                _rijndaelAES.Mode = CipherMode.ECB;
                _rijndaelAES.Padding = PaddingMode.PKCS7;
                _transformer = _rijndaelAES.CreateEncryptor();
            }
            int remaining = pData.Length;
            int length = 0x5B0;
            int start = 0;
            byte[] realIV = new byte[pIV.Length * 4];
            while (remaining > 0)
            {
                for (int index = 0; index < realIV.Length; ++index) realIV[index] = pIV[index % 4];

                if (remaining < length) length = remaining;
                for (int index = start; index < (start + length); ++index)
                {
                    if (((index - start) % realIV.Length) == 0)
                    {
                        byte[] tempIV = new byte[realIV.Length];
                        _transformer.TransformBlock(realIV, 0, realIV.Length, tempIV, 0);
                        Buffer.BlockCopy(tempIV, 0, realIV, 0, realIV.Length);
                    }
                    pData[index] ^= realIV[(index - start) % realIV.Length];
                }
                start += length;
                remaining -= length;
                length = 0x5B4;
            }
        }

        #endregion
        #endregion
    }
}
