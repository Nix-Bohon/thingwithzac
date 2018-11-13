using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;


namespace thingwithzac
{
    public class FileHandler
    {
        private string text;
        public const string nonPunctuationRegex = @"(\w+-\w+)|(\w+'{1}\w+)|(\w+)";
        public const string wordsWithEndRegex = @"(\w+-\w+)|(\w+'{1}\w+)|(\w+)|([.!?])";
        public const string punctuationRegex = @"(?<punctuation>\p{P})";
        public const string withPunctuationRegex = @"(\w+-\w+)|(\w+'{1}\w+)|(?<punctuation>\p{P})|(\w+)";
        public const string nextWordWithPunctuationRegex = @"(?<=\bend(\s|\b))((\w)+|\p{P})(?=\s)";
        public const string nextWordRegex = @"(?<=\bend(\s|\b))(\w)+(?=\s)";

        public static Func<string, string, bool> CheckRegEx = (str, regex) => Regex.IsMatch(str, regex) &&
                !Regex.IsMatch(str, nonPunctuationRegex);

        public FileHandler(string filename)
        {
            filename = Directory.GetCurrentDirectory() + "\\" + filename;
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException();
            }
            foreach (string line in File.ReadLines(filename))
            {
                text += " " + line;
            }
        }

        private IEnumerable<string> GetStringEnumeratorFromMatchCollection(MatchCollection matches) =>
            from Match match in matches
            select match.Value;

        private IEnumerable<string> GetStringEnumeratorFromRegex(string regex, bool distinct = false)
        {
            var matches = Regex.Matches(this.text, regex);
            return GetStringEnumeratorFromMatchCollection(matches);
        }

        public IEnumerable<string> SplitWithPunctuation()
        {
            return GetStringEnumeratorFromRegex(withPunctuationRegex);
        }

        public IEnumerable<string> DistinctWithoutPunctuation()
        {
            return GetStringEnumeratorFromRegex(nonPunctuationRegex, true);
        }
        public IEnumerable<string> DistinctWordsWithEnd()
        {
            return GetStringEnumeratorFromRegex(wordsWithEndRegex, true);
        }

        public IEnumerable<string> NextWordsByWord(string word)
        {
            return GetStringEnumeratorFromRegex(nextWordRegex, true);
        }

    }
}
