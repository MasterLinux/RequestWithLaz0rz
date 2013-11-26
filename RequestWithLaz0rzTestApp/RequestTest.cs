using Windows.Storage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using RequestWithLaz0rz;
using RequestWithLaz0rz.Type;

namespace RequestWithLaz0rzTestApp
{
    #region request implementation

    public class JamendoTracksModel
    {
        
    }

    class JamendoTracksRequest : Request<JamendoTracksModel>
    {
        public JamendoTracksRequest(string clientId)
        {
            AddParameter(new Parameter("client_id", clientId));
            AddParameter(new Parameter("format", "json"));
            AddParameter(new Parameter("name", "we+are+fm"));
            AddParameter(new Parameter("album_datebetween", "0000-00-00_2012-01-01"));
        }

        protected override string BaseUri
        {
            get { return "http://api.jamendo.com/v3.0/"; }
        }

        protected override string Path
        {
            get { return "artists/tracks"; }
        }

        protected override ContentType ContentType
        {
            get { return ContentType.Json; }
        }

        protected override HttpMethod HttpMethod
        {
            get { return HttpMethod.GET; }
        }
    }

    #endregion

    [TestClass]
    public class RequestTest
    {
        [TestMethod]
        public void StartRequest()
        {
            var request = new JamendoTracksRequest("b6747d04");
            var r = request.Start();

            Assert.AreEqual(r, request, "Strange stuff!");
        }
    }
}
