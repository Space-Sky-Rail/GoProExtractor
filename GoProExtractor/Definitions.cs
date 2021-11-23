using System;
using System.Collections.Generic;

namespace GoProExtractor
{
    public class Definitions
    {
        public class DevInfo
        {
            public int DeviceID { get; set; }       // Auto generated unique-ID for managing a large number of connect devices
            public string DeviceName { get; set; }  // Display name of the device
        }

        public class GpsInfo
        {
            public int Fix { get; set; }
            public DateTime Utc { get; set; }
            public string AltSys { get; set; }
            public Unit Unit { get; set; } = new();
            public Scal Scal { get; set; } = new();
            public List<PosVel> Pos_list { get; set; } = new();
            public double Dop { get; set; } = 9999;
        }

        public class Unit
        {
            public string Lat { get; set; }
            public string Lon { get; set; }
            public string Alt { get; set; }
            public string Spd2D { get; set; }
            public string Spd3D { get; set; }
        }

        public class Scal : PosVel { }

        public class PosVel
        {
            public double Lat { get; set; }
            public double Lon { get; set; }
            public double Alt { get; set; }
            public double Spd2D { get; set; }
            public double Spd3D { get; set; }
            public DateTime UtcOfSample { get; set; }
        }

        public class SettingProperties
        {
            public string Mp4Path { get; set; }
            public string FFProbePath { get; set; }
            public string FFMpegPath { get; set; }
            public string OutputPath { get; set; }
            public bool OutputGpx { get; set; }
            public bool OutputKml { get; set; }
            public bool OutputAcc { get; set; }             // for future work
            public bool OutputGyro { get; set; }            // for future work
            public string TargetStream { get; set; }
            public int GpsInterval { get; set; }            // interval in second
            public int GpsFixFlag { get; set; }             // 1:output all record, 2:output 2D/3D, 3:output 3D fix only
            public int GpsDopThreshold { get; set; }        // minimum DOP value to be output
        }
    }
}
