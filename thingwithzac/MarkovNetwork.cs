using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace thingwithzac
{
    class MarkovNetwork : Dictionary<string, Dictionary<string, int>>
    {
        // Some consts that can be tweaked as needed.
        private const int maxAttempts = 10;
        private const int defaultCutoff = 30;
        private const int defaultLength = 15;
        private const string punctuationRegex = @"[.?!]";

        private MarkovNetwork(IEnumerable<string> corpus, bool hasPunctuation) : base()
        {
            this.corpus = corpus;
            this.hasPunctuation = hasPunctuation;
        }

        // object variables
        private Random r = new Random();
        private IEnumerable<string> corpus;
        private bool hasPunctuation;

        // Helper methods!
        private static bool containsPunctuation(string s) => Regex.IsMatch(s, punctuationRegex);
        private static bool containsPunctuation(IEnumerable<string> l) => l.Any(containsPunctuation);
        private List<string> getWordSeed() => corpus.Skip(r.Next(0, corpus.Count() - 4)).Take(3).ToList();


        /// <summary>
        /// Create a new network.
        /// </summary>
        /// <param name="corpus">The raw input sentences</param>
        /// <param name="hasPunctuation">Use this option if your input stream includes '.', '?', and '!' to denote the end of sentences</param>
        /// <returns></returns>
        internal static MarkovNetwork GenerateNetwork(IEnumerable<string> corpus, bool hasPunctuation = false)
        {
            var mn = new MarkovNetwork(corpus, hasPunctuation);

            // This list keeps track of the three previous words.
            var priorsQueue = new List<string>();
            foreach(var word in corpus)
            {
                // Add the prior and the posterior to the dictionary
                if(priorsQueue.Count == 3 && !(hasPunctuation && containsPunctuation(priorsQueue)))
                    mn.AddWords(string.Join(" ", priorsQueue), word);

                // Update priors word queue.
                priorsQueue.Add(word);
                if(priorsQueue.Count == 4)
                    priorsQueue.RemoveAt(0);
            }
            return mn;
        }

        private void AddWords(string prior, string posterior)
        {
            if (!ContainsKey(prior))
                Add(prior, new Dictionary<string, int>());
            if (!this[prior].ContainsKey(posterior))
                this[prior].Add(posterior, 0);
            this[prior][posterior] += 1;
        }

        /// <summary>
        /// This method is used to Get the next word when generating a random sequence of words.
        /// </summary>
        /// <param name="prior">This is expected to be a string with three words separated by spaces.</param>
        /// <returns></returns>
        private string GetPosterior(string prior)
        {
            var frequencies = this[prior].Select(kv => kv);
            var count = frequencies.Sum(kv => kv.Value);
            var test = r.Next(1, count);
            foreach(var kv in frequencies)
            {
                test -= kv.Value;
                if (test <= 0)
                    return kv.Key;
            }
            throw new Exception("This should not happen! Zac done goofed");
        }

        internal string GenerateSentence(int cutoff = defaultCutoff)
        {
            if (!hasPunctuation)
                throw new Exception("This network does not have a concept of the ending of sentences and so cannot generate a sentence.");
            List<string> words;
            int counter = 0;
            do
            {
                words = getWordSeed();
                counter++;
                if(counter == maxAttempts)
                {
                    throw new Exception($"Couldn't find a reasonable start string in {maxAttempts} attempts. Gave up.");
                }
            } while (containsPunctuation(words));

            var punctuation = ".";
            while(words.Count < cutoff)
            {
                var prior = string.Join(" ", words.GetRange(words.Count - 3, 3));
                var posterior = GetPosterior(prior);
                if (containsPunctuation(posterior))
                    break;
                words.Add(posterior);
            }
            return string.Join(" ", words) + punctuation;
        }

        internal string GenerateSomeWords(int length = defaultLength)
        {
            if(length < 4)
                throw new Exception($"Length = {length} is invalid. Length must be greater than 3.");
            if (hasPunctuation)
                throw new NotImplementedException("This network contains sentence ending glyphs and does not support this method.");
            // Grab a three word sequence from the original corpus.
            var words = getWordSeed();
            while(words.Count < length)
            {
                var prior = string.Join(" ", words.GetRange(words.Count - 3, 3));
                words.Add(GetPosterior(prior));
            }
            return string.Join(" ", words);
        }
    }
}
