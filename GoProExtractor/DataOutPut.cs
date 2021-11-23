using System;
using System.Collections.Generic;
using static GoProExtractor.Definitions;

namespace GoProExtractor
{
    public class DataOutPut
    {
        public static int GetGPX(List<string> gpx, List<GpsInfo> gps, int gpsInterval, int gpsFixFlag, double gpsDopThreshold)
        {
            gpx.Add("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");

            List<string> gpx_attr = new();
            gpx_attr.Add("version=\"1.1\" ");
            gpx_attr.Add("creator=\"SkyRail - http://skyrail.tech/\" ");
            gpx_attr.Add("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" ");
            gpx_attr.Add("xmlns=\"http://www.topografix.com/GPX/1/0\" ");

            gpx.Add("<gpx " + string.Join(" ", gpx_attr) + " >");
            gpx.Add("  <metadata>");
            gpx.Add($"    <time>{gps[0].Utc.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ")}</time>");
            gpx.Add("  </metadata>");

            gpx.Add("  <trk>");
            gpx.Add("    <name>Track-01</name>");
            gpx.Add("    <trkseg>");

            TimeSpan timeSpan;
            DateTime refTime = gps[0].Pos_list[0].UtcOfSample;
            int count = 0;

            foreach (var g in gps)
            {
                if (g.Fix < gpsFixFlag || g.Dop > gpsDopThreshold) continue;
                foreach (var pvt in g.Pos_list)
                {
                    timeSpan = pvt.UtcOfSample - refTime;
                    if (timeSpan.TotalSeconds < gpsInterval) continue;
                    refTime = pvt.UtcOfSample;
                    count++;
                    gpx.Add($"      <trkpt lat=\"{pvt.Lat}\" lon=\"{pvt.Lon}\">");
                    gpx.Add($"        <ele>{pvt.Alt}</ele>");
                    gpx.Add($"        <time>{pvt.UtcOfSample.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ")}</time>");
                    gpx.Add($"        <extensions>");
                    gpx.Add($"          <gpxtpx:TrackPointExtension>");
                    gpx.Add($"            <gpxtpx:speed>{pvt.Spd2D}</gpxtpx:speed>");
                    gpx.Add($"            <gpxtpx:dop>{g.Dop}</gpxtpx:dop>");
                    gpx.Add($"          </gpxtpx:TrackPointExtension>");
                    gpx.Add($"        </extensions>");
                    gpx.Add("      </trkpt>");
                }
            }
            gpx.Add("    </trkseg>");
            gpx.Add("  </trk>");
            gpx.Add("</gpx>");
            return count;
        }

        public static int GetKML(List<string> kml, List<GpsInfo> gps, int gpsInterval, int gpsFixFlag, double gpsDopThreshold)
        {
            kml.Add("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            kml.Add("<kml xmlns=\"http://www.opengis.net/kml/2.2\">");
            kml.Add("<Document>");
            kml.Add("<name>Demo</name>");
            kml.Add("<description>Description Demo</description>");
            kml.Add("<Placemark>");
            kml.Add("  <name>Track Title</name>");
            kml.Add("  <description>Track Description</description>");
            kml.Add("  <Style>");
            kml.Add("    <LineStyle>");
            kml.Add("      <color>FF1400BE</color>");
            kml.Add("      <width>4</width>");
            kml.Add("    </LineStyle>");
            kml.Add("  </Style>");
            kml.Add("  <LineString>");
            kml.Add("    <extrude>1</extrude>");
            kml.Add("    <tessellate>1</tessellate>");
            kml.Add("    <altitudeMode>clampToGround</altitudeMode>");
            kml.Add("    <coordinates>");

            TimeSpan timeSpan;
            DateTime refTime = gps[0].Pos_list[0].UtcOfSample;
            int count = 0;

            foreach (var g in gps)
            {
                if (g.Fix < gpsFixFlag || g.Dop > gpsDopThreshold) continue;
                foreach (var pvt in g.Pos_list)
                {
                    timeSpan = pvt.UtcOfSample - refTime;
                    if (timeSpan.TotalSeconds < gpsInterval) continue;
                    refTime = pvt.UtcOfSample;
                    count++;

                    kml.Add($"{pvt.Lon},{pvt.Lat},{pvt.Alt}");
                    break;
                }
            }
            kml.Add("    </coordinates>");
            kml.Add("  </LineString>");
            kml.Add("</Placemark>");
            kml.Add("</Document>");
            kml.Add("</kml>");
            return count;
        }
    }
}
