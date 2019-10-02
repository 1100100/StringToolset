using System.Windows;

namespace StringToolset
{
    public class JsonViewerModel : ViewModelBase
    {
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
    }
}
