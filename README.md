RequestWithLaz0rz
=================

#Dependencies
* Json.NET *(Shouldn't be a hard dependency, an abstract implementation of a XML and JSON parser is the way to go)*
* Microsoft HTTP Client Libraries

#How to use
Firstly you should implement a `BaseRequest` for the API you will access, so each request in your application can inherit from it.

```csharp
using RequestWithLaz0rz;
using RequestWithLaz0rz.Type;

namespace YourApp.Request
{
    abstract public class BaseRequest<TResponse> : Request<TResponse>
    {
        // add all default parameter and header inside the constructor
        // required by each request
        protected BaseRequest()
        {
            AddParameter(new KeyValuePair("client_id", ClientId));
            AddParameter(new KeyValuePair("format", "json"));
        }

        // set the base URL which points to your API
        protected override string BaseUri
        {
            get { return "https://api.yourapi.com/"; }
        }

        // define the data format of the response, so the correct parser can be selected
        protected override ContentType ContentType
        {
            get { return ContentType.Json; }
        }

        // use setter for more readability
        private static string ClientId
        {
            get { return "public_key"; }
        }
    }
}
```

Now you are able to implement requests for specific API endpoints like the following one.

```csharp
namespace YourApp.Request
{
    abstract public class DocumentRequest : BaseRequest<DocumentModel>
    {
        private readonly int _identifier;
        
        // add all specific parameter and headers. 
        // in addition use parameter for path parameter, etc.  
        protected DocumentRequest(string userName, int identifier)
        {
            AddHeader(new KeyValuePair("X-USER-NAME", userName));
            _identifier = identifier;    
        }

        // set the path which points to a specific API endpoint
        protected override string Path
        {
            get { return string.Format("documents/{0}", _identifier); }
        }
    }
}

```

To get the response you just need to to call `Request.GetResponseAsync()`

```csharp
var request = new DocumentRequest("testUser", 1);
var document = await request.GetResponseAsync();
```
