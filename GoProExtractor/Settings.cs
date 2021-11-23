using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using static GoProExtractor.Definitions;

namespace GoProExtractor
{
    public class Settings
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


        private MainWindow MainWindow { get; set; }

        public Settings(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            LoadStg();
        }

        public bool Renew()
        {
            bool OkFlag = true;
            List<string> errMsg = new();

            // ffmpegインストールフォルダが指定されているかチェック
            if (string.IsNullOrEmpty(MainWindow.Tb_PathFFMpeg.Text))
            {
                errMsg.Add("FFMpeg folder is not specified.");
                OkFlag = false;
            }
            else
            {
                // ffprobeのチェック
                string t_FFProbePath = MainWindow.Tb_PathFFMpeg.Text + @"\ffprobe.exe";
                if (!File.Exists(t_FFProbePath))
                {
                    errMsg.Add(t_FFProbePath + " not exists.");
                    OkFlag = false;
                }
                else
                {
                    FFProbePath = t_FFProbePath;
                }
                // ffmpegのチェック
                string t_FFMpegPath = MainWindow.Tb_PathFFMpeg.Text + @"\ffmpeg.exe";
                if (!File.Exists(t_FFMpegPath))
                {
                    errMsg.Add(t_FFMpegPath + " not exists.");
                    OkFlag = false;
                }
                else
                {
                    FFMpegPath = t_FFMpegPath;
                }
            }

            // mp4ファイルが指定されているかチェック
            if (string.IsNullOrEmpty(MainWindow.Tb_Mp4File.Text))
            {
                errMsg.Add("mp4 file is not specified.");
                OkFlag = false;
            }
            else
            {
                // mp4のチェック
                string t_Mp4Path = MainWindow.Tb_Mp4File.Text;
                if (!File.Exists(t_Mp4Path))
                {
                    errMsg.Add(t_Mp4Path + " not exists.");
                    OkFlag = false;
                }
                else
                {
                    Mp4Path = t_Mp4Path;
                }
            }

            // outputフォルダが指定されているかチェック
            if (string.IsNullOrEmpty(MainWindow.Tb_PathOutput.Text))
            {
                // 指定されていない場合，mp4ファイルが指定されているならmp4と同じフォルダを設定する
                if (string.IsNullOrEmpty(MainWindow.Tb_Mp4File.Text))
                {
                    errMsg.Add("Output folder is not specified.");
                    OkFlag = false;
                }
                else
                {
                    string t_OutputPath = Path.GetDirectoryName(MainWindow.Tb_Mp4File.Text);
                    if (!Directory.Exists(t_OutputPath))
                    {
                        errMsg.Add("Output folder is not specified.");
                        OkFlag = false;
                    }
                    else
                    {
                        OutputPath = t_OutputPath;
                        MainWindow.Tb_PathOutput.Text = OutputPath;
                    }
                }
            }
            else
            {
                // 指定されている場合，存在をチェックして設定する
                string t_OutputPath = MainWindow.Tb_PathOutput.Text;
                if (!Directory.Exists(t_OutputPath))
                {
                    errMsg.Add("Directory [" + t_OutputPath + "] not exists.");
                    OkFlag = false;
                }
                else
                {
                    OutputPath = t_OutputPath;
                }
            }

            // 出力ファイル選択チェックボックスを確認
            OutputGpx = (bool)MainWindow.Chkbox_GPX.IsChecked;
            OutputKml = (bool)MainWindow.Chkbox_KML.IsChecked;

            // ターゲットのStreamを確認して設定
            string cmb_target = MainWindow.Cmb_TargetStream.SelectedValue.ToString();
            switch (cmb_target)
            {
                case "Stream #0:1":
                    TargetStream = "0:1";
                    break;
                case "Stream #0:2":
                    TargetStream = "0:2";
                    break;
                case "Stream #0:3":
                    TargetStream = "0:3";
                    break;
                default:
                    TargetStream = "0:3";
                    break;
            }



            // GPS出力間隔を確認
            string cmb_gpsrate = MainWindow.Cmb_GpsRate.SelectedValue.ToString();
            switch (cmb_gpsrate)
            {
                case "All":
                    GpsInterval = 0;
                    break;
                case "1 sec":
                    GpsInterval = 1;
                    break;
                case "5 sec":
                    GpsInterval = 5;
                    break;
                case "10 sec":
                    GpsInterval = 10;
                    break;
                default:
                    GpsInterval = 1;
                    break;
            }

            // GPS fix状態による出力を確認
            string cmb_fixflag = MainWindow.Cmb_GpsFixFlag.SelectedValue.ToString();
            switch (cmb_fixflag)
            {
                case "All":
                    GpsFixFlag = 1;
                    break;
                case "2D/3D Fix":
                    GpsFixFlag = 2;
                    break;
                case "3D Fix only":
                    GpsFixFlag = 3;
                    break;
                default:
                    GpsFixFlag = 3;
                    break;
            }

            // GPS DOP値の下限を設定
            string cmb_mindop = MainWindow.Cmb_GpsDop.SelectedValue.ToString();
            switch (cmb_mindop)
            {
                case "Don't care":
                    GpsDopThreshold = 9999;
                    break;
                case "5.0 (fair)":
                    GpsDopThreshold = 5;
                    break;
                case "3.0 (good)":
                    GpsDopThreshold = 3;
                    break;
                case "2.0 (very good)":
                    GpsDopThreshold = 2;
                    break;
                default:
                    GpsDopThreshold = 5;
                    break;
            }

            if (errMsg.Count > 0)
            {
                MainWindow.Tb_Msg.Text = errMsg.Aggregate((x, y) => x + "\n" + y);
            }
            if (OkFlag) SaveStg();
            return OkFlag;
        }


        public string GetGpxFileName()
        {
            return OutputPath + @"\" + Path.GetFileName(Path.ChangeExtension(Mp4Path, ".gpx"));
        }

        public string GetKmlFileName()
        {
            return OutputPath + @"\" + Path.GetFileName(Path.ChangeExtension(Mp4Path, ".kml"));
        }


        public void ResetAfterLoading()
        {
            MainWindow.Tb_Mp4File.Text = Mp4Path;
            MainWindow.Tb_PathFFMpeg.Text = Path.GetDirectoryName(FFMpegPath);
            MainWindow.Tb_PathOutput.Text = OutputPath;
            MainWindow.Chkbox_GPX.IsChecked = OutputGpx;
            MainWindow.Chkbox_KML.IsChecked = OutputKml;
            switch (TargetStream)
            {
                case "0:1":
                    MainWindow.Cmb_TargetStream.SelectedIndex = 0;
                    break;
                case "0:2":
                    MainWindow.Cmb_TargetStream.SelectedIndex = 1;
                    break;
                case "0:3":
                    MainWindow.Cmb_TargetStream.SelectedIndex = 2;
                    break;
                default:
                    MainWindow.Cmb_TargetStream.SelectedIndex = 2;
                    break;
            }

            switch (GpsInterval)
            {
                case 0:
                    MainWindow.Cmb_GpsRate.SelectedIndex = 0;
                    break;
                case 1:
                    MainWindow.Cmb_GpsRate.SelectedIndex = 1;
                    break;
                case 5:
                    MainWindow.Cmb_GpsRate.SelectedIndex = 2;
                    break;
                case 10:
                    MainWindow.Cmb_GpsRate.SelectedIndex = 3;
                    break;
                default:
                    MainWindow.Cmb_GpsRate.SelectedIndex = 1;
                    break;

            }

            switch (GpsFixFlag)
            {
                case 1:
                    MainWindow.Cmb_GpsFixFlag.SelectedIndex = 0;
                    break;
                case 2:
                    MainWindow.Cmb_GpsFixFlag.SelectedIndex = 1;
                    break;
                case 3:
                    MainWindow.Cmb_GpsFixFlag.SelectedIndex = 2;
                    break;
                default:
                    MainWindow.Cmb_GpsFixFlag.SelectedIndex = 2;
                    break;
            }

            switch (GpsDopThreshold)
            {
                case 9999:
                    MainWindow.Cmb_GpsDop.SelectedIndex = 0;
                    break;
                case 5:
                    MainWindow.Cmb_GpsDop.SelectedIndex = 1;
                    break;
                case 3:
                    MainWindow.Cmb_GpsDop.SelectedIndex = 2;
                    break;
                case 2:
                    MainWindow.Cmb_GpsDop.SelectedIndex = 3;
                    break;
                default:
                    MainWindow.Cmb_GpsDop.SelectedIndex = 1;
                    break;
            }
            Renew();
        }


        private void SaveStg()
        {
            JsonSerializerOptions opt = new JsonSerializerOptions();
            opt.WriteIndented = true;
            opt.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            string json = JsonSerializer.Serialize(this, opt);
            File.WriteAllText("settings.json", json);
        }

        private void LoadStg()
        {
            if (File.Exists("settings.json"))
            {
                var opt = new JsonSerializerOptions();
                //opt.Converters.Add(new JsonStringEnumConverter());
                opt.ReadCommentHandling = JsonCommentHandling.Skip;
                string loaded_txt = File.ReadAllText("settings.json");
                SettingProperties stg = JsonSerializer.Deserialize<SettingProperties>(loaded_txt, opt);

                Mp4Path = stg.Mp4Path;
                FFProbePath = stg.FFProbePath;
                FFMpegPath = stg.FFMpegPath;
                OutputPath = stg.OutputPath;
                OutputGpx = stg.OutputGpx;
                OutputKml = stg.OutputKml;
                OutputAcc = stg.OutputAcc;                  // for future work
                OutputGyro = stg.OutputGyro;                // for future work
                TargetStream = stg.TargetStream;
                GpsInterval = stg.GpsInterval;              // interval in second
                GpsFixFlag = stg.GpsFixFlag;                // 1:output all record, 2:output 2D/3D, 3:output 3D fix only
                GpsDopThreshold = stg.GpsDopThreshold;      // minimum DOP value to be output
                ResetAfterLoading();
            }
        }

    }

}

