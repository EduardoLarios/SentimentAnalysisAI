using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExtensionMethods;

namespace SentimentAnalysis
{
    class Program
    {

        static Tuple<List<double>, List<double>> PositiveOrNegative(IEnumerable<string> Lexicon, IEnumerable<string> Tweets)
        {
            var wordsScore = new Dictionary<string, double>();
            var separated = Lexicon.Select(line => line.Split(','));
            var separatedPairs = separated.Select(pair => new { key = pair.First(), value = double.Parse(pair.Last()) });

            foreach (var pair in separatedPairs)
            {
                if (!wordsScore.ContainsKey(pair.key))
                    wordsScore.Add(pair.key, pair.value);
            }

            var ListPositive = new List<double>();
            var ListNegative = new List<double>();
            var trimChars = new char[] { ':', ',', '"', '*', '.', ';', '(', ')', '@', '#', '!', '?' };

            foreach (var sentence in Tweets)
            {
                double positive = 0;
                double negative = 0;
                var cleanSentence = sentence.Split(' ').Select(word => word.Trim(trimChars)).ToList();

                if (wordsScore.ContainsKey(cleanSentence.First()))
                {
                    if (wordsScore[cleanSentence.First()] > 0)
                        positive += wordsScore[cleanSentence.First()];
                    else
                        negative += wordsScore[cleanSentence.First()];
                }

                for (int i = 1; i < cleanSentence.Count(); i++)
                {
                    if (wordsScore.ContainsKey(cleanSentence[i]))
                    {
                        var word = cleanSentence[i];
                        var previous = cleanSentence[i - 1];

                        if (previous.ContainsNegation())
                        {
                            if (wordsScore[word] > 0)
                                negative -= wordsScore[word];
                            else
                                positive -= wordsScore[word];
                        }
                        else
                        {
                            if (wordsScore[word] > 0)
                                positive += wordsScore[word];
                            else
                                negative += wordsScore[word];
                        }

                    }

                }

                ListPositive.Add(positive);
                ListNegative.Add(negative);
            }
            return Tuple.Create(ListPositive, ListNegative);
            //var result = ListPositive.Zip(ListNegative, (pos, neg) => new { Positive = pos, Negative = neg });
        }
        static Tuple<List<double>, List<double>> GetMultipliers(IEnumerable<string> Adverbs, IEnumerable<string> Tweets)
        {
            var trimChars = new char[] { ':', ',', '"', '*', '.', ';', '(', ')', '@', '#', '!', '?' };
            var noWeirdChars = Tweets.Select(tweet => tweet.Split(' ').Select(word => word.Trim(trimChars)));
            var sentences = noWeirdChars.Select(words => string.Join(' ', words));

            var adverbs = new Dictionary<string, string>();
            var tuples = Adverbs.Select(line => line.Split(',')).Select(array => new { key = array.First(), value = array.Last() });
            var amplifiers = new List<double>();
            var diminishers = new List<double>();

            foreach (var pair in tuples)
            {
                if (!adverbs.ContainsKey(pair.key))
                    adverbs.Add(pair.key, pair.value);
            }

            foreach (var sentence in Tweets)
            {
                double amplifier = 1;
                double diminisher = 1;

                foreach (var adverb in adverbs.Keys)
                {
                    if (sentence.Contains(adverb))
                    {
                        var index = sentence.IndexOf(adverb);
                        var slice = sentence.Substring(0, index);
                        var contains = slice.ContainsNegation();
                        if (contains)
                        {
                            if (adverbs[adverb] == "amplifier")
                            {
                                diminisher *= 1.5;

                            }

                            else
                            {
                                amplifier *= 1.5;

                            }

                        }

                        else
                        {

                            if (adverbs[adverb] == "amplifier")
                            {
                                amplifier *= 1.5;
                            }

                            else
                            {
                                diminisher *= 1.5;
                            }
                        }

                    }

                }

                amplifiers.Add(amplifier);
                diminishers.Add(diminisher);

            }

            return Tuple.Create(amplifiers, diminishers);

        }
        static void Main(string[] args)
        {
            var lexicon = File.ReadAllLines("Assets/lexicon.txt").Select(line => line.ToLower());
            var adverbs = File.ReadAllLines("Assets/adverbs.csv").Select(line => line.ToLower());
            var training = File.ReadAllLines("Tweets/testdata.txt").Select(line => line.ToLower());

            var train = training.SelectMany(line => line.Split('^').Take(1));

            var score = PositiveOrNegative(lexicon, train);
            var multipliers = GetMultipliers(adverbs, train);
            //var results = training.SelectMany(line => line.Split('^').Skip(1)).ToArray();
            var header = 
            @"@RELATION sentiment
@ATTRIBUTE F1 REAL
@ATTRIBUTE F2 REAL
@ATTRIBUTE F3 REAL
@ATTRIBUTE F4 REAL
@ATTRIBUTE RESULT {0,1}

@DATA";

            var trainingSet = new List<string>{ header };
            for (int i = 0; i < score.Item1.Count(); i++)
            {
                trainingSet.Add($"{ score.Item1[i] },{ score.Item2[2] },{ multipliers.Item1[i] },{ multipliers.Item2[i] },?");
            }

            File.WriteAllLines("test.arff", trainingSet);

            Console.WriteLine("Training set is ready");
            Console.WriteLine("\nPress enter to exit...");
            Console.ReadKey();
	    //var output1 = "export CLASSPATH=\"weka-3-8-3/weka.jar\"".Bash();
            var output = "java weka.classifiers.meta.RandomCommittee -l sentiment.model -p 0 -T test.arff > prediction.txt".Bash();
            var pred = File.ReadAllText("prediction.txt").Split('\n');

            for (int i = 5; i < pred.Length; i++)
            {
                
                if(pred[i].Contains(":0"))
                {
                    System.Console.WriteLine(training.ToList()[i-5]+" Negative");
                    System.Console.WriteLine();
                }
                else if(pred[i].Contains(":1"))
                {
                    System.Console.WriteLine(training.ToList()[i-5]+" Positive"); 
                }
            }
        }
    }
}
