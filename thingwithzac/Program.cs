using System;
using System.Linq;

namespace thingwithzac
{
    class Program
    {
        static void Main(string[] args)
        {
            var filename = "federalist_papers.txt";
            //var filename = "obama.txt";
            FileHandler handler = new FileHandler(filename);
            var words = handler.DistinctWordsWithEnd();
            var network = MarkovNetwork.GenerateNetwork(words, true);
            string input;
            do
            {
                Console.WriteLine(network.GenerateSentence());
                input = Console.ReadLine();
            } while (input != "q");
        }

    }
}
