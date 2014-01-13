using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using C5;
using RequestWithLaz0rz.Exception;
using RequestWithLaz0rz.Extension;
using RequestWithLaz0rz.Handler;
using RequestWithLaz0rz.Serializer;
using RequestWithLaz0rz.Type;
using HttpMethod = RequestWithLaz0rz.Type.HttpMethod;

namespace RequestWithLaz0rz
{
    public abstract class Request<TResponse> : IRequest  
    {
        private readonly Dictionary<string, string> _parameter = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _body = new Dictionary<string, string>();
        private readonly RequestQueue _queue = RequestQueue.Instance;
        private readonly HttpClient _client = new HttpClient();       

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

        protected abstract string BaseUri
        {
            get;
        }

        protected abstract string Path
        {
            get;
        }

        protected  string Accept { private get; set; }

        protected string UserAgent { private get; set; }

        protected abstract ContentType ContentType
        {
            get;
        }

        protected abstract HttpMethod HttpMethod
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
        private bool IsBusy { get; set; }

        /// <summary>
        /// Tries to build the URL query
        /// </summary>
        /// <param name="paramDict">Dictionary of parameter</param>
        /// <param name="query">The required query or null if no parameter added</param>
        /// <returns></returns>
        private static bool TryBuildQuery(IReadOnlyCollection<System.Collections.Generic.KeyValuePair<string, string>> paramDict, out string query)  
        {
            //does nothing if no parameter are added
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
        /// Adds a new parameter. When a parameter with the same key
        /// already exists it will be overridden.
        /// </summary>
        /// <param name="parameter">The parameter to add</param>
        /// <returns>This request</returns>
        public Request<TResponse> AddParameter(Parameter parameter)
        {
            //remove already existing parameter
            if (_parameter.ContainsKey(parameter.Key))
            {
                _parameter.Remove(parameter.Key);
            }

            _parameter.Add(parameter.Key, parameter.Value);
            return this;
        }

        public Request<TResponse> AddHeader(Header header)
        {
            //remove already existing parameter
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
        /// <param name="key">The key of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>This request</returns>
        public Request<TResponse> AddBody(string key, string value)
        {
            if (_body.ContainsKey(key))
            {
                _body.Remove(key);
            }

            _body.Add(key, value);
            return this;
        }

        //TODO implement SetBody method for objects

        public IPriorityQueueHandle<IRequest> QueueHandle { get; set; }

        public abstract RequestPriority Priority { get; }

        public async void RunAsync(Action onCompleted)
        {
            if (IsBusy) return; //TODO throw exception? -> already running
            IsBusy = true;

            //configure client
            _client
                .SetAcceptHeader(Accept)
                .SetUserAgent(UserAgent)
                .AddHeaders(_headers);

            HttpResponseMessage response = null;

            switch (HttpMethod)
            {
                case HttpMethod.GET:
                    response = await _client.GetAsync(Uri);
                    break;
                case HttpMethod.POST:
                    var content = new FormUrlEncodedContent(_body);
                    response = await _client.PostAsync(Uri, content);
                    break;
                    //TODO add all method implementations
                default:
                    //TODO throw exception
                    break;
            }

            if (response != null)
            {
                var responseBody = await response.Content.ReadAsStreamAsync();
                var test = await response.Content.ReadAsStringAsync();

                TResponse result;
                if (TryParseResponse(responseBody, ContentType, out result))
                {                   
                    //TODO update arguments
                    var args = new CompletedEventArgs<TResponse>
                    {
                        Response = result,
                        StatusCode = response.StatusCode,
                        IsErrorOccured = false,
                        IsCached = false //TODO set dynamically
                    };

                    //response is valid
                    if (OnValidation(args))
                    {
                        IsBusy = false;
                        OnCompleted(args);
                    }

                    //an error occurred
                    else
                    {
                        IsBusy = false;
                        args.IsErrorOccured = true;
                        OnError(args);
                    }    
                }
                else
                {
                    IsBusy = false;

                    //invoke onCompleted handler
                    if (onCompleted != null) onCompleted();

                    //throw parse exception
                    //var actualContentType = responseBody != null ? responseBody.ContentType : "?";
                    throw new ParseException(ContentType, "?"); //TODO optimize error message
                }
            }
            else
            {
                IsBusy = false;
                //TODO invoke error
            }
        }
     
        /// <summary>
        /// Adds the request into request queue and
        /// executes  
        /// </summary>
        /// <returns></returns>
        public Request<TResponse> Execute()
        {
            if (IsBusy) return this; //TODO throw exception?

            IPriorityQueueHandle<IRequest> handle = null;
            
            //add to queue
            _queue.Enqueue(ref handle, this);

            QueueHandle = handle;

            return this;
        }

        
        public Request<TResponse> Abort()
        {
            if (!IsBusy || _client == null) return this;
            _client.CancelPendingRequests(); //TODO check whether this is the correct method
            IsBusy = false;

            return this;
        } 

        /// <summary>
        /// Tries to parse the response
        /// </summary>
        /// <returns>Whether the response could be parsed</returns>
        private static bool TryParseResponse(Stream responseBody, ContentType format, out TResponse result)
        {
            if (responseBody != null)
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

            result = default(TResponse);
            return false;
        }

        /// <summary>
        /// Compares the priority of this request with the priority of another request.
        /// </summary>
        /// <param name="other">The other request to compare with</param>
        /// <returns>Returns whether the priority of this request is higher than the one of the other request</returns>
        public int CompareTo(IRequest other)
        {
            return Priority.Compare(other.Priority);
        }
    }
}
