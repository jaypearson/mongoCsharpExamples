using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;


namespace csharpExamples
{
    public class Program
    {
        static void Main(string[] args)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("test");
            var collection = db.GetCollection<BsonDocument>("test");

            //BulkWriteExample(collection);
            //ChangeStreamExample(collection);
            UpdateOneExample(collection);
        }

        static void UpdateOneExample(IMongoCollection<BsonDocument> collection)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var updateBuilder = Builders<BsonDocument>.Update;
            var random = new Random();

            var document = new BsonDocument("_id", 1234567);
            var updateDocument = new BsonDocument("$set", document);
            var updateOptions = new UpdateOptions() { IsUpsert = true };
            var filter = filterBuilder.Eq("_id", 1234567);
            collection.UpdateOne(filter, updateDocument, updateOptions);

            document = collection.Find(filter).First();
            Console.Out.WriteLine(document.ToJson());

            document.Set("field", random.Next());
            var arrayValues = Enumerable.Range(1, 10).Select(x => x * random.Next());
            document.Set("arrayField", new BsonArray(arrayValues));

            updateDocument = new BsonDocument("$set", document);
            collection.UpdateOne(filter, updateDocument);

            document = collection.Find(filter).First();
            Console.Out.WriteLine(document.ToJson());

        }

        static void HandleCollection1(IMongoCollection<BsonDocument> collection2, ChangeStreamDocument<BsonDocument> change)
        {
            switch (change.OperationType)
            {
                case ChangeStreamOperationType.Update:
                    // change.UpdateDescription.UpdatedFields contains the deltas for the update
                    // change.UpdateDescription.RemovedFields contains any fields that were $unset

                    collection2.UpdateOne(new BsonDocument("_id", "value"),
                        new BsonDocument("$set", change.UpdateDescription.UpdatedFields));
                    break;
                default:
                    break;
            }
        }

        static void ChangeStreamExample(IMongoCollection<BsonDocument> collection)
        {
            using (var cursor = collection.Watch())
            {
                foreach (var change in cursor.ToEnumerable())
                {
                    switch (change.CollectionNamespace.FullName)
                    {
                        case "coll1":
                            // HandleCollection1(change);
                            break;
                        case "coll2":
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        static void BulkWriteExample(IMongoCollection<BsonDocument> collection)
        {
            var models = new List<WriteModel<BsonDocument>>();
            for (int i = 0; i < 10; i++)
            {
                var doc = new BsonDocument("_id", i);
                var changes = new BsonDocument("a", i * 2);
                changes.Add("updatedAt", DateTime.Now);
                changes.Add("updatedAtUTC", DateTime.UtcNow);
                var address1 = new BsonDocument("line1", "1234 Main Street");

                changes.Add("addresses", new BsonArray() { address1 });
                var model = new UpdateOneModel<BsonDocument>(doc, new BsonDocument("$set", changes));

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
