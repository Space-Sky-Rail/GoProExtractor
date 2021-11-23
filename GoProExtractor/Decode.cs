using System;
using System.Collections.Generic;
using static GoProExtractor.Definitions;

namespace GoProExtractor
{
    public class Decode
    {
        /// <summary>
        /// DEVCのブロック毎に分割し，byte配列のリストにする
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static List<byte[]> SplitToDEVC(byte[] rawData)
        {
            int pos = 0;
            List<byte[]> byteDevc = new();
            while (pos < rawData.Length)
            {
                if (rawData[pos] == 'D' && rawData[pos + 1] == 'E' && rawData[pos + 2] == 'V' && rawData[pos + 3] == 'C')
                {
                    pos += 4;
                    pos++;      // skip "type" in (type-size-repeat structure)
                    pos++;      // skip "structure size" in (type-size-repeat structure)
                    int nByte = BitConverter.ToUInt16(GetReversedByteArray(rawData, pos, 2), 0);
                    pos += 2;
                    byte[] devc = new byte[nByte];
                    Array.Copy(rawData, pos, devc, 0, nByte);
                    byteDevc.Add(devc);
                    pos += nByte;
                }
                else
                {
                    pos++;
                }
            }
            return byteDevc;
        }

        public static DevInfo GetDevInfo(byte[] data)
        {
            //var txt = System.Text.Encoding.ASCII.GetString(data, 0, 4);
            DevInfo devInfo = new();
            int pos = 0;                // 頭は"DEVC"+4byteなので，8から始める
            while (pos + 3 < data.Length)
            {
                var fourCC = System.Text.Encoding.ASCII.GetString(data, pos, 4);
                if (fourCC == "DVID")
                {
                    pos += 4;
                    devInfo.DeviceID = (int)BitConverter.ToUInt32(GetReversedByteArray(data, pos, 4));
                    pos += 4;
                }
                else if (fourCC == "DVNM")
                {
                    pos += 4;
                    pos++;  // skip "type" in (type-size-repeat structure)
                    int nChar = (int)(byte)data[pos];
                    pos++;
                    pos += 2;   // skip "repeat" in (type-size-repeat structure)
                    devInfo.DeviceName = System.Text.Encoding.ASCII.GetString(data, pos, nChar);
                    pos += nChar;
                    continue;
                }
                else
                {
                    pos++;
                    continue;
                }
            }
            return devInfo;
        }


        public static GpsInfo GetGpsInfo(byte[] d)
        {
            GpsInfo gps = new();
            string utcFormat = "yyyyMMddHHmmss.fff";
            double dt = 1.0 / 18.0; // GPS rate (sec) 18Hz
            int pos = 0;
            while (pos + 3 < d.Length)
            {
                var fourCC = System.Text.Encoding.ASCII.GetString(d, pos, 4);
                if (fourCC == "STNM")
                {
                    pos += 4;
                    while (pos + 10 < d.Length)
                    {
                        var txt = System.Text.Encoding.ASCII.GetString(d, pos, 10);
                        if (txt.Contains("GPS (Lat.,"))
                        {
                            pos += 44;
                            while (pos + 3 < d.Length)
                            {
                                fourCC = System.Text.Encoding.ASCII.GetString(d, pos, 4);
                                switch (fourCC)
                                {
                                    case "GPSF":
                                        pos += 4;   // proceed 4bytes (fourCC)
                                        pos += 4;   // skip (type-size-repeat structure)
                                        gps.Fix = BitConverter.ToInt32(GetReversedByteArray(d, pos, 4));
                                        break;
                                    case "GPSU":
                                        pos += 4;   // proceed 4bytes (fourCC)
                                        pos += 4;   // skip (type-size-repeat structure)
                                        string utcs = "20" + System.Text.Encoding.ASCII.GetString(d, pos, 16);
                                        //gps.Utc = System.Text.Encoding.ASCII.GetString(d, pos, 16);
                                        //string utcs = "20" + gps.Utc;
                                        gps.Utc = DateTime.ParseExact(utcs, utcFormat, null);
                                        pos += 16;
                                        break;
                                    case "GPSA":
                                        pos += 4;   // proceed 4bytes (fourCC)
                                        pos += 4;   // skip (type-size-repeat structure)
                                        gps.AltSys = System.Text.Encoding.ASCII.GetString(d, pos, 4);
                                        pos += 4;
                                        break;
                                    case "GPS5":
                                        pos += 4;   // proceed 4bytes (fourCC)
                                        pos += 2;   // skip type and structure size 2bytes (type-size-repeat structure)
                                        int nEpoch = (int)BitConverter.ToUInt16(GetReversedByteArray(d, pos, 2));
                                        pos += 2;
                                        for (int i = 0; i < nEpoch; i++)
                                        {
                                            double dt_total = dt * i;
                                            TimeSpan timeSpan = new(0, 0, 0, 0, (int)(dt_total * 1000));
                                            DateTime utctime = gps.Utc + timeSpan;
                                            PosVel pv = new();
                                            pv.Lat = (int)BitConverter.ToUInt32(GetReversedByteArray(d, pos, 4)) / gps.Scal.Lat; pos += 4;
                                            pv.Lon = (int)BitConverter.ToUInt32(GetReversedByteArray(d, pos, 4)) / gps.Scal.Lon; pos += 4;
                                            pv.Alt = (int)BitConverter.ToUInt32(GetReversedByteArray(d, pos, 4)) / gps.Scal.Alt; pos += 4;
                                            pv.Spd2D = (int)BitConverter.ToUInt32(GetReversedByteArray(d, pos, 4)) / gps.Scal.Spd2D; pos += 4;
                                            pv.Spd3D = (int)BitConverter.ToUInt32(GetReversedByteArray(d, pos, 4)) / gps.Scal.Spd3D; pos += 4;
                                            pv.UtcOfSample = utctime;
                                            gps.Pos_list.Add(pv);
                                        }
                                        break;
                                    case "GPSP":
                                        pos += 4;   // proceed 4bytes (fourCC)
                                        pos += 4;   // skip (type-size-repeat structure)
                                        gps.Dop = ((int)BitConverter.ToUInt16(GetReversedByteArray(d, pos, 2))) / 100.0;
                                        pos += 4;
                                        break;
                                    case "UNIT":
                                        pos += 4;
                                        pos += 4;
                                        gps.Unit.Lat = System.Text.Encoding.ASCII.GetString(d, pos, 3); pos += 3;
                                        gps.Unit.Lon = System.Text.Encoding.ASCII.GetString(d, pos, 3); pos += 3;
                                        gps.Unit.Alt = System.Text.Encoding.ASCII.GetString(d, pos, 3); pos += 3;
                                        gps.Unit.Spd2D = System.Text.Encoding.ASCII.GetString(d, pos, 3); pos += 3;
                                        gps.Unit.Spd3D = System.Text.Encoding.ASCII.GetString(d, pos, 3); pos += 4;
                                        break;
                                    case "SCAL":
                                        pos += 4;
                                        pos += 4;
                                        gps.Scal.Lat = BitConverter.ToInt32(GetReversedByteArray(d, pos, 4)); pos += 4;
                                        gps.Scal.Lon = BitConverter.ToInt32(GetReversedByteArray(d, pos, 4)); pos += 4;
                                        gps.Scal.Alt = BitConverter.ToInt32(GetReversedByteArray(d, pos, 4)); pos += 4; ;
                                        gps.Scal.Spd2D = BitConverter.ToInt32(GetReversedByteArray(d, pos, 4)); pos += 4; ;
                                        gps.Scal.Spd3D = BitConverter.ToInt32(GetReversedByteArray(d, pos, 4)); pos += 4; ;
                                        break;
                                    case "STNM":
                                        pos += 1000000;
                                        break;
                                    default:
                                        pos++;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            pos++;
                        }
                    }
                }
                else
                {
                    pos++;
                }
            }
            return gps;
        }

        public static byte[] GetReversedByteArray(byte[] data, int pos, int n)
        {
            List<byte> rev = new List<byte> { };
            for (int i = pos + n - 1; i >= pos; i--)
            {
                rev.Add(data[i]);
            }
            return rev.ToArray();
        }

    }
}
