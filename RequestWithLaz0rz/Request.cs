using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RequestWithLaz0rz.Data;
using RequestWithLaz0rz.Exception;
using RequestWithLaz0rz.Extension;
using RequestWithLaz0rz.Handler;
using RequestWithLaz0rz.Serializer;
using RequestWithLaz0rz.Type;
using HttpMethod = RequestWithLaz0rz.Type.HttpMethod;

namespace RequestWithLaz0rz
{
    /*
    public abstract class Request<TResponse> : IRequest<TResponse>
    {
        private readonly Dictionary<string, string> _parameter = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _body = new Dictionary<string, string>();
        private readonly RequestQueue _queue = RequestQueue.GetRequestQueue();
        private readonly HttpClient _client = new HttpClient();
        private SemaphoreSlim _completedSignal;
        private CompletedEventArgs<TResponse> _completedEventArgs;

        #region event handler

        /// <summary>
        /// Event which is invoked whenever the request is
        /// successfully executed. 
        /// </summary>
        public event CompletedHandler<TResponse> Completed;

        /// <summary>
        /// Executes the completed handler.
        /// </summary>
        /// <param name="args">Event arguments</param>
        private void OnCompleted(CompletedEventArgs<TResponse> args)
        {
            var handler = Completed;
            if (handler != null) handler(this, args);
        }

        /// <summary>
        /// Event which is invoked whenever an error occured
        /// during the request executing.
        /// </summary>
        public event CompletedHandler<TResponse> Error;

        /// <summary>
        /// Executes the error handler
        /// </summary>
        /// <param name="args">Event arguments</param>
        private void OnError(CompletedEventArgs<TResponse> args)
        {
            var handler = Error;
            if (handler != null) handler(this, args);
        }

        public event ValidationHandler<TResponse> Validation;

        /// <summary>
        /// Invokes the validation event.
        /// </summary>
        /// <param name="args">Event arguments</param>
        /// <returns>true if response is valid and no error occured, false otherwise</returns>
        private bool OnValidation(CompletedEventArgs<TResponse> args)
        {
            var handler = Validation;
            return handler == null || handler(this, args);
        }

        #endregion

        /// <summary>
        /// Gets the queue handle used to find
        /// the requests position inside the
        /// priority queue 
        /// </summary>
        public int QueueHandle { get; set; }

        public RequestPriority Priority { get; private set; }

        public abstract string BaseUri
        {
            get;
        }

        public abstract string Path
        {
            get;
        }

        public string Accept { get; set; }

        public string UserAgent { get; set; }

        public abstract ContentType ContentType
        {
            get;
        }

        public abstract HttpMethod HttpMethod
        {
            get;
        }

        /// <summary>
        /// Builds and gets the whole request URI.
        /// </summary>
        private string Uri
        {
            get
            {
                var builder = new StringBuilder(BaseUri);
                string query;
                string path;

                //try to add path
                if (TryAddPath(BaseUri, Path, out path))
                {
                    builder.Append(path);
                }

                //try to add query
                if (TryBuildQuery(_parameter, out query))
                {
                    builder.Append(query);
                }

                return builder.ToString();
            }
        }

        private static bool TryAddPath(string baseUri, string path, out string s)
        {
            if (string.IsNullOrEmpty(baseUri) || string.IsNullOrEmpty(path))
            {
                s = null;
                return false;
            }

            var baseUriEndsWithSlash = baseUri.EndsWith("/");
            var pathStartsWithSlash = path.StartsWith("/");

            //add slash if not exists
            if (!baseUriEndsWithSlash && !pathStartsWithSlash)
            {
                s = new StringBuilder().Append("/").Append(path).ToString();
            }

            //remove slash if both contains 
            else if (baseUriEndsWithSlash && pathStartsWithSlash)
            {
                s = path.TrimStart('/');
            }

            //everything is OK ;)
            else {
                s = path;
            }

            return true;
        }

        /// <summary>
        /// Flag which indicates whether the 
        /// request is currently executing.
        /// </summary>
        public bool IsBusy { get; private set; }

        public bool IsAborted { get; private set; } //TODO imlement

        /// <summary>
        /// Tries to build the URL query
        /// </summary>
        /// <param name="paramDict">Dictionary of KeyValuePair</param>
        /// <param name="query">The required query or null if no KeyValuePair added</param>
        /// <returns></returns>
        private static bool TryBuildQuery(IReadOnlyCollection<System.Collections.Generic.KeyValuePair<string, string>> paramDict, out string query)  
        {
            //does nothing if no KeyValuePair are added
            if (!paramDict.Any())
            {
                query = null;
                return false;
            }

            //otherwise build URI part
            var first = true;
            var builder = new StringBuilder("?");
            foreach (var parameter in paramDict)
            {
                builder
                    .Append(first ? string.Empty : "&")
                    .Append(parameter.Key)
                    .Append("=")
                    .Append(parameter.Value);

                first = false;
            }

            query = builder.ToString();
            return true;
        }     
 
        /// <summary>
        /// Adds a new parameter. An already existing 
        /// parameter will be overridden.
        /// </summary>
        /// <param name="keyValuePair">The KeyValuePair to add</param>
        /// <returns>This request</returns>
        public Request<TResponse> AddParameter(KeyValuePair keyValuePair)
        {
            //remove already existing KeyValuePair
            if (_parameter.ContainsKey(keyValuePair.Key))
            {
                _parameter.Remove(keyValuePair.Key);
            }

            _parameter.Add(keyValuePair.Key, keyValuePair.Value);
            return this;
        }

        /// <summary>
        /// Adds a new header. An already existing 
        /// header will be overridden.
        /// </summary>
        /// <param name="header">The header to add</param>
        /// <returns>This request</returns>
        public Request<TResponse> AddHeader(KeyValuePair header)
        {
            //remove already existing KeyValuePair
            if (_headers.ContainsKey(header.Key))
            {
                _headers.Remove(header.Key);
            }

            _headers.Add(header.Key, header.Value);
            return this;
        }

        /// <summary>
        /// Adds a key value pair to the body. If this method
        /// is used the body is added as FormUrlEncodedContent
        /// </summary>
        /// <param name="body">The body to add</param>
        /// <returns>This request</returns>
        public Request<TResponse> AddBody(KeyValuePair body)
        {
            if (_body.ContainsKey(body.Key))
            {
                _body.Remove(body.Key);
            }

            _body.Add(body.Key, body.Value);
            return this;
        }

        //TODO implement SetBody method for objects

        /// <summary>
        /// Executes the request directly without adding
        /// it to the request queue. Use GetResponseAsync
        /// instead of this method. 
        /// </summary>
        /// <seealso cref="GetResponseAsync"/>
        public async Task RunAsync()
        {
            if (IsBusy) return; //TODO throw exception? -> already running
            IsBusy = true;

            //configure client
            _client
                .SetAcceptHeader(Accept)
                .SetUserAgent(UserAgent)
                .AddHeaders(_headers);
            
            HttpResponseMessage response = null;
            FormUrlEncodedContent content;

            switch (HttpMethod)
            {
                case HttpMethod.GET:
                    response = await _client.GetAsync(Uri);
                    break;

                case HttpMethod.HEAD:
                    //TODO implement
                    break;

                case HttpMethod.DELETE:
                    response = await _client.DeleteAsync(Uri);
                    break;

                case HttpMethod.PATCH:
                    //TODO implement
                    break;

                case HttpMethod.UPDATE:
                    //TODO implement
                    break;

                case HttpMethod.OPTIONS:
                    //TODO implement
                    break;

                case HttpMethod.POST:
                    content = new FormUrlEncodedContent(_body);
                    response = await _client.PostAsync(Uri, content);
                    break;

                case HttpMethod.PUT:
                    content = new FormUrlEncodedContent(_body);
                    response = await _client.PutAsync(Uri, content);
                    break;

                default:
                    throw new HttpMethodNotSupportedException(HttpMethod);
            }

            if (response != null)
            {
                var responseBody = await response.Content.ReadAsStreamAsync();

                TResponse result;
                if (response.IsSuccessStatusCode && TryParseResponse(responseBody, ContentType, out result))
                {                   
                    //TODO update arguments
                    _completedEventArgs = new CompletedEventArgs<TResponse>(response.Headers)
                    {
                        Response = result,
                        StatusCode = response.StatusCode,
                        IsErrorOccured = false,
                        IsCached = false //TODO set dynamically
                    };

                    //response is valid
                    if (OnValidation(_completedEventArgs))
                    {
                        IsBusy = false;
                        OnCompleted(_completedEventArgs);
                    }

                    //an error occurred
                    else
                    {
                        IsBusy = false;
                        _completedEventArgs.IsErrorOccured = true;
                        OnError(_completedEventArgs);
                    }                    
                }
                else
                {
                    IsBusy = false;

                    //TODO update arguments -> add error message
                    _completedEventArgs = new CompletedEventArgs<TResponse>(response.Headers)
                    {
                        Response = default(TResponse),
                        StatusCode = response.StatusCode,
                        IsErrorOccured = true,
                        IsCached = false
                    };

                    OnError(_completedEventArgs);                  
                }
            }
            else
            {
                IsBusy = false;
                //TODO invoke error
            }

            IsBusy = false;

            //release completed signal to finish GetResponseAsync method
            _completedSignal.Release();           
        }

        /// <summary>
        /// Adds the request into request queue and
        /// executes the request asynchroniously.
        /// </summary>
        /// <returns>The response</returns>
        public async Task<CompletedEventArgs<TResponse>> GetResponseAsync()
        {
            if (!IsBusy) {

                //initialize signal to wait for
                _completedSignal = new SemaphoreSlim(0, 1);

                //add to queue
                _queue.Enqueue(this);
            }

            //wait for completed event to continue
            await _completedSignal.WaitAsync();

            return _completedEventArgs;
        }

        /// <summary>
        /// Stops the execution of this request
        /// </summary>
        /// <returns>This request</returns>
        public async Task AbortAsync()
        {
            if (!IsBusy || _client == null) return;
            _client.CancelPendingRequests(); //TODO check whether this is the correct method
            IsBusy = false;

            //TODO implement this method awaitable (async) 
        } 

        /// <summary>
        /// Tries to parse the response
        /// </summary>
        /// <returns>Whether the response could be parsed</returns>
        private static bool TryParseResponse(Stream responseBody, ContentType format, out TResponse result)
        {
            if (responseBody != null)
            {
                //catch exception when content type 
                //of the response isn't as expected
                try
                {
                    switch (format)
                    {
                        case ContentType.Json:                            
                            return new JsonSerializer<TResponse>().TryParse(responseBody, out result);

                        case ContentType.Xml:
                            return new XmlSerializer<TResponse>().TryParse(responseBody, out result);

                        case ContentType.Text:
                            return new TextSerializer<TResponse>().TryParse(responseBody, out result);
                    }
                }
                catch (System.Exception e)
                {
                    //TODO just log the exception    
                }
            }

            result = default(TResponse);
            return false;
        }

        /// <summary>
        /// Compares the priority of this request with the priority of another request.
        /// </summary>
        /// <param name="other">The other request to compare with</param>
        /// <returns>Returns whether the priority of this request is higher than the one of the other request</returns>
        public int CompareTo(IPriorityRequest other)
        {
            return Priority.Compare(other.Priority);
        }       
    } */
}
