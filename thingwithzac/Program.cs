using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace thingwithzac
{
    class Program
    {
        static void Main(string[] args)
        {
            FileHandler handler = new FileHandler("obama.txt");

            var words = handler.DistinctWithoutPunctuation();
            foreach(var item in words)
            {
                Console.WriteLine(item);
            }
            Console.Read();
        }

    }

    public class FileHandler
    {
        private string text;
        public const string nonPunctuationRegex = @"(\w+-\w+)|(\w+'{1}\w+)|(\w+)";
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

        public IEnumerable<string> NextWordsByWord(string word)
        {
            return GetStringEnumeratorFromRegex(nextWordRegex, true);
        }

    }

    public class Frequency
    {
        public int BeginningCount { get; private set; }
        public int MiddleCount { get; private set; }
        public int EndCount { get; private set; }

        public Frequency(int beginningCount, int middleCount, int endCount)
        {
            BeginningCount = beginningCount;
            MiddleCount = middleCount;
            EndCount = endCount;
        }

        public Frequency() : this(0, 0, 0) { }

        public int TotalCount => BeginningCount + MiddleCount + EndCount;

        public void IncrementBeginningCount(int amount = 1) => BeginningCount += amount;
        public void IncrementMiddleCount(int amount = 1) => MiddleCount += amount;
        public void IncrementEndCount(int amount = 1) => EndCount += amount;
    }

    public class FrequencyMap : Dictionary<string, Frequency>
    {
        private enum Position
        {
            Beginning,
            Middle,
            End
        }

        private FrequencyMap() { }

        public FrequencyMap(IEnumerable<string> tokens)
        {
            string[] arr = tokens.ToArray();
            foreach(string token in arr)
            {
                Add(token, CountFrequencyByPosition(token, arr));
            }
        }

        public void AddItem(string word, string[] tokens)
        {
            var query =
                from int index in tokens
                where tokens[index] == word
                select index;
            Frequency freq = new Frequency();
            foreach (int index in query)
            {
                string before = index == 0 ? null : tokens[index - 1];
                string after = index == tokens.Length - 1 ? null : tokens[index + 1];
                switch (CheckPosition(tokens[index], before, after))
                {
                    case Position.Beginning:
                        freq.IncrementBeginningCount();
                        break;
                    case Position.Middle:
                        freq.IncrementMiddleCount();
                        break;
                    case Position.End:
                        freq.IncrementEndCount();
                        break;
                };
            }
            Add(word, )
        }

        private Position CheckPosition(string word, string before, string after)
        {
            if (before == null || FileHandler.CheckRegEx(before, FileHandler.punctuationRegex))
                return Position.Beginning;
            if (after == null || FileHandler.CheckRegEx(after, FileHandler.punctuationRegex))
                return Position.End;
            return Position.Middle;
        }

    }

    public class WordMap : Dictionary<string, FrequencyMap>
    {
        public WordMap(List<string> tokens)
        {
            foreach(string token in tokens)
            {
                Add(token, new FrequencyMap(tokens))
            }
        }
    }
}
