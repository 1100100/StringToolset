// Copyright (c) 2009 Daniel Grunwald
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using System.Linq;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace StringToolset
{
    /// <summary>
    /// Allows producing foldings from a document based on braces.
    /// </summary>
    public class BraceFoldingStrategy
    {
        /// <summary>
        /// Gets/Sets the opening brace. The default value is '{'.
        /// </summary>
        public string[] OpeningBrace { get; set; }

        /// <summary>
        /// Gets/Sets the closing brace. The default value is '}'.
        /// </summary>
        public string[] ClosingBrace { get; set; }

        public char[] Braces { get; set; }

        public string[] Line { get; set; }

        /// <summary>
        /// Creates a new BraceFoldingStrategy.
        /// </summary>
        public BraceFoldingStrategy()
        {
            OpeningBrace = new[] { "{", "[" };
            ClosingBrace = new[] { "}", "]" };
            Line = new[] { "\n", "\r" };
            Braces = new[] { '{', '[', '}', ']', '\n', '\r' };
        }

        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            int firstErrorOffset;
            IEnumerable<NewFolding> newFoldings = CreateNewFoldings(document, out firstErrorOffset);
            manager.UpdateFoldings(newFoldings, firstErrorOffset);
        }

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;
            return CreateNewFoldings(document);
        }

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        public IEnumerable<NewFolding> CreateNewFoldings(ITextSource document)
        {
            var newFoldings = new List<NewFolding>();
            var startOffsets = new Stack<int>();
            var lastNewLineOffset = 0;
            var currentIndex = 0;
            var json = document.Text;
            //for (var i = 0; i < document.TextLength; i++)
            //{
            //    var c = document.GetCharAt(i);
            //    if (c == '{')
            //    {
            //        startOffsets.Push(i);
            //    }
            //    else if (c == '}' && startOffsets.Count > 0)
            //    {
            //        var startOffset = startOffsets.Pop();
            //        // don't fold if opening and closing brace are on the same line
            //        if (startOffset < lastNewLineOffset)
            //        {
            //            newFoldings.Add(new NewFolding(startOffset, i + 1));
            //        }
            //    }
            //    else if (c == '\n' || c == '\r')
            //    {
            //        lastNewLineOffset = i + 1;
            //    }
            //}
            while (true)
            {
                //开始标记
                var index = json.IndexOfAny(Braces, currentIndex);
                if (index == -1)
                    break;
                currentIndex = index + 1;
                var brace = json.Substring(index, 1);
                if (OpeningBrace.Contains(brace))
                    startOffsets.Push(index);
                else if (ClosingBrace.Contains(brace) && startOffsets.Any())
                {
                    var start = startOffsets.Pop();
                    if (start < lastNewLineOffset)
                        newFoldings.Add(new NewFolding(start, index + 1));
                }
                else if (Line.Contains(brace))
                    lastNewLineOffset = index + 1;

            }
            newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return newFoldings;
        }
    }
}
