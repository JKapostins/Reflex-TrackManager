
using Amazon.Lambda.TestUtilities;
using Xunit;

namespace UploadReflexTrackToS3.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void TestTrackProcessor()
        {
            // Invoke the lambda function and confirm the string was upper cased.
            var function = new UploadReflexTrackToS3();
            var context = new TestLambdaContext();

            //GNARLY_TODO: input a sqs event here instead of null;
            function.FunctionHandler(null, context);
        }

    }
}
