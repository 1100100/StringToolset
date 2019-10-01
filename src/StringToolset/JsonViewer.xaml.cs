using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace StringToolset
{
    /// <summary>
    /// JsonViewer.xaml 的交互逻辑
    /// </summary>
    public partial class JsonViewer : UserControl
    {
        private readonly FoldingManager _foldingManager;
        private readonly BraceFoldingStrategy _strategy;


        public JsonViewer()
        {
            InitializeComponent();
            _foldingManager = FoldingManager.Install(JsonInputText.TextArea);
            _strategy = new BraceFoldingStrategy();
            using var xmlReader = new System.Xml.XmlTextReader("SyntaxHighlighting\\JSON.xml");
            JsonInputText.SyntaxHighlighting = HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);
            SearchPanel.Install(JsonInputText);
        }

        private string JsonRaw
        {
            set => JsonInputText.Text = value;
        }


        private async void FormatJsonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(JsonInputText.Text))
                {
                    return;
                }
                var s = new JsonSerializer();
                JsonReader reader = new JsonTextReader(new StringReader(JsonInputText.Text));
                var jsonObject = s.Deserialize(reader);
                if (jsonObject == null) return;

                await using var sWriter = new StringWriter();
                var writer = new JsonTextWriter(sWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                try
                {
                    s.Serialize(writer, jsonObject);
                    SetEditorValue(sWriter.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    await writer.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetEditorValue(string value)
        {
            JsonRaw = value;
            _strategy.UpdateFoldings(_foldingManager, JsonInputText.Document);
        }


        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            var toolBar = sender as ToolBar;
            if (toolBar != null && toolBar.Template.FindName("OverflowGrid", toolBar) is FrameworkElement overflowGrid)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }

            if (toolBar != null && toolBar.Template.FindName("MainPanelBorder", toolBar) is FrameworkElement mainPanelBorder)
            {
                mainPanelBorder.Margin = new Thickness();
            }
        }

        private void CleanClick(object sender, RoutedEventArgs e)
        {
            SetEditorValue("");
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = "Json|*.json" };
            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, JsonInputText.Text);
            }
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "Json|*.json" };
            if (dialog.ShowDialog() == true)
            {
                SetEditorValue(File.ReadAllText(dialog.FileName));
            }
        }


        private void JsonInputText_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void JsonInputText_PreviewDrop(object sender, DragEventArgs e)
        {
            var fileName = ((Array)e.Data.GetData(DataFormats.FileDrop))?.GetValue(0).ToString();
            if (!string.IsNullOrWhiteSpace(fileName) && File.Exists(fileName))
            {
                SetEditorValue(File.ReadAllText(fileName));
            }
        }

        private void JsonInputText_TextChanged(object sender, EventArgs e)
        {
            _strategy.UpdateFoldings(_foldingManager, JsonInputText.Document);
        }
    }
}
