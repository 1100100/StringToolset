using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
        private readonly JsonViewerModel _viewerModel = new JsonViewerModel();
        public JsonViewer()
        {
            DataContext = _viewerModel;
            InitializeComponent();
            _foldingManager = FoldingManager.Install(JsonInputText.TextArea);
            _strategy = new BraceFoldingStrategy();
            using var xmlReader = new System.Xml.XmlTextReader("SyntaxHighlighting\\JSON.xml");
            JsonInputText.SyntaxHighlighting = HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);
            SearchPanel.Install(JsonInputText);
        }

        private string JsonRaw
        {
            get => JsonInputText.Text;
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
                    var metroWindow = Application.Current.MainWindow as MetroWindow;
                    await metroWindow.ShowMessageAsync("错误", ex.Message);
                }
                finally
                {
                    await writer.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                var metroWindow = Application.Current.MainWindow as MetroWindow;
                await metroWindow.ShowMessageAsync("错误", ex.Message);
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
            SaveDocument();
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

        private void SwitchReplace_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _viewerModel.ShowReplace = _viewerModel.ShowReplace == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            _viewerModel.IsShowReplace = !_viewerModel.IsShowReplace;
        }

        private async void Replace_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(_viewerModel.SearchText))
                return;
            try
            {
                JsonInputText.Document.Replace(JsonInputText.SelectionStart, JsonInputText.SelectionLength, _viewerModel.ReplaceText);
            }
            catch (Exception ex)
            {
                var metroWindow = Application.Current.MainWindow as MetroWindow;
                await metroWindow.ShowMessageAsync("错误", ex.Message);
            }
        }

        private void Replace_TextChanged(object sender, TextChangedEventArgs e)
        {
            _viewerModel.ReplaceText = ((TextBox)sender).Text;
        }

        private async void ReplaceAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(_viewerModel.SearchText))
                return;
            var metroWindow = Application.Current.MainWindow as MetroWindow;
            var dialogResult = await metroWindow.ShowMessageAsync("警告", $"确定要替换所有的字符{_viewerModel.SearchText}吗？", MessageDialogStyle.AffirmativeAndNegative);
            if (dialogResult != MessageDialogResult.Affirmative) return;
            var regex = GetRegEx(_viewerModel.SearchText);
            var offset = 0;
            JsonInputText.BeginChange();
            foreach (Match match in regex.Matches(JsonRaw))
            {
                JsonInputText.Document.Replace(offset + match.Index, match.Length, _viewerModel.ReplaceText);
                offset += _viewerModel.ReplaceText.Length - match.Length;
            }
            JsonInputText.EndChange();
        }
        private Regex GetRegEx(string oldText)
        {
            var options = RegexOptions.None;

            if (_viewerModel.MatchCase == false)
                options |= RegexOptions.IgnoreCase;

            if (_viewerModel.UserRegex)
                return new Regex(oldText, options);

            var pattern = Regex.Escape(oldText);
            if (_viewerModel.WholeWord)
                pattern = "\\b" + pattern + "\\b";
            return new Regex(pattern, options);
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            _viewerModel.SearchText = ((TextBox)sender).Text;
        }


        private void MatchCaseChecked(object sender, RoutedEventArgs e)
        {
            _viewerModel.MatchCase = true;
        }

        private void MatchCaseUnchecked(object sender, RoutedEventArgs e)
        {
            _viewerModel.MatchCase = false;
        }

        private void WholeWordsChecked(object sender, RoutedEventArgs e)
        {
            _viewerModel.WholeWord = true;
        }

        private void WholeWordsUnchecked(object sender, RoutedEventArgs e)
        {
            _viewerModel.WholeWord = false;
        }

        private void RegexUnchecked(object sender, RoutedEventArgs e)
        {
            _viewerModel.UserRegex = false;
        }

        private void UseRegexChecked(object sender, RoutedEventArgs e)
        {
            _viewerModel.UserRegex = true;
        }

        private void SavePressKey(object sender, CanExecuteRoutedEventArgs e)
        {
            SaveDocument();
        }

        private void SaveDocument()
        {
            var dialog = new SaveFileDialog { Filter = "Json|*.json" };
            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, JsonInputText.Text);
            }
        }
    }
}
