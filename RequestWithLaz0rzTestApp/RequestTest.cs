using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace RequestWithLaz0rzTestApp
{
    [TestClass]
    public class RequestTest
    {

        [TestInitialize]
        public void Init()
        {
            
        }

        [TestMethod]
        public void StartRequest()
        {
            /*
            var request = new JamendoTracksRequest("b6747d04");
            var privateObject = new PrivateObject(request);
            */

            Assert.AreEqual("s", "r", "Strange stuff!");
        }
    }
}
