﻿/* The MIT License (MIT)

Copyright(c) 2016 Stanislav Zhelnio

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MPSSELight
{
    public class SpiDevice
    {
        private MpsseDevice mpsse;

        public enum SpiMode
        {
            Mode0,  //CPOL=0, CPHA=0
            Mode2   //CPOL=1, CPHA=0
        }

        public class SpiParams
        {
            public SpiMode Mode = SpiMode.Mode0;
            public FtdiPin ChipSelect = FtdiPin.None;
        }

        SpiParams param;

        private delegate void WriteCommandDelegate(byte[] data);
        private delegate byte[] ReadWriteCommandDelegate(byte[] data);
        WriteCommandDelegate writeCommand;
        ReadWriteCommandDelegate readWriteCommand;

        public SpiDevice(MpsseDevice mpsse) : this(mpsse, new SpiParams()) { }

        public SpiDevice(MpsseDevice mpsse, SpiParams param)
        {
            this.mpsse = mpsse;
            this.param = param;

            switch (param.Mode)
            {
                default:
                case SpiMode.Mode0:
                    writeCommand = mpsse.BytesOutOnMinusEdgeWithMsbFirst;
                    readWriteCommand = mpsse.BytesInOnPlusOutOnMinusWithMsbFirst;
                    break;
                case SpiMode.Mode2:
                    writeCommand = mpsse.BytesOutOnPlusEdgeWithMsbFirst;
                    readWriteCommand = mpsse.BytesInOnMinusOutOnPlusWithMsbFirst;
                    break;
            }

            //pin init values
            CS = Bit.One;

            Debug.WriteLine("SPI initial successful : " + mpsse.ClockFrequency);
        }

        public void write(byte[] data)
        {
            writeCommand(data);
        }

        public byte[] readWrite(byte[] data)
        {
            return readWriteCommand(data);
        }

        private Bit cs;
        public Bit CS
        {
            get { return cs; }
            set
            {
                cs = value;
                FtdiPin pinValue = (cs == Bit.One) ? param.ChipSelect : FtdiPin.None;
                mpsse.SetDataBitsLowByte(pinValue, FtdiPin.CS | FtdiPin.DO | FtdiPin.SK);
            }
        }

        public bool LoopbackEnabled
        {
            get { return mpsse.Loopback; }
            set { mpsse.Loopback = value; }
        }
    }
}