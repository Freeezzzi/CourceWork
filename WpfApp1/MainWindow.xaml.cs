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
            using (FileStream message = new FileStream($"message.txt",FileMode.Open))
            {
                using (FileStream key = new FileStream($"key.txt", FileMode.Open))
                {
                    using (FileStream source = new FileStream($"1.wav", FileMode.Open))
                    {
                        using (FileStream destination = new FileStream($"2.wav", FileMode.Create))
                        {
                            LSB.Hide(message, key, source, destination);
                            
                        }
                        using (FileStream destination = new FileStream($"2.wav", FileMode.Open))
                        {
                            using (FileStream message1 = new FileStream($"message1.txt",FileMode.Create))
                                lsb.Extract(message1, key, destination).ForEach((t) => MessageBox.Show(t.ToString()));
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
