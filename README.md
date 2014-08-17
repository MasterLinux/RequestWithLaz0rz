RequestWithLaz0rz
=================

#Dependencies
* Json.NET *(Shouldn't be a hard dependency, an abstract implementation of a XML and JSON parser is the way to go)*
* Microsoft HTTP Client Libraries
* C5 - PriorityQueue *(will be replaced by an own implementation)*

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
