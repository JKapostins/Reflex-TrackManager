using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using PollDs19TrackList;

namespace PollDs19TrackList.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void TestPollDs19TrackListFunction()
        {
            // Invoke the lambda function and confirm the string was upper cased.
            var function = new Function();
            var context = new TestLambdaContext();
            function.FunctionHandler(context);
        }
    }
}
