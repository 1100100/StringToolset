using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Search;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace StringToolset
{
    public class JsonViewerModel : ViewModelBase
    {

        #region
        private Visibility _showReplace = Visibility.Hidden;

        public Visibility ShowReplace
        {
            get => _showReplace;
            set
            {
                _showReplace = value;
                OnPropertyChanged(nameof(ShowReplace));
            }
        }

        private bool _isShowReplace = true;

        public bool IsShowReplace
        {
            get => _isShowReplace;
            set
            {
                _isShowReplace = value;
                OnPropertyChanged(nameof(IsShowReplace));
            }
        }

        private string _replaceText = "";

        public string ReplaceText
        {
            get => _replaceText;
            set
            {
                _replaceText = value;
                OnPropertyChanged(nameof(ReplaceText));
            }
        }

        private string _searchText = "";

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        private bool _matchCase;

        public bool MatchCase
        {
            get => _matchCase;
            set
            {
                _matchCase = value;
                OnPropertyChanged(nameof(MatchCase));
            }
        }

        private bool _useRegex;

        public bool UserRegex
        {
            get => _useRegex;
            set
            {
                _useRegex = value;
                OnPropertyChanged(nameof(UserRegex));
            }
        }


        private bool _wholeWord;

        public bool WholeWord
        {
            get => _wholeWord;
            set
            {
                _wholeWord = value;
                OnPropertyChanged(nameof(WholeWord));
            }
        }
        #endregion



        public ObservableCollection<Tab> Tabs { get; set; } = new ObservableCollection<Tab> { new Tab { Title = "tab1" } };
    }

    public class Tab
    {
        public string Title { get; set; }


        private BraceFoldingStrategy BraceFoldingStrategy { get; set; }

        private FoldingManager FoldingManager { get; set; }

        private SearchPanel SearchPanel { get; set; }

        public ICommand LoadedCommand { get; } = new CustomCommand(par =>
        {
            var editor = (TextEditor)par;
            editor.TextChanged += Editor_TextChanged;
            var context = (Tab)editor.DataContext;
            context.FoldingManager = FoldingManager.Install(editor.TextArea);
            context.BraceFoldingStrategy = new BraceFoldingStrategy();
            context.SearchPanel = SearchPanel.Install(editor);
        });

        private static void Editor_TextChanged(object sender, EventArgs e)
        {
            var editor = (TextEditor)sender;
            var context = (Tab)editor.DataContext;
            context.BraceFoldingStrategy.UpdateFoldings(context.FoldingManager, editor.Document);
        }

        public ICommand OpenFileCommand { get; } = new CustomCommand(async par =>
        {
            var editor = (TextEditor)par;
            var dialog = new OpenFileDialog { Filter = "Json|*.json" };
            if (dialog.ShowDialog() == true)
            {
                editor.Text = await File.ReadAllTextAsync(dialog.FileName);
            }
        });

        public ICommand SaveCommand { get; } = new CustomCommand(async par =>
        {
            var editor = (TextEditor)par;
            await SaveDocument(editor.Text);
        });

        private static async Task SaveDocument(string text)
        {
            var dialog = new SaveFileDialog { Filter = "Json|*.json" };
            if (dialog.ShowDialog() == true)
            {
                await File.WriteAllTextAsync(dialog.FileName, text);
            }
        }

        public ICommand FormatCommand { get; } = new CustomCommand(async par =>
        {
            var editor = (TextEditor)par;
            var context = (Tab)editor.DataContext;
            try
            {
                if (string.IsNullOrWhiteSpace(editor.Text))
                {
                    return;
                }
                var s = new JsonSerializer();
                JsonReader reader = new JsonTextReader(new StringReader(editor.Text));
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
                    editor.Text = sWriter.ToString();
                    context.BraceFoldingStrategy.UpdateFoldings(context.FoldingManager, editor.Document);
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
        });

        public ICommand CleanCommand { get; } = new CustomCommand(async par =>
        {
            var editor = (TextEditor)par;
            editor.Text = "";
        });

        public ICommand ReplaceCommand { get; } = new CustomCommand(async par =>
        {
            var editor = (TextEditor)par;
            editor.Text = "";
        });
    }
}
