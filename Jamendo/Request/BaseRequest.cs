using RequestWithLaz0rz;
using RequestWithLaz0rz.Type;

namespace Jamendo.Request
{
    abstract public class BaseRequest<TResponse> : Request<TResponse>
    {
        protected BaseRequest()
        {
            AddParameter(new Parameter("client_id", ClientId));
            AddParameter(new Parameter("format", "json"));
        }

        protected override string BaseUri
        {
            get { return "https://api.jamendo.com/v3.0/"; }
        }

        protected override ContentType ContentType
        {
            get { return ContentType.Json; }
        }

        private static string ClientId
        {
            get { return "b6747d04"; } //TODO remove test client ID
        }
    }
}
