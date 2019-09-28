using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StringToolset
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly StringCodecViewModel _viewModel = new StringCodecViewModel();
        public MainWindow()
        {
            DataContext = _viewModel;
            InitializeComponent();

        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            await JsonTree.Load(File.ReadAllText(@"E:\GitHub\JsonViewer\Samples\Sample1.txt"));
        }

        protected async void MessageBox(string title, string message)
        {
            await this.ShowMessageAsync(title, message, settings: new MetroDialogSettings { });
        }



        private void SourceTextInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var input = (TextBox)sender;
            _viewModel.SourceText = input.Text;
        }

        private void EncodeTextInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var input = (TextBox)sender;
            _viewModel.EncodeText = input.Text;
        }

        private void Codec(CodecType codec)
        {
            switch (codec)
            {
                case CodecType.EncodeUrl:
                    _viewModel.EncodeText = Uri.EscapeUriString(_viewModel.SourceText);
                    break;
                case CodecType.DecodeUrl:
                    _viewModel.SourceText = Uri.UnescapeDataString(_viewModel.EncodeText);
                    break;
                case CodecType.EncodeUrlComponent:
                    _viewModel.EncodeText = HttpUtility.UrlEncode(_viewModel.SourceText);
                    break;
                case CodecType.DecodeUrlComponent:
                    _viewModel.SourceText = HttpUtility.UrlDecode(_viewModel.EncodeText);
                    break;
                case CodecType.EncodeHtml:
                    _viewModel.EncodeText = HttpUtility.HtmlEncode(_viewModel.SourceText);
                    break;
                case CodecType.DecodeHtml:
                    _viewModel.SourceText = HttpUtility.HtmlDecode(_viewModel.EncodeText);
                    break;
            }
        }

        private void UrlEncode_Click(object sender, RoutedEventArgs e)
        {
            Codec(CodecType.EncodeUrl);
        }
        private void UrlDecode_Click(object sender, RoutedEventArgs e)
        {
            Codec(CodecType.DecodeUrl);
        }

        private void EncodeUrlComponent_Click(object sender, RoutedEventArgs e)
        {
            Codec(CodecType.EncodeUrlComponent);
        }

        private void DecodeUrlComponent_Click(object sender, RoutedEventArgs e)
        {
            Codec(CodecType.DecodeUrlComponent);
        }

        private void DecodeHtml_Click(object sender, RoutedEventArgs e)
        {
            Codec(CodecType.DecodeHtml);
        }

        private void EncodeHtml_Click(object sender, RoutedEventArgs e)
        {
            Codec(CodecType.EncodeHtml);
        }
    }
}
