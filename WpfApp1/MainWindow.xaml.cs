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

            LSB.Hide("message.txt", "key.txt", "1.wav", "2.wav");

            LSB.Extract("message1.txt","key.txt","2.wav");

            output.Text = PhaseCoding.PhaseCoding.Phase_Coding("message.txt", "key.txt", "1.wav", "2.wav").ToString();


        }

        private void output_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
