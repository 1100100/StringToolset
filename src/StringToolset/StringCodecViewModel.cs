using System;
using System.Collections.Generic;
using System.Text;

namespace StringToolset
{
    public class StringCodecViewModel : ViewModelBase
    {
        private string _sourceText;
        public string SourceText
        {
            get => _sourceText; set
            {
                _sourceText = value;
                OnPropertyChanged(nameof(SourceText));
            }
        }

        private string _encodeText;
        public string EncodeText
        {
            get => _encodeText; set
            {
                _encodeText = value;
                OnPropertyChanged(nameof(EncodeText));
            }
        }
    }

}
