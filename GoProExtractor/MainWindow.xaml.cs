using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows;

namespace GoProExtractor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        EntryPoint Ep { get; set; }
        Settings Stg { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Ep = new(this);
            Stg = new(this);
        }

        // FFMpeg インストールフォルダへのパス　選択ダイアログボックス表示ボタン押下
        private void Btn_SelectFFMpeg_Click(object sender, RoutedEventArgs e)
        {
            using CommonOpenFileDialog cofd = new()
            {
                Title = "フォルダを選択してください",
                RestoreDirectory = true,
                IsFolderPicker = true,
            };
            if (cofd.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }
            Tb_PathFFMpeg.Text = cofd.FileName;
        }
        // FFMpeg インストールフォルダへのパス　テキストボックスへのドラッグ＆ドロップ処理
        private void Tb_PathFFMpeg_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }
        private void Tb_PathFFMpeg_Drop(object sender, DragEventArgs e)
        {
            Tb_PathFFMpeg.Text = string.Empty;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null)
            {
                bool isDirectory = File.GetAttributes(files[0]).HasFlag(FileAttributes.Directory);
                Tb_PathFFMpeg.Text = isDirectory ? Path.GetFullPath(files[0]) : Path.GetDirectoryName(files[0]);
            }
        }

        // 出力フォルダへのパス　選択ダイアログボックス表示ボタン押下
        private void Btn_SelectOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            using CommonOpenFileDialog cofd = new()
            {
                Title = "フォルダを選択してください",
                RestoreDirectory = true,
                IsFolderPicker = true,
            };
            if (cofd.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }
            Tb_PathOutput.Text = cofd.FileName;
        }
        // ファイル出力フォルダへのパス　テキストボックスへのドラッグ＆ドロップ処理
        private void Tb_PathOutput_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }
        private void Tb_PathOutput_Drop(object sender, DragEventArgs e)
        {
            Tb_PathOutput.Text = string.Empty;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null)
            {
                Tb_PathOutput.Text = Path.GetDirectoryName(files[0]);
            }
        }

        // mp4ファイル選択
        private void Btn_Mp4Select_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new();
            dialog.Title = "ファイルを開く";
            dialog.Filter = "全てのファイル(*.*)|*.*";
            Tb_Mp4File.Text = (bool)dialog.ShowDialog() ? dialog.FileName : "キャンセルされました";
        }
        private void Tb_Mp4File_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }

        private void Tb_Mp4File_Drop(object sender, DragEventArgs e)
        {
            Tb_Mp4File.Text = string.Empty;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null)
            {
                Tb_Mp4File.Text = files[0];
            }
        }

        private void Btn_Extract_Click(object sender, RoutedEventArgs e)
        {
            bool OK_Flag = Stg.Renew();
            if (OK_Flag)
            {
                Ep.Extract(Stg);
            }
        }

        private void Btn_Probe_Click(object sender, RoutedEventArgs e)
        {
            bool OK_Flag = Stg.Renew();
            if (OK_Flag)
            {
                Ep.ProbeMp4(Stg);
            }
        }

        private void Chkbox_StreamChange_Click(object sender, RoutedEventArgs e)
        {
            Cmb_TargetStream.IsEnabled = !Cmb_TargetStream.IsEnabled;
        }

    }
}
