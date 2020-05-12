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
using Encode;
using System.IO;
using NAudio.Wave;
using Microsoft.Win32;


namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string audiofile;
        string message;
        string currentdirectory = "C:\\";
        string selectedFileName;
        string outputdirectory;

        public MainWindow()
        {
            InitializeComponent();
            decode_text.Text = "Выберите метод кодирования, чтобы получить больше информации";
            encode_text.Text = "Выберите метод кодирования, чтобы получить больше информации";
        }

        bool ChooseFile(string filter)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = currentdirectory;
            //вставить выбор расширения файла 
            openFileDialog1.Filter = filter;
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == true)
            {
                selectedFileName = openFileDialog1.FileName;
                currentdirectory = openFileDialog1.FileName;
                return true;
            }
            return false;
        }
        void Encode_SetDefaultColors()
        {
            encode_audiofile.Background = new SolidColorBrush(Color.FromArgb(100, 0xFF, 0xFF, 0xFF));
            encode_message.Background = new SolidColorBrush(Color.FromArgb(100, 0xFF, 0xFF, 0xFF));
            encode_outputdirectorytext.Background = new SolidColorBrush(Color.FromArgb(100, 0xFF, 0xFF, 0xFF));
        }

        bool Encode_CheckValidate()
        {
            bool check = true;
            if (!File.Exists(encode_audiofile.Text))
            {
                encode_audiofile.Background = new SolidColorBrush(Color.FromArgb(100,0xE5, 0xBA, 0xBA));
                check = false;
            }
            if (!File.Exists(encode_message.Text))
            {
                encode_message.Background = new SolidColorBrush(Color.FromArgb(100,0xE5, 0xBA, 0xBA));
                check = false;
            }
            if (encode_outputdirectorytext.Text == "" || !Directory.Exists(System.IO.Path.GetDirectoryName(encode_outputdirectorytext.Text)))
            {
                encode_outputdirectorytext.Background = new SolidColorBrush(Color.FromArgb(100, 0xE5, 0xBA, 0xBA));
                check = false;
            }
            return check;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            //LSB.Hide("message.txt", "key.txt", "1.wav", "2.wav");

            //LSB.Extract("message1.txt","key.txt","2.wav");

            //ParityMethod.Hide("message.txt", "1.wav", "2.wav");

            //ParityMethod.Extract("message1.txt", "2.wav");

            //PhaseCoding.PhaseCoding.Phase_Coding("message.txt", "key.txt", "1.wav", "2.wav").ToString();

            Encode_SetDefaultColors();
            switch (encode_select.SelectedIndex)
            {
                case 0:
                    MessageBox.Show("Выберите тип кодирования");
                    break;
                case 1:
                    if (!Encode_CheckValidate())
                    {
                        MessageBox.Show("Введены неверные данные\nИсправьте выделенные пути файлов");
                        break;
                    }
                    LSB.Hide(encode_message.Text, password.Password, encode_audiofile.Text, encode_outputdirectorytext.Text);
                    break;
                case 2:
                    if (!Encode_CheckValidate())
                    {
                        MessageBox.Show("Введены неверные данные\nИсправьте выделенные пути файлов");
                        break;
                    }
                    ParityMethod.Hide(encode_message.Text, encode_audiofile.Text, encode_outputdirectorytext.Text);
                    break;
                case 3:
                    encode_text.Text = "Метод фазвого кодирования";
                    break;
            }

        }

        private void output_Loaded(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Когда выбираем другой метод кодирования.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            //comboBox.SelectedIndex
            if (encode_text == null)
            {
                return;
            }
            switch (comboBox.SelectedIndex)
            {
                case 0:
                    encode_text.Text = "Выберите метод кодирования, чтобы получить больше информации";
                    break;
                case 1:
                    encode_text.Text = "Метод LSB";
                    break;
                case 2:
                    encode_text.Text = "Метод четного кодирования";
                    break;
                case 3:
                    encode_text.Text = "Метод фазвого кодирования";
                    break;
            }
        }

        private void encode_selectaudiofile_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseFile("WAV audio file (*.wav)|;*.wav"))
            {
                audiofile = selectedFileName;
                encode_audiofile.Text = audiofile;
            }
            
        }

        private void encode_selectmessage_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseFile("TXT text file (*.txt)|;*.txt"))
            {
                message = selectedFileName;
                encode_message.Text = message;
            }
        }

        private void encode_outputdirectory_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.InitialDirectory = "C:\\";
            dialog.ValidateNames = false;
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;


            // Always default to Folder Selection.
            dialog.FileName = "encoded_"+"file"+ ".wav";

            if (dialog.ShowDialog() == true)
            {
                outputdirectory = dialog.FileName;
                encode_outputdirectorytext.Text = outputdirectory;
                currentdirectory = dialog.FileName;
            }
        }

        private void decode_select_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            //comboBox.SelectedIndex
            if (encode_text == null || decode_text == null)
            {
                return;
            }
            switch (comboBox.SelectedIndex)
            {
                case 0:
                    decode_text.Text = "Выберите метод кодирования, чтобы получить больше информации";
                    break;
                case 1:
                    decode_text.Text = "Метод LSB";
                    break;
                case 2:
                    decode_text.Text = "Метод четного кодирования";
                    break;
                case 3:
                    decode_text.Text = "Метод фазвого кодирования";
                    break;
            }
        }



        void Decode_SetDefaultColors()
        {
            decode_audiofile.Background = new SolidColorBrush(Color.FromArgb(100, 0xFF, 0xFF, 0xFF));
            decode_outputdirectorytext.Background = new SolidColorBrush(Color.FromArgb(100, 0xFF, 0xFF, 0xFF));
        }

        bool Decode_CheckValidate()
        {
            bool check = true;
            if (!File.Exists(decode_audiofile.Text))
            {
                encode_audiofile.Background = new SolidColorBrush(Color.FromArgb(100, 0xE5, 0xBA, 0xBA));
                check = false;
            }
            if (decode_outputdirectorytext.Text == ""  || !Directory.Exists(System.IO.Path.GetDirectoryName(decode_outputdirectorytext.Text)))
            {
                encode_outputdirectorytext.Background = new SolidColorBrush(Color.FromArgb(100, 0xE5, 0xBA, 0xBA));
                check = false;
            }
            return check;
        }







        private void decode_button_Click(object sender, RoutedEventArgs e)
        {
            //LSB.Hide("message.txt", "key.txt", "1.wav", "2.wav");

            //LSB.Extract("message1.txt","key.txt","2.wav");

            //ParityMethod.Hide("message.txt", "1.wav", "2.wav");

            //ParityMethod.Extract("message1.txt", "2.wav");

            //PhaseCoding.PhaseCoding.Phase_Coding("message.txt", "key.txt", "1.wav", "2.wav").ToString();

            Decode_SetDefaultColors();
            switch (decode_select.SelectedIndex)
            {
                case 0:
                    MessageBox.Show("Выберите тип кодирования");
                    break;
                case 1:
                    if (!Decode_CheckValidate())
                    {
                        MessageBox.Show("Введены неверные данные\nИсправьте выделенные пути файлов");
                        break;
                    }
                    LSB.Extract(decode_outputdirectorytext.Text, decode_password.Password, decode_audiofile.Text);
                    break;
                case 2:
                    if (!Decode_CheckValidate())
                    {
                        MessageBox.Show("Введены неверные данные\nИсправьте выделенные пути файлов");
                        break;
                    }
                    ParityMethod.Extract(decode_outputdirectorytext.Text, decode_audiofile.Text);
                    break;
                case 3:
                    encode_text.Text = "Метод фазвого кодирования";
                    break;
            }
        }

        private void decode_outputdirectorybutton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.InitialDirectory = "C:\\";
            dialog.ValidateNames = false;
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;


            // Always default to Folder Selection.
            dialog.FileName = "decoded_" + "message" + ".txt";

            if (dialog.ShowDialog() == true)
            {
                outputdirectory = dialog.FileName;
                decode_outputdirectorytext.Text = outputdirectory;
                currentdirectory = dialog.FileName;
            }
        }

        private void decode_selectaudiofile_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseFile("WAV audio file (*.wav)|;*.wav"))
            {
                audiofile = selectedFileName;
                decode_audiofile.Text = audiofile;
            }
        }
    }
}
