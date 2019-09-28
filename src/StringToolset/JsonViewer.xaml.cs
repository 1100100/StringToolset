using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StringToolset
{
    /// <summary>
    /// JsonViewer.xaml 的交互逻辑
    /// </summary>
    public partial class JsonViewer : UserControl
    {
        private readonly JsonTreeViewModel _viewModel = new JsonTreeViewModel();
        private const GeneratorStatus Generated = GeneratorStatus.ContainersGenerated;
        private DispatcherTimer _timer;
        public JsonViewer()
        {
            DataContext = _viewModel;
            InitializeComponent();
        }

        public async Task Load(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                _viewModel.JsonTree = null;
                return;
            }
            await Task.Run(() =>
            {
                try
                {
                    var tree = JObject.Parse(json);
                    _viewModel.JsonTree = tree;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
            JsonInputText.Focus();
        }


        private async void FormatJsonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(JsonInputText.Text))
                {
                    _viewModel.JsonTree = null;
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
                    Indentation = 6,
                    IndentChar = ' '
                };
                try
                {
                    s.Serialize(writer, jsonObject);
                    _viewModel.JsonRaw = sWriter.ToString();
                    await Load(JsonInputText.Text);
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

        private void ExpandAll(object sender, RoutedEventArgs e)
        {

            ToggleItems(true);

        }

        private void CollapseAll(object sender, RoutedEventArgs e)
        {
            ToggleItems(false);
        }

        private void ToggleItems(bool isExpanded)
        {
            if (JsonTreeView.Items.IsEmpty)
                return;

            var prevCursor = Cursor;
            Cursor = Cursors.Wait;
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, delegate
            {
                ToggleItems(JsonTreeView, JsonTreeView.Items, isExpanded);
                _timer.Stop();
                Cursor = prevCursor;
            }, Application.Current.Dispatcher ?? throw new InvalidOperationException());
            _timer.Start();
        }

        private void ToggleItems(ItemsControl parentContainer, ItemCollection items, bool isExpanded)
        {
            var itemGen = parentContainer.ItemContainerGenerator;
            if (itemGen.Status == Generated)
            {
                Recurse(items, isExpanded, itemGen);
            }
            else
            {
                itemGen.StatusChanged += delegate
                {
                    Recurse(items, isExpanded, itemGen);
                };
            }
        }

        private void Recurse(ItemCollection items, bool isExpanded, ItemContainerGenerator itemGen)
        {
            if (itemGen.Status != Generated)
                return;

            foreach (var item in items)
            {
                if (!(itemGen.ContainerFromItem(item) is TreeViewItem tvi)) continue;
                tvi.IsExpanded = isExpanded;
                ToggleItems(tvi, tvi.Items, isExpanded);
            }
        }

        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2)
                return;

            if (sender is TextBlock tb)
            {
                Clipboard.SetText(tb.Text.Trim('"'));
            }
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
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
            _viewModel.JsonTree = null;
            _viewModel.JsonRaw = "";
        }
    }
}
