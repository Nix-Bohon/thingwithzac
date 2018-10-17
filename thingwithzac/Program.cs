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
            var text = FileHandler.ReadFile("obama.txt");
            foreach(var item in text)
            {
                Console.WriteLine(item);
            }
            Console.Read();
        }

    }

    public class FileHandler
    {
        private string text;
        private const string nonPunctuationRegex = @"(\w+-\w+)|(\w+'{1}\w+)|(\w+)";
        private const string punctuationRegex = @"(?<punctuation>\p{P})";
        private const string combinedRegex = @"(\w+-\w+)|(\w+'{1}\w+)|(?<punctuation>\p{P})|(\w+)";

        public static Func<string, bool> CheckRegEx = (string str) => Regex.IsMatch(str, punctuationRegex) &&
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

        public IEnumerable<string> SplitWithPunctuation()
        {
            var matches = Regex.Matches(this.text, combinedRegex);
            return GetStringEnumeratorFromMatchCollection(matches);
        }

        public IEnumerable<string> DistinctWithoutPunctuation()
        {
            var matches = Regex.Matches(this.text, nonPunctuationRegex);
            var query = from Match match in matches
                        select match.Value;
            return query.Distinct();
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
            var map = new FrequencyMap();
            foreach(string token in arr)
            {
                map.Add(token, CountFrequencyByPosition(token, arr));
            }
        }

        public Frequency CountFrequencyByPosition(string word, string[] tokens)
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
            return freq;
        }

        private Position CheckPosition(string word, string before, string after)
        {
            if (before == null || FileHandler.CheckRegEx(before))
                return Position.Beginning;
            if (after == null || FileHandler.CheckRegEx(after))
                return Position.End;
            return Position.Middle;
        }

    }

    public class WordMap : Dictionary<string, FrequencyMap>
    {
        public WordMap(List<string> tokens)
        {

        }
    }
}
