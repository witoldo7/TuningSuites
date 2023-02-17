using System;
using System.Collections.Generic;
using System.IO;
using CommonSuite;
using NLog;
using TrionicCANLib.Checksum;

namespace T8SuitePro
{
    class T8Header
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        private FlashBlockCollection fbc = new FlashBlockCollection();

        public FlashBlockCollection FlashBlocks
        {
            get { return fbc; }
        }

        private string m_fileName;

        private int _numberOfFlashBlocks = 0;

        public int NumberOfFlashBlocks
        {
            get { return _numberOfFlashBlocks; }
            set { _numberOfFlashBlocks = value; }
        }

        private string m_interfaceDevice = string.Empty;

        public string InterfaceDevice
        {
            get { return m_interfaceDevice; }
            set { m_interfaceDevice = value; }
        }

        private string m_ecuDescription = string.Empty;

        public string EcuDescription
        {
            get { return m_ecuDescription; }
            set { m_ecuDescription = value; }
        }

        private string m_programmerDevice = string.Empty;

        public string ProgrammerDevice
        {
            get { return m_programmerDevice; }
            set { m_programmerDevice = value; }
        }
        private string m_programmerName = string.Empty;

        public string ProgrammerName
        {
            get { return m_programmerName; }
            set { m_programmerName = value; }
        }
        private string m_releaseDate = string.Empty;

        public string ReleaseDate
        {
            get { return m_releaseDate; }
            set { m_releaseDate = value; }
        }

        private string m_deviceType = string.Empty;

        public string DeviceType
        {
            get { return m_deviceType; }
            set { m_deviceType = value; }
        }

        private string m_hardwareID = string.Empty;

        public string HardwareID
        {
            get { return m_hardwareID; }
            set { m_hardwareID = value; }
        }

        private string m_SerialNumber = string.Empty;

        public string SerialNumber
        {
            get { return m_SerialNumber; }
            set { m_SerialNumber = value; }
        }
        private string m_SoftwareVersion = string.Empty;

        public string SoftwareVersion
        {
            get { return m_SoftwareVersion; }
            set { m_SoftwareVersion = value; }
        }
        private string m_CarDescription = string.Empty;

        public string CarDescription
        {
            get { return m_CarDescription; }
            set { m_CarDescription = value; }
        }
        private string m_ChassisID = string.Empty;

        public string ChassisID
        {
            get { return m_ChassisID; }
            set { m_ChassisID = value; }
        }

        private string m_SecretCode = string.Empty;

        public string SecretCode
        {
            get { return m_SecretCode; }
            set { m_SecretCode = value; }
        }

        private string m_PartNumber = string.Empty;

        public string PartNumber
        {
            get { return m_PartNumber; }
            set { m_PartNumber = value; }
        }

        /// <summary>
        /// FileHeaderField represents a field in the file header.
        /// Each field consists of a field ID, a field length and data.
        /// </summary>
        class FileHeaderField
        {
            //public byte m_fieldID;
            //public byte m_fieldLength;
            public byte[] m_data = new byte[255];
        }

        public static int GetEmptySpaceStartFrom(string filename, int offset)
        {
            int retval = 0;
            if (filename == "") return retval;
            FileStream fsread = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using (BinaryReader br = new BinaryReader(fsread))
            {
                fsread.Seek(offset, SeekOrigin.Begin);
                bool found = false;
                while (!found && fsread.Position < offset + 0x1000)
                {
                    retval = (int)fsread.Position;
                    byte b1 = br.ReadByte();
                    if (b1 == 0xFF)
                    {
                        byte b2 = br.ReadByte();
                        if (b2 == 0xFF)
                        {
                            byte b3 = br.ReadByte();
                            if (b3 == 0xFF) found = true;
                        }
                    }
                }
            }
            fsread.Close();
            return retval;
        }

        private byte[] readdatafromfile(string filename, int address, int length)
        {
            byte[] retval = new byte[length];
            try
            {
                FileStream fsi1 = File.OpenRead(filename);
                while (address > fsi1.Length) address -= (int)fsi1.Length;
                BinaryReader br1 = new BinaryReader(fsi1);
                fsi1.Position = address;
                string temp = string.Empty;
                for (int i = 0; i < length; i++)
                {
                    retval.SetValue(br1.ReadByte(), i);
                }
                fsi1.Flush();
                br1.Close();
                fsi1.Close();
                fsi1.Dispose();
            }
            catch (Exception E)
            {
                logger.Debug(E);
            }
            return retval;
        }

        private void DecodeInfo(string filename)
        {
            if (filename == "") return;
            int checksumAreaOffset = ChecksumT8.GetChecksumAreaOffset(filename);
            int endOfPIArea = GetEmptySpaceStartFrom(filename, checksumAreaOffset);

            //logger.Debug("Area: " + checksumAreaOffset.ToString("X8") + " - " + endOfPIArea.ToString("X8"));
            byte[] piarea = readdatafromfile(filename, checksumAreaOffset, endOfPIArea - checksumAreaOffset + 1);
            //logger.Debug("Size: " + piarea.Length.ToString());
            for (int t = 0; t < piarea.Length; t++)
            {
                piarea[t] += 0xD6;
                piarea[t] ^= 0x21;

            }
            int i = 0;
            int len = 0;
            int type = 0;
            do
            {
                if (i == piarea.Length) break;
                len = Convert.ToInt32(piarea[i++]);
                type = Convert.ToInt32(piarea[i++]);
                if (len == 0xF7 && type == 0xF7) break;
                //printf("\nLENGTH = %02X, TYPE = %02X, text = ", len, type);

                string data = string.Empty;
                try
                {
                    for (int f = 0; f < len; f++)
                    {

                        if (type == 0x92 || type == 0x97 || type == 0x0C || type == 0xC1 || type == 0x08 || type == 0x1D || type == 0x10 || type == 0x0A || type == 0x0F || type == 0x16)
                        {
                            data += Convert.ToChar(piarea[i++]);
                        }
                        else
                        {
                            data += piarea[i++].ToString("X2");
                        }
                    }
                }
                catch (Exception E)
                {
                    logger.Debug(E.Message);
                }
                logger.Debug("Len: " + len.ToString("X2") + " Type = " + type.ToString("X2") + "   " + data);
                /*
Len: 10 Type = 0D   58 C3 25 2D 92 B3 2D 82 95 E5 E4 23 15 E3 A4 55  // layer 1 checksum
Len: 09 Type = 92   GMPT 0100		//Hardware ID
Len: 14 Type = 97   ECM  		//Type of device               
Len: 02 Type = 9A   01 08 		//revision???
Len: 01 Type = 1E   01 
Len: 03 Type = 0C   82h			//version extension?
Len: 04 Type = CB   03 4F C5 10 
Len: 02 Type = DB   20 20 
Len: 08 Type = C1   55559436
Len: 02 Type = D1   20 20 
Len: 1E Type = 08   FA5I_C_FME2_65_FIEF_82h       //version?
Len: 02 Type = 95   00 04 
Len: 06 Type = 96   2D 20 20 20 20 20 
Len: 04 Type = FB   02 70 84 9B 	//layer2 checksum
Len: 04 Type = FC   00 0B AF 06 	//CHECKSUM LAST ADDRESS
Len: 04 Type = FD   00 02 00 00 	//ROM OFFSET
Len: 06 Type = 16   p50660
Len: 13 Type = 0A   2004-08-12 14:13:34		// release date
Len: 1E Type = 0F   FA5I_C_FME2_XX_XXX_XXX.tmp    
Len: 0F Type = 1D   Stefan Mossberg	//programmed by name?
Len: 0C Type = 10   EOLStation2		//programmed by device                 * */
                switch (type)
                {
                    case 0x10:
                        m_programmerDevice = data;
                        break;
                    case 0x1D:
                        m_programmerName = data;
                        break;
                    case 0x0A:
                        m_releaseDate = data;
                        break;
                    case 0x08:
                        m_SoftwareVersion = data;
                        break;
                    case 0xC1:
                        m_PartNumber = data;
                        break;
                    case 0x92:
                        m_hardwareID = data;
                        break;
                    case 0x97:
                        m_deviceType = data;
                        break;
                }
            } while (i < piarea.Length - 1);

            //TODO: decode vin parts etc
            DecodeExtraInfo(filename);
            DumpThisData(piarea, @"C:\t8decode\" + Path.GetFileNameWithoutExtension(filename) + "_piarea.bin");
        }

        private int[] lowaddress = new int[32];
        private int[] lowtypes = new int[32];
        private int[] highaddress = new int[32];
        private int[] hightypes = new int[32];

        private void DecodeExtraInfo(string filename)
        {
            AddToHeaderLog("DecodeExtraInfo begins for : " + Path.GetFileName(filename));
            // get the info that is stored in coded form in blocks from 4000
            // read all flashblocks
            byte[] file_data = readdatafromfile(filename, 0x4000, 0x4000);
            // example: 
            //44 2A 02 00 00 00 40 CE 00 FF FF FF 	// info block?
            //44 2A 01 30 00 00 42 D8 01 00 00 03 *
            // the last FF FF FF points us to the segment where non coded into is stored like the immo code
            // the partnumber etc
            // read that first
            // @ 0x4004 and 0x6004 there are two 4 byte counters which indicate which block (0x4000 or 0x6000 was last written, so active?)
            // <*MFS><4bytecounter>FFFFFFFFFF<D*> in which D* indicates the start of the first header
            int blockNumber = 0;
            int segmentBlocks = 0;
            int _lowSegmentCounter = 0;
            int _highSegmentCounter = 0;

            _lowSegmentCounter = Convert.ToInt32(file_data[0x0004]) * 256 * 256 * 256;
            _lowSegmentCounter += Convert.ToInt32(file_data[0x0005]) * 256 * 256;
            _lowSegmentCounter += Convert.ToInt32(file_data[0x0006]) * 256;
            _lowSegmentCounter += Convert.ToInt32(file_data[0x0007]);

            _highSegmentCounter = Convert.ToInt32(file_data[0x2004]) * 256 * 256 * 256;
            _highSegmentCounter += Convert.ToInt32(file_data[0x2005]) * 256 * 256;
            _highSegmentCounter += Convert.ToInt32(file_data[0x2006]) * 256;
            _highSegmentCounter += Convert.ToInt32(file_data[0x2007]);
            // Thread.Sleep(100); // <to get the debug output correct>

            // get the addresses of the lower flash blocks

            int idx = 0x0E;
            int lowBlkCnt = 0;
            int lowAddressIdx = 0;
            for (int t = 0; t < 32; t++)
            {
                string descriptor = string.Empty;
                descriptor += file_data[idx].ToString("X2") + " ";
                descriptor += file_data[idx + 1].ToString("X2") + " ";
                descriptor += file_data[idx + 2].ToString("X2") + " ";
                descriptor += file_data[idx + 3].ToString("X2") + " ";
                descriptor += file_data[idx + 4].ToString("X2") + " ";
                descriptor += file_data[idx + 5].ToString("X2") + " ";
                AddToHeaderLog(descriptor);
                if (file_data[idx] != 0x44 || file_data[idx + 1] != 0x2A)
                {
                    if (lowBlkCnt == 0)
                    {
                        lowaddress[lowAddressIdx] = Convert.ToInt32(file_data[idx]) * 256 + Convert.ToInt32(file_data[idx + 1]);
                        lowtypes[lowAddressIdx] = Convert.ToInt32(file_data[idx + 5]);
                        lowAddressIdx++;
                    }
                    if (lowBlkCnt >= 2)
                    {
                        lowaddress[lowAddressIdx] = Convert.ToInt32(file_data[idx]) * 256 + Convert.ToInt32(file_data[idx + 1]);
                        lowtypes[lowAddressIdx] = Convert.ToInt32(file_data[idx + 5]); ;
                        lowAddressIdx++;
                    }
                    lowBlkCnt++;

                }
                idx += 6;
            }

            int highBlkCnt = 0;
            int highAddressIdx = 0;
            idx = 0x200E;
            for (int t = 0; t < 32; t++)
            {
                if (file_data[idx] != 0x44 || file_data[idx + 1] != 0x2A)
                {
                    if (highBlkCnt == 0)
                    {
                        highaddress[highAddressIdx] = Convert.ToInt32(file_data[idx]) * 256 + Convert.ToInt32(file_data[idx + 1]);
                        hightypes[highAddressIdx] = Convert.ToInt32(file_data[idx + 5]);
                        highAddressIdx++;
                    }
                    if (highBlkCnt >= 2)
                    {
                        highaddress[highAddressIdx] = Convert.ToInt32(file_data[idx]) * 256 + Convert.ToInt32(file_data[idx + 1]);
                        hightypes[highAddressIdx] = Convert.ToInt32(file_data[idx + 5]); ;
                        highAddressIdx++;
                    }
                    highBlkCnt++;

                }
                idx += 6;

            }
            AddToHeaderLog("Low segment headers");

            for (int t = 0; t < 32; t++)
            {
                if (lowaddress[t] != 0 && lowaddress[t] != 0xFFFF)
                {
                    AddToHeaderLog("Found low address: " + lowaddress[t].ToString("X8") + " type: " + lowtypes[t].ToString("X2"));
                    // AddToHeaderLog("Low segment headers address : " + address.ToString("X8") + " type: " + type.ToString("X4"));

                    switch (lowtypes[t])
                    {
                        case 0x01: // history record pointer, ignore

                            /* debug */
                            AddToHeaderLog("LOW history record: " + blockNumber.ToString());
                            FlashBlock fb01 = new FlashBlock();
                            fb01.BlockType = lowtypes[t];
                            fb01.BlockAddress = lowaddress[t];
                            byte[] block_data01 = new byte[0x130];
                            for (int bt = 0; bt < 0x130; bt++)
                            {
                                block_data01[bt] = file_data[bt + lowaddress[t] - 0x4000];
                            }
                            fb01.BlockData = block_data01;
                            if (fb01.isValid())
                            {
                                fb01.BlockNumber = blockNumber;
                                fbc.Add(fb01);
                                blockNumber++;
                                segmentBlocks++;
                            }
                            /* debug */

                            break;
                        case 0x03: // active record pointer
                            AddToHeaderLog("LOW active record: " + blockNumber.ToString());

                            FlashBlock fb = new FlashBlock();
                            fb.BlockType = lowtypes[t];
                            fb.BlockAddress = lowaddress[t];
                            byte[] block_data = new byte[0x130];
                            for (int bt = 0; bt < 0x130; bt++)
                            {
                                block_data[bt] = file_data[bt + lowaddress[t] - 0x4000];
                            }
                            fb.BlockData = block_data;
                            if (fb.isValid())
                            {
                                fb.BlockNumber = blockNumber;
                                fbc.Add(fb);
                                blockNumber++;
                                segmentBlocks++;
                            }
                            break;
                        case 0xFF: // information record pointer
                            // read this record and fill immocode etc
                            AddToHeaderLog("LOW 0xFF record: " + blockNumber.ToString());
                            try
                            {
                                string t_PartNumber = "";
                                int pc = 0;
                                for (pc = 0; pc < 10; pc++)
                                {
                                    t_PartNumber += Convert.ToChar(file_data[lowaddress[t] - 0x4000 + pc]);
                                }
                                m_PartNumber = t_PartNumber.Trim();
                                // first 10 bytes are serialnumber (but different from PI-area?)
                                // 16 bytes immocode
                                string t_SerialNumber = "";
                                for (pc = 0; pc < 16; pc++)
                                {
                                    t_SerialNumber += Convert.ToChar(file_data[lowaddress[t] - 0x4000 + pc + 10]);
                                }
                                m_SerialNumber = t_SerialNumber.Trim();
                            }
                            catch (Exception E)
                            {
                                AddToHeaderLog("Failed to process low header 0xFF record: " + E.Message);
                            }
                            break;
                    }
                    string dbg = string.Empty;
                    for (int bt = 0; bt < 12; bt++)
                    {
                        dbg += file_data[0x0E + ((t * 24) + bt)].ToString("X2") + " ";
                    }
                    AddToHeaderLog(dbg);
                }
            }
            for (int t = 0; t < 32; t++)
            {
                if (highaddress[t] != 0 && highaddress[t] != 0xFFFF)
                {
                    AddToHeaderLog("Found high address: " + highaddress[t].ToString("X8") + " type: " + hightypes[t].ToString("X2"));
                    switch (hightypes[t])
                    {
                        case 0x01: // history record pointer, ignore
                            /* debug */
                            AddToHeaderLog("HIGH history record: " + blockNumber.ToString());

                            FlashBlock fb01 = new FlashBlock();
                            fb01.BlockAddress = highaddress[t];
                            fb01.BlockType = hightypes[t];
                            byte[] block_data01 = new byte[0x130];
                            for (int bt = 0; bt < 0x130; bt++)
                            {
                                block_data01[bt] = file_data[bt + highaddress[t] - 0x4000];
                            }
                            fb01.BlockData = block_data01;
                            if (fb01.isValid())
                            {
                                fb01.BlockNumber = blockNumber;
                                fbc.Add(fb01);
                                blockNumber++;
                                segmentBlocks++;
                            }
                            /* debug */
                            break;
                        case 0x03: // active record pointer
                            AddToHeaderLog("HIGH active record: " + blockNumber.ToString());

                            FlashBlock fb = new FlashBlock();
                            fb.BlockAddress = highaddress[t];
                            fb.BlockType = hightypes[t];
                            byte[] block_data = new byte[0x130];
                            for (int bt = 0; bt < 0x130; bt++)
                            {
                                block_data[bt] = file_data[bt + highaddress[t] - 0x4000];
                            }
                            fb.BlockData = block_data;
                            if (fb.isValid())
                            {
                                fb.BlockNumber = blockNumber;
                                fbc.Add(fb);
                                blockNumber++;
                                segmentBlocks++;
                            }
                            break;
                        case 0xFF: // information record pointer
                            // read this record and fill immocode etc
                            AddToHeaderLog("HIGH 0xFF record: " + blockNumber.ToString());
                            try
                            {
                                string t_PartNumber = "";
                                int pc = 0;
                                for (pc = 0; pc < 10; pc++)
                                {
                                    t_PartNumber += Convert.ToChar(file_data[highaddress[t] - 0x6000 + pc]);
                                }
                                m_PartNumber = t_PartNumber.Trim();
                                // first 10 bytes are serialnumber (but different from PI-area?)
                                // 16 bytes immocode
                                string t_serialNumber = "";
                                for (pc = 0; pc < 16; pc++)
                                {
                                    t_serialNumber += Convert.ToChar(file_data[highaddress[t] - 0x6000 + pc + 10]);
                                }
                                m_SerialNumber = t_serialNumber.Trim();
                            }
                            catch (Exception E)
                            {
                                AddToHeaderLog("Failed to process high header 0xFF record: " + E.Message);
                            }
                            break;
                    }
                    string dbg = string.Empty;
                    for (int bt = 0; bt < 12; bt++)
                    {
                        dbg += file_data[0x200E + ((t * 24) + bt)].ToString("X2") + " ";
                    }
                    AddToHeaderLog(dbg);
                }
            }

            // first find 0x78 0x5A 0x84 0xE6... marks the start of this stuff, there are two halves both 0x2000 bytes long
            // every block = 0x130 bytes so a maximum of 1A blocks. Substract the offset 0x2d8 in each half which means 2 blocks.
            // That leaves a maximum number of blocks of 0x18 (24) in each half.
            int iblock = 0;
            DumpThisData(file_data, @"C:\t8decode\" + Path.GetFileNameWithoutExtension(filename) + "_memdump.bin");
            foreach (FlashBlock fbcheck in fbc)
            {
                iblock++;
                string _vin = string.Empty;
                string _interfacedevice = string.Empty;
                string _ecudescription = string.Empty;
                string _secretcode = string.Empty;
                fbcheck.DecodeBlock(out _vin, out _ecudescription, out _interfacedevice, out _secretcode);
                DumpThisData(fbcheck.BlockData, @"C:\T8Decode\flashblock" + iblock.ToString() + ".blk");
                AddToHeaderLog("Found (" + fbcheck.BlockNumber.ToString() + ") VIN: " + _vin + " address: " + fbcheck.BlockAddress.ToString("X8"));
                AddToHeaderLog("Found (" + fbcheck.BlockNumber.ToString() + ") ECU: " + _ecudescription);
                AddToHeaderLog("Found (" + fbcheck.BlockNumber.ToString() + ") ITF: " + _interfacedevice);
                AddToHeaderLog("Found (" + fbcheck.BlockNumber.ToString() + ") SEC: " + _secretcode);
                if (m_ChassisID != _vin) m_ChassisID = _vin;
                if (m_SecretCode != _secretcode) m_SecretCode = _secretcode;
                if (m_interfaceDevice != _interfacedevice) m_interfaceDevice = _interfacedevice;
                if (m_ecuDescription != _ecudescription) m_ecuDescription = _ecudescription;
            }
            _numberOfFlashBlocks = fbc.Count;
        }



        private void DumpThisData(byte[] data, string filename)
        {
            if (filename != null)
            {
                if (filename != string.Empty)
                {
                    if (Directory.Exists(Path.GetDirectoryName(filename)))
                    {
                        FileStream fs = new FileStream(filename, FileMode.Create);
                        using (BinaryWriter bw = new BinaryWriter(fs))
                        {
                            bw.Write(data);
                        }
                        fs.Close();
                    }
                }
            }
        }

        public bool init(string a_filename)
        {
            m_SerialNumber = string.Empty;
            m_SoftwareVersion = string.Empty;
            m_CarDescription = string.Empty;
            m_PartNumber = string.Empty;
            m_fileName = a_filename;
            DecodeInfo(a_filename);
            return true;
        }

        private int GetInfoBlockAddress()
        {
            int retval = 0;
            AddToHeaderLog("GetInfoBlockAddress begins");
            byte[] file_data = readdatafromfile(m_fileName, 0x4000, 0x4000);
            int _lowSegmentCounter = 0;
            int _highSegmentCounter = 0;
            _lowSegmentCounter = Convert.ToInt32(file_data[0x0004]) * 256 * 256 * 256;
            _lowSegmentCounter += Convert.ToInt32(file_data[0x0005]) * 256 * 256;
            _lowSegmentCounter += Convert.ToInt32(file_data[0x0006]) * 256;
            _lowSegmentCounter += Convert.ToInt32(file_data[0x0007]);
            _highSegmentCounter = Convert.ToInt32(file_data[0x2004]) * 256 * 256 * 256;
            _highSegmentCounter += Convert.ToInt32(file_data[0x2005]) * 256 * 256;
            _highSegmentCounter += Convert.ToInt32(file_data[0x2006]) * 256;
            _highSegmentCounter += Convert.ToInt32(file_data[0x2007]);
            int startAddress = 0;
            if (_highSegmentCounter > _lowSegmentCounter) startAddress = 0x2000;

            for (int t = 0; t < 32; t++)
            {
                // get the address
                if (file_data[startAddress + 0x0E + ((t * 24))] == 0x44 && file_data[startAddress + 0x0E + ((t * 24)) + 1] == 0x2A)
                {
                    int address = Convert.ToInt32(file_data[startAddress + 0x0E + ((t * 24)) + 6]) * 256 + Convert.ToInt32(file_data[startAddress + 0x0E + ((t * 24)) + 7]);
                    int type = Convert.ToInt32(file_data[startAddress + 0x0E + ((t * 24)) + 11]);
                    switch (type)
                    {
                        case 0x01: // history record pointer, ignore
                            break;
                        case 0x03: // active record pointer
                            break;
                        case 0xFF: // information record pointer
                            // read this record and fill immocode etc
                            retval = address;

                            break;
                    }
                }
            }
            return retval;
        }

        private void savedatatobinary(int address, int length, byte[] data, string filename)
        {
            FileStream fsi1 = File.OpenWrite(filename);
            BinaryWriter bw1 = new BinaryWriter(fsi1);
            fsi1.Position = address;

            for (int i = 0; i < length; i++)
            {
                bw1.Write((byte)data.GetValue(i));
            }
            fsi1.Flush();
            bw1.Close();
            fsi1.Close();
            fsi1.Dispose();
        }

        private void AddToHeaderLog(string item)
        {
            if (Directory.Exists(@"C:\T8Decode"))
            {
                using (StreamWriter sw = new StreamWriter(@"C:\T8Decode\flashblocks.log", true))
                {
                    sw.WriteLine(item);
                }
            }
        }

        internal bool UpdateBlock()
        {
            UpdateBlock(true, true, true, true);
            return true;
        }

        private bool UpdateSerialNumber(string serialNumber)
        {
            if (fbc != null)
            {
                int pc = 0;
                byte[] serialBytes = new byte[16];
                serialNumber = serialNumber.PadRight(16, ' ');
                for (pc = 0; pc < 16; pc++)
                {
                    serialBytes[pc] = Convert.ToByte(serialNumber[pc]);
                }
                for (int i = 0; i < lowtypes.Length; i++)
                {
                    // get the type of block?
                    if (lowtypes[i] == 0xFF && lowaddress[i] != 0 && lowaddress[i] != 0xFFFF)
                    {
                        logger.Debug("Updating for serialNumber: " + lowaddress[i].ToString("X8"));
                        savedatatobinary(lowaddress[i] + 10, 16, serialBytes, m_fileName);
                    }
                }
                for (int i = 0; i < hightypes.Length; i++)
                {
                    // get the type of block?
                    if (hightypes[i] == 0xFF && highaddress[i] != 0 && highaddress[i] != 0xFFFF)
                    {
                        logger.Debug("Updating for serialNumber: " + highaddress[i].ToString("X8"));
                        savedatatobinary(highaddress[i] + 10, 16, serialBytes, m_fileName);
                    }
                }
            }
            return false;
        }

        internal bool UpdatePartNumberExtra(string partNumber)
        {
            if (fbc != null)
            {
                int pc = 0;
                byte[] partBytes = new byte[10];
                partNumber = partNumber.PadRight(10, ' ');
                for (pc = 0; pc < 10; pc++)
                {
                    partBytes[pc] = Convert.ToByte(partNumber[pc]);
                }
                for (int i = 0; i < lowtypes.Length; i++)
                {
                    // get the type of block?
                    if (lowtypes[i] == 0xFF && lowaddress[i] != 0 && lowaddress[i] != 0xFFFF)
                    {
                        logger.Debug("Updating for partNumber: " + lowaddress[i].ToString("X8"));
                        savedatatobinary(lowaddress[i], 10, partBytes, m_fileName);
                    }
                }
                for (int i = 0; i < hightypes.Length; i++)
                {
                    // get the type of block?
                    if (hightypes[i] == 0xFF && highaddress[i] != 0 && highaddress[i] != 0xFFFF)
                    {
                        logger.Debug("Updating for partNumber: " + highaddress[i].ToString("X8"));
                        savedatatobinary(highaddress[i], 10, partBytes, m_fileName);
                    }
                }
            }
            return false;
        }

        internal bool UpdateBlock(bool vin, bool secret, bool iface, bool description)
        {
            if ((vin || secret || iface || description) && fbc != null)
            {
                if (vin && (m_ChassisID.Length < 17)) m_ChassisID = m_ChassisID.PadRight(17, '0');
                if (secret && (m_SecretCode.Length < 4)) m_SecretCode = m_SecretCode.PadRight(4, '0');
                if (secret && (m_interfaceDevice.Length < 4)) m_interfaceDevice = m_interfaceDevice.PadRight(4, '0');
                if (secret && (m_ecuDescription.Length < 4)) m_ChassisID = m_ecuDescription.PadRight(4, '0');
                foreach (FlashBlock fb in fbc)
                {
                    if (vin) fb.SetVin(m_ChassisID);
                    if (secret) fb.SetSecret(m_SecretCode);
                    if (iface) fb.SetInterface(m_interfaceDevice);
                    if (description) fb.SetDescription(m_ecuDescription);
                    //DumpThisData(fb.BlockData, @"C:\T8Decode\flashblock_afterchange" + fb.BlockNumber.ToString() + "_" + fb.BlockAddress.ToString("X8") + ".blk");
                    fb.CodeBlock();
                    //DumpThisData(fb.BlockData, @"C:\T8Decode\flashblock_afterchangeandcode" + fb.BlockNumber.ToString() + "_" + fb.BlockAddress.ToString("X8") + ".blk");
                    // save the data back to the flashblock location
                    savedatatobinary(fb.BlockAddress, fb.BlockData.Length, fb.BlockData, m_fileName);
                }
                return true;
            }
            return false;
        }

        internal void ClearVIN()
        {
            m_ChassisID = "                 ";
            UpdateBlock(true, false, false, false);
        }

        internal byte[] ReadContainer(byte[] decodedpiArea, byte id)
        {
            if (decodedpiArea == null) return null;

            int pos = 0;
            while ((pos + 2) <= decodedpiArea.Length)
            {
                uint len = decodedpiArea[pos++];
                uint type = decodedpiArea[pos++];
                if (len == 0xF7 && type == 0xF7) break;

                if (type == id && len > 0)
                {
                    if (len + pos > decodedpiArea.Length)
                    {
                        logger.Debug("Pi container out of range: " + (len + pos).ToString("D") + " / " + decodedpiArea.Length.ToString("D"));
                        return null;
                    }

                    byte[] retContainer = new byte[len];
                    for (int i = 0; i < len; i++)
                    {
                        retContainer[i] = decodedpiArea[pos++];
                    }
                    return retContainer;
                }
                else
                {
                    pos += (int)len;
                }

            }
            return null;
        }

        /// <summary>
        /// This is only to be used for explicit saving where the container is to be created if it's not present
        /// </summary>
        /// <param name="decodedpiArea">Input buffer</param>
        /// <param name="id">Id of the target container</param>
        /// <param name="containerdata">Actual string that is to be stored</param>
        /// <param name="createLen">Explicit length if the container has to be created (Can be set to 0 if you want the string length to be the deciding factor)</param>
        /// <returns>null or output buffer since "PoINtErS ArE NoT COnDoNed iN c#"...</returns>
        internal static byte[] StoreStringContainer(byte[] decodedpiArea, byte id, string containerdata, int createLen)
        {
            bool found = false;
            int pos = 0;
            uint len = 0;
            uint type = 0;

            if (decodedpiArea == null ||
                createLen > 255 || createLen < 0 ||
                (containerdata != null && containerdata.Length > 255))
            {
                return null;
            }

            while ((pos + 2) <= decodedpiArea.Length)
            {
                len = decodedpiArea[pos++];
                type = decodedpiArea[pos++];
                if (len == 0xF7 && type == 0xF7)
                {
                    pos -= 2;
                    break;
                }

                if (type == id)
                {
                    found = true;
                    break;
                }
                else
                {
                    pos += (int)len;
                }
            }

            // Item must be created
            if (found == false)
            {
                int strLen = 0;
                int free = 0;
                int origPos = pos;
                while (pos < decodedpiArea.Length)
                {
                    if (decodedpiArea[pos] != 0xf7) break;
                    free++;
                    pos++;
                }

                // Not enough room for data + header
                if ((createLen + 2) > free ||
                    (createLen == 0 && containerdata != null && (containerdata.Length + 2) > free))
                {
                    return null;
                }

                if (createLen == 0 && containerdata != null)
                {
                    createLen = containerdata.Length;
                }

                decodedpiArea[origPos++] = (byte)createLen;
                decodedpiArea[origPos++] = id;

                // Strings are evil
                try
                {
                    if (containerdata != null)
                    {
                        strLen = containerdata.Length;
                        for (int i = 0; i < strLen && i < createLen; i++)
                        {
                            decodedpiArea[origPos++] = (byte)containerdata[i];
                        }
                    }

                    for (int i = strLen; i < createLen; i++)
                    {
                        decodedpiArea[origPos++] = (byte)' ';
                    }
                }
                catch (Exception E)
                {
                    return null;
                }

                return decodedpiArea;
            }
            else
            {
                // Something is clearly wrong with this header
                if ((pos + len) > decodedpiArea.Length)
                {
                    return null;
                }

                // Strings are evil
                int strLen = 0;
                try
                {
                    if (containerdata != null)
                    {
                        strLen = containerdata.Length;
                        for (int i = 0; i < strLen && i < len; i++)
                        {
                            decodedpiArea[pos++] = (byte)containerdata[i];
                        }
                    }
                    for (int i = strLen; i < len; i++)
                    {
                        decodedpiArea[pos++] = (byte)' ';
                    }
                }
                catch (Exception E)
                {
                    return null;
                }
                return decodedpiArea;
            }
        }

        // Very much junk code that is just enough to make it work
        internal void UpdatePiArea(byte id, string containerdata, int createLen)
        {
            if (m_fileName == "") return;
            int checksumAreaOffset = ChecksumT8.GetChecksumAreaOffset(m_fileName);
            int endOfPIArea = GetEmptySpaceStartFrom(m_fileName, checksumAreaOffset);
            byte[] piarea = readdatafromfile(m_fileName, checksumAreaOffset, endOfPIArea - checksumAreaOffset + 1);

            for (int t = 0; t < piarea.Length; t++)
            {
                piarea[t] += 0xD6;
                piarea[t] ^= 0x21;
            }

            piarea = StoreStringContainer(piarea, id, containerdata, createLen);
            if (piarea == null) return;

            for (int t = 0; t < piarea.Length; t++)
            {
                piarea[t] ^= 0x21;
                piarea[t] -= 0xD6;
            }

            savedatatobinary(checksumAreaOffset, piarea.Length, piarea, m_fileName);
        }

        internal void UpdateSoftwareVersion()
        {
            UpdatePiArea(0x08, m_SoftwareVersion, 30);
        }

        internal void UpdatePartNumber()
        {
            UpdatePiArea(0xC1, m_PartNumber, 8);
            UpdatePartNumberExtra(m_PartNumber);
        }

        internal void UpdateProgramerName()
        {
            UpdatePiArea(0x1D, m_programmerName, 17);
        }

        internal void UpdateProgramingDevice()
        {
            UpdatePiArea(0x10, m_programmerDevice, 10);
        }

        internal void UpdateReleaseDate()
        {
            UpdatePiArea(0x0A, m_releaseDate, 19);
        }

        internal void UpdateHardwareID()
        {
            UpdatePiArea(0x92, m_hardwareID, 9);
        }

        internal void UpdateSerialNumber()
        {
            UpdateSerialNumber(m_SerialNumber);
        }
    }
}
