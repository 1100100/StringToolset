using Newtonsoft.Json.Linq;

namespace StringToolset
{
    public class JsonTreeViewModel : ViewModelBase
    {
        private JObject _jsonTree;
        public JObject JsonTree
        {
            get => _jsonTree;
            set
            {
                _jsonTree = value;
                OnPropertyChanged(nameof(JsonTree));
            }
        }

        private string _jsonRaw;

        public string JsonRaw
        {
            get => _jsonRaw;
            set
            {
                _jsonRaw = value;
                OnPropertyChanged(nameof(JsonRaw));
            }
        }
    }
}
