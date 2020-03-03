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
namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LSB lsb = new LSB();
            using (FileStream message = new FileStream($"C:\\Users\\Alex Novoselov\\Desktop\\message.txt",FileMode.Open))
            {
                using (FileStream key = new FileStream($"C:\\Users\\Alex Novoselov\\Desktop\\key.txt", FileMode.Open))
                {
                    using (FileStream source = new FileStream($"C:\\Users\\Alex Novoselov\\Desktop\\1.wav", FileMode.Open))
                    {
                        using (FileStream destination = new FileStream($"C:\\Users\\Alex Novoselov\\Desktop\\2.wav", FileMode.OpenOrCreate))
                        {
                            LSB.Hide(message, key, source, destination);
                            output.Text = "1";
                        }
                    }
                }
            }
        }

        private void output_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
