using System;
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
    public abstract class Request<TResponse> : IRequest
    {
        private readonly Dictionary<string, string> _parameter = new Dictionary<string, string>(); //TODO make parameter public
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>(); //TODO make _headers public
        private readonly Dictionary<string, string> _body = new Dictionary<string, string>(); //TODO make _body public
        private readonly HttpClient _client = new HttpClient(); //TODO move to ExecuteAsync / GetResponseAsync method 

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
        private void OnCompleted(Response<TResponse> args)
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
        private void OnError(Response<TResponse> args)
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
        private bool OnValidation(Response<TResponse> args)
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
        public bool IsExecuting { get; private set; }

        public bool IsAborted { get; private set; } //TODO imlement

        /// <summary>
        /// Tries to build the URL query
        /// </summary>
        /// <param name="paramDict">Dictionary of KeyValuePair</param>
        /// <param name="query">The required query or null if no KeyValuePair added</param>
        /// <returns></returns>
        private static bool TryBuildQuery(IReadOnlyCollection<KeyValuePair<string, string>> paramDict, out string query)  
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

        //TODO rename to GetResponseAsync
        /// <summary>
        /// Executes the request directly without adding
        /// it to the request queue. Use GetResponseAsync
        /// instead of this method. 
        /// </summary>
        /// <seealso cref="GetResponseAsync"/>
        public async Task ExecuteAsync() //TODO use Task<Response<TResponse>> as return type
        {
            if (IsExecuting) return; //TODO throw exception? -> already running
            Response<TResponse> result = null;
            IsExecuting = true;

            //configure client
            _client
                .SetAcceptHeader(Accept)
                .SetUserAgent(UserAgent)
                .AddHeaders(_headers);

            var response = await GetResponseAsync(_client, this);

            if (response != null && response.IsSuccessStatusCode)
            {
                result = await ParseResponseAsync(response, ContentType);
                result = await ValidateResponseAsync(result); //TODO implement as stream?
            }
            else
            {
                IsExecuting = false;
                //TODO invoke error
            }

            IsExecuting = false;

            //return result;
        }

        private async Task<Response<TResponse>> ValidateResponseAsync(Response<TResponse> result)
        {
            /*
            TResponse result;

            if (TryParseResponse(responseBody, ContentType, out result))
            {
                //TODO update arguments
                _response = 

                //response is valid
                if (OnValidation(_response))
                {
                    IsExecuting = false;
                    OnCompleted(_response);
                }

                //an error occurred
                else
                {
                    IsExecuting = false;
                    _response.IsErrorOccured = true;
                    OnError(_response);
                }
            }
            else
            {
                IsExecuting = false;

                //TODO update arguments -> add error message
                _response = new Response<TResponse>(response.Headers)
                {
                    Content = default(TResponse),
                    StatusCode = response.StatusCode,
                    IsErrorOccured = true,
                    IsCached = false
                };

                OnError(_response);
            }*/

            return result;
        }

        private static async Task<Response<TResponse>> ParseResponseAsync(HttpResponseMessage response, ContentType contentType)
        {
            var content = await response.Content.ReadAsStreamAsync();

            return await Task.Run(() =>
            {
                var result = default(TResponse);
                var isErrorOccured = true;
                var isCached = false; //TODO set dynamically

                if(content != null) {
                    try
                    {
                        switch (contentType)
                        {
                            case ContentType.Json:
                                isErrorOccured = new JsonSerializer<TResponse>().TryParse(content, out result);
                                break;

                            case ContentType.Xml:
                                isErrorOccured = new XmlSerializer<TResponse>().TryParse(content, out result);
                                break;

                            case ContentType.Text:
                                isErrorOccured = new TextSerializer<TResponse>().TryParse(content, out result);
                                break;
                        }
                    }
                    catch (System.Exception e)
                    {
                        throw new ParseException(e, contentType);
                    }
                }
            
                return new Response<TResponse>(response.Headers)
                {
                    Content = result,
                    StatusCode = response.StatusCode,
                    IsErrorOccured = isErrorOccured,
                    IsCached = isCached 
                }; ;
            });           
        }

        private async Task<HttpResponseMessage> GetAsync(HttpClient client)
        {
            return await client.GetAsync(Uri);
        }

        private async Task<HttpResponseMessage> DeleteAsync(HttpClient client)
        {
            return await client.DeleteAsync(Uri);
        }

        private async Task<HttpResponseMessage> PostAsync(HttpClient client)
        {
            var content = new FormUrlEncodedContent(_body); //TODO _body == Body == public
            return await client.PostAsync(Uri, content);
        }

        private async Task<HttpResponseMessage> PutAsync(HttpClient client)
        {
            var content = new FormUrlEncodedContent(_body); //TODO _body == Body == public
            return await client.PutAsync(Uri, content);
        }

        // TODO client, request as parameter
        private static async Task<HttpResponseMessage> GetResponseAsync(HttpClient client, Request<TResponse> request)
        {
            HttpResponseMessage response = null;
            FormUrlEncodedContent content;

            switch (request.HttpMethod)
            {
                case HttpMethod.GET:
                    response = await request.GetAsync(client);
                    break;

                case HttpMethod.HEAD:
                    //TODO implement
                    break;

                case HttpMethod.DELETE:
                    response = await request.DeleteAsync(client);
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
                    response = await request.PostAsync(client);
                    break;

                case HttpMethod.PUT:
                    response = await request.PutAsync(client);
                    break;

                default:
                    throw new HttpMethodNotSupportedException(request.HttpMethod);
            }

            return response;
        }

        /// <summary>
        /// Stops the execution of this request
        /// </summary>
        /// <returns>This request</returns>
        public async Task AbortAsync()
        {
            if (!IsExecuting || _client == null) return;
            _client.CancelPendingRequests(); //TODO check whether this is the correct method
            IsExecuting = false;

            //TODO implement this method awaitable (async) 
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
