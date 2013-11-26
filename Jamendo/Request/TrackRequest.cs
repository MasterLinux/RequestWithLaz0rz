using Jamendo.Request.Model;
using RequestWithLaz0rz;
using RequestWithLaz0rz.Type;

namespace Jamendo.Request
{
    public class TrackRequest : Request<TrackRequestModel>
    {
        public TrackRequest(string clientId)
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
}
