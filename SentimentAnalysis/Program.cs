using System;
using System.IO;

namespace SentimentAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var directory = Directory.GetFiles("Assets");
            foreach (var file in directory)
            {
                var text = File.ReadAllLines(file);
                Console.WriteLine($"{ string.Join('\n', text) }");

            }

            Console.WriteLine("\nPress enter to exit...");
            Console.ReadKey();
        }
    }
}