using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace CUEeditor
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        List<string> source_data = new List<string>();
        DataCUE dataCUE = new DataCUE();
        string file_path;

        private void Window_Drop(object sender, DragEventArgs e)
        {
            var paths = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
            if (paths.Length <= 0) { return; }
            source_data.Clear();
            string path = paths.GetValue(0).ToString();
            file_path = path;
            StreamReader reader = new StreamReader(path);
            string line = reader.ReadLine();
            string key_artist = "PERFORMER \"";
            string key_title = "TITLE \"";
            string key_track = "TRACK ";
            char[] lineSpliter = new char[] { '\"' };
            StringBuilder builder = new StringBuilder();
            dataCUE.Clear();
            int index = 0;
            while (line != null)
            {
                string title = FindCUELineKeyWord(line, lineSpliter, key_title);
                if (title != null)
                { dataCUE.title = new DataStringLine(title, index); }
                string artist = FindCUELineKeyWord(line, lineSpliter, key_artist);
                if (artist != null)
                { dataCUE.artist = new DataStringLine(artist, index); }

                if (line.IndexOf(key_track) != -1)
                {
                    source_data.Add(line);
                    line = reader.ReadLine();
                    index++;
                    break; 
                }
                source_data.Add(line);
                line = reader.ReadLine();
                index++;
            }
            DataTrack track = new DataTrack();
            while (line != null)
            {
                string title = FindCUELineKeyWord(line, lineSpliter, key_title);
                if (title != null)
                { 
                    track.title = new DataStringLine(title, index);
                    builder.AppendLine(title);
                }
                string artist = FindCUELineKeyWord(line, lineSpliter, key_artist);
                if (artist != null)
                { track.artist = new DataStringLine(artist, index); }

                if (line.IndexOf(key_track) != -1)
                {
                    dataCUE.tracks.Add(track);
                    track = new DataTrack();
                }
                source_data.Add(line);
                line = reader.ReadLine();
                index++;
            }
            dataCUE.tracks.Add(track);
            reader.Close();
            reader.Dispose();
            tbTitleInputer.Text = builder.ToString();
            SetupUI();
        }

        string FindCUELineKeyWord(string line, char[] spliter, string key_word)
        {
            int findIndex = line.IndexOf(key_word);
            if (findIndex != -1)
            {
                string[] line_split = line.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                if (line_split.Length > 1)
                {
                    string line_data = line_split[1];
                    return line_data;
                }
            }
            return null;
        }

        public void SetupUI()
        {
            tbAlbumTitle.Text = dataCUE.title.data;
            tbAlbumArtist.Text = dataCUE.artist.data;
            stackTitle.Children.Clear();
            stackArtist.Children.Clear();
            foreach (DataTrack track in dataCUE.tracks)
            {
                TextBox boxTitle = new TextBox() { Text = track.title.data };
                TextBox boxArtist = new TextBox() { Text = track.artist.data };
                stackTitle.Children.Add(boxTitle);
                stackArtist.Children.Add(boxArtist);
            }
        }

        private void SetAllArtist_Click(object sender, RoutedEventArgs e)
        {
            string artist = tbAlbumArtist.Text;
            foreach (TextBox tb in stackArtist.Children)
            {
                tb.Text = artist;
            }
        }

        private void SetInputer_Click(object sender, RoutedEventArgs e)
        {
            string[] lines = tbTitleInputer.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length && i < stackTitle.Children.Count; i++)
            {
                TextBox box = (TextBox)stackTitle.Children[i];
                box.Text = lines[i];
            }
        }

        private void ClearNum_Click(object sender, RoutedEventArgs e)
        {
            string[] lines = tbTitleInputer.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int index = line.IndexOf(tbNumberClearer.Text);
                if (index != -1)
                {
                    line = line.Substring(index + 1);
                    builder.AppendLine(line);
                }
            }
            tbTitleInputer.Text = builder.ToString();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            string key_artist = "PERFORMER \"";
            string key_title = "TITLE \"";
            string key_end = "\"";
            string key_space = "    ";
            if (dataCUE.title != null)
            {
                dataCUE.title.data = tbAlbumTitle.Text;
                source_data[dataCUE.title.line] = key_title + dataCUE.title.data + key_end;
            }
            if (dataCUE.artist != null)
            {
                dataCUE.artist.data = tbAlbumArtist.Text;
                source_data[dataCUE.artist.line] = key_artist + dataCUE.artist.data + key_end;
            }
            for (int i = 0; i < dataCUE.tracks.Count; i++)
            {
                DataTrack track = dataCUE.tracks[i];
                TextBox boxTitle = (TextBox)stackTitle.Children[i];
                TextBox boxArtist = (TextBox)stackArtist.Children[i];
                if (track.title != null)
                {
                    track.title.data = boxTitle.Text;
                    source_data[track.title.line] = key_space + key_title + track.title.data + key_end;
                }
                if (track.artist != null)
                {
                    track.artist.data = boxArtist.Text;
                    source_data[track.artist.line] = key_space + key_artist + track.artist.data + key_end;
                }
            }

            StreamWriter writer = new StreamWriter(file_path, false);
            foreach (string line in source_data)
            {
                writer.WriteLine(line);
            }
            writer.Close();
            writer.Dispose();
        }
    }

    public class DataStringLine
    {
        public string data;
        public int line;
        public DataStringLine(string setData, int setLine)
        {
            data = setData;
            line = setLine;
        }
    }
    public class DataTrack
    {
        public DataStringLine title;
        public DataStringLine artist;
    }
    public class DataCUE : DataTrack
    {
        public List<DataTrack> tracks;

        public DataCUE()
        {
            tracks = new List<DataTrack>();
        }
        public void Clear()
        {
            tracks.Clear();
            title = null;
            artist = null;
        }
    }
}
