using System;
using System.IO;
using MongoDB.Driver;
using Utf8Json;

namespace csharpExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            var filename = "twitter.json";
            var jsonString = JsonSerializer.ToJsonString(File.ReadAllBytes(filename));
            var client = new MongoClient("mongodb://localhost:27017");
        }
    }
}
