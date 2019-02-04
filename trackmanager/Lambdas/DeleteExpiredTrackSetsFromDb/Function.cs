using System;
using System.IO;
using System.Text;

using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.DynamoDBv2.Model;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon;
using System.Collections.Generic;
using ReflexUtility;
using Amazon.DynamoDBv2.DocumentModel;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DeleteExpiredTrackSetsFromDb
{
    public class Function
    {
        public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            context.Logger.LogLine($"Beginning to process {dynamoEvent.Records.Count} records...");

            bool containsInsertRecord = false;

            //Search for an insert record
            foreach (var record in dynamoEvent.Records)
            {
                if(record.EventName == "INSERT")
                {
                    containsInsertRecord = true;
                    break;
                }
            }

            if (containsInsertRecord)
            {
                var dynamoDbClient = new AmazonDynamoDBClient(RegionEndpoint.USEast1);
                var dynamoContext = new DynamoDBContext(dynamoDbClient);
                long currentTime = TimeUtility.DateTimeToUnixTimeStamp(DateTime.UtcNow);
                long expiredTime = currentTime - (TrackSharing.LifeSpanMinutes * 60);

                context.Logger.LogLine($"Looking for records older than {expiredTime}");
                var exipredItems = new List<ScanCondition>()
                {
                    new ScanCondition("CreationTime", ScanOperator.LessThanOrEqual, expiredTime)
                };
                var expiredItems = await dynamoContext.ScanAsync<TrackList>(exipredItems).GetRemainingAsync();

                foreach (var trackSet in expiredItems)
                {
                    context.Logger.LogLine($"Deleting {trackSet.Name}...");
                    var request = new DeleteItemRequest
                    {
                        TableName = "SharedReflexTrackLists",
                        Key = new Dictionary<string, AttributeValue>() { { "Name", new AttributeValue { S = trackSet.Name } } },
                    };
                    var response = await dynamoDbClient.DeleteItemAsync(request);
                }
                
            }

            context.Logger.LogLine("Stream processing complete.");
        }
    }
}