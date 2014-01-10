using Jamendo.Request.Model;
using RequestWithLaz0rz;
using RequestWithLaz0rz.Type;

namespace Jamendo.Request
{
    public class ArtistsTracksRequest : BaseRequest<TrackRequestModel>
    {
        /// <summary>
        /// Gets all tracks of a specific artist.
        /// </summary>
        /// <param name="artistName">The name of the artist</param>
        public ArtistsTracksRequest(string artistName)
        {
            
            AddParameter(new Parameter("name", "we+are+fm"));
            AddParameter(new Parameter("album_datebetween", "0000-00-00_2012-01-01"));
        }

        protected override string Path
        {
            get { return "artists/tracks"; }
        }       

        protected override HttpMethod HttpMethod
        {
            get { return HttpMethod.GET; }
        }

        public override RequestPriority Priority
        {
            get { return RequestPriority.High; }
        }
    }
}
