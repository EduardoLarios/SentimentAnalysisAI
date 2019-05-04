using System;
using System.IO;

namespace SentimentAnalysis
{
    class Program
    {

        static void PositiveOrNegative(string[] lexicon, string[] Tweets)
        {
            double positive = 0;
            double negative = 0;

            foreach (var tweet in Tweets)
            {

                foreach (var word in lexicon)
                {
                    var first = word.Split(',');
                    if (first[0] == tweet)
                    {
                        Double.TryParse(first[1], out double temp);
                        if (temp > 0)
                            positive += temp;
                        else
                            negative -= temp;

                    }
                }
            }
        }
        static void Main(string[] args)
        {
            var lexicon = File.ReadAllLines("Assets/lexicon.txt");
            var adverbs = File.ReadAllLines("Assets/adverbs_clean.csv");
            var training = File.ReadAllLines("Tweets/training.txt");

            Console.WriteLine("\nPress enter to exit...");
            Console.ReadKey();
        }
    }
}