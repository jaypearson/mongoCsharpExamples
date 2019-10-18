using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace csharpExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("test");
            var collection = db.GetCollection<BsonDocument>("test");
            var models = new List<WriteModel<BsonDocument>>();
            for (int i = 0; i < 10; i++)
            {
                var doc = new BsonDocument("_id", i);
                var model = new UpdateOneModel<BsonDocument>(doc, new BsonDocument("$set", new BsonDocument("a", i * 2)));
                var insertModel = new InsertOneModel<BsonDocument>(doc);
                model.IsUpsert = true;
                models.Add(model);
                if (i % 5 == 0)
                {
                    models.Add(insertModel);
                }
            }
            try
            {
                var result = collection.BulkWrite(models, new BulkWriteOptions { IsOrdered = false });
            }
            catch (MongoWriteConcernException mwce)
            {
                var result = mwce.WriteConcernResult;

                Console.Out.WriteLine(mwce.Message);
            }
            catch (MongoBulkWriteException mbwe)
            {
                foreach (var error in mbwe.WriteErrors)
                {
                    int index = error.Index;
                    string message = error.Message;
                    int code = error.Code;
                    var category = error.Category;
                    category.Equals(ServerErrorCategory.DuplicateKey);
                    category.Equals(ServerErrorCategory.ExecutionTimeout);
                    Console.Out.WriteLine($"Index #{index} - {message} - code {code} - category {category}");
                }
            }

        }
    }
}
