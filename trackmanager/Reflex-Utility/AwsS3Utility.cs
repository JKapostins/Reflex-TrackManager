using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ReflexUtility
{
    public class AwsS3Utility
    {
        public static async Task UploadFileAsync(Stream fileStream, string bucketName, string keyName, RegionEndpoint bucketRegion)
        {
            using (var s3Client = new AmazonS3Client(bucketRegion))
            {
                var fileTransferUtility = new TransferUtility(s3Client);
                await fileTransferUtility.UploadAsync(fileStream, bucketName, keyName);
            }
        }

        public static bool DeleteObject(string bucketName, string keyName, RegionEndpoint bucketRegion)
        {
            bool success = false;
            try
            {
                using (var client = new AmazonS3Client(bucketRegion))
                {
                    var deleteObjectRequest = new DeleteObjectRequest
                    {
                        BucketName = bucketName,
                        Key = keyName
                    };

                    Console.WriteLine(string.Format("Deleting {0}/{1} from s3", bucketName, keyName));
                    client.DeleteObjectAsync(deleteObjectRequest).Wait();
                    success = true;
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            return success;
        }
    }
}