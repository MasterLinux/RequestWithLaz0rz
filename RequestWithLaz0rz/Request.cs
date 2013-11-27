using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using RequestWithLaz0rz.Exception;
using RequestWithLaz0rz.Extension;
using RequestWithLaz0rz.Handler;
using RequestWithLaz0rz.Serializer;
using RequestWithLaz0rz.Type;

namespace RequestWithLaz0rz
{
    public abstract class Request<TResponse>    
    {
        private readonly Dictionary<string, string> _parameter = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _header = new Dictionary<string, string>();
        private HttpWebRequest _request;

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
        /// <param name="errorMessage">The error message of the occured error or null if no error</param>
        /// <returns>true if response is valid and no error occured, false otherwise</returns>
        private bool OnValidation(CompletedEventArgs<TResponse> args, out string errorMessage)
        {
            var handler = Validation;
            errorMessage = null;
            return handler == null || handler(this, args, out errorMessage);
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
        private static bool TryBuildQuery(IReadOnlyCollection<KeyValuePair<string, string>> paramDict, out string query)  
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
            if (_header.ContainsKey(header.Key))
            {
                _header.Remove(header.Key);
            }

            _header.Add(header.Key, header.Value);
            return this;
        }

        

        public Request<TResponse> Start()
        {
            if (IsBusy) return this;
            IsBusy = true;

            //create request
            _request = WebRequest.Create(Uri) as HttpWebRequest;
           
            if (_request != null)
            {
                //TODO check whether the request is updated
                //configure request
                _request
                    .AddHeader(_header)
                    .SetMethod(HttpMethod);

                //start request
                _request.BeginGetResponse(result =>
                {
                    HttpStatusCode statusCode;
                    TResponse response;

                    if (TryParseResponse(result, ContentType, out statusCode, out response))
                    {
                        string errorMessage;

                        //TODO update arguments
                        var args = new CompletedEventArgs<TResponse>
                        {
                            Response = response,
                            StatusCode = statusCode,
                            IsErrorOccured = false,
                            IsCached = false //TODO set dynamically
                        };

                        //response is valid
                        if (OnValidation(args, out errorMessage))
                        {
                            OnCompleted(args);
                        }
                        else
                        {
                            //set error
                            args.ErrorMessage = errorMessage;
                            args.IsErrorOccured = true;

                            OnError(args);
                        }
                    }
                    else
                    {
                        IsBusy = false;
                        throw new ParseException(ContentType); //TODO pass content type of response
                        //TODO throw error
                    }

                    IsBusy = false;

                }, _request);
            }
            else
            {
                //TODO throw error
                IsBusy = false;
            }

            return this;
        }

        

        public Request<TResponse> Abort()
        {
            if (!IsBusy || _request == null) return this;
            _request.Abort();
            IsBusy = false;

            return this;
        } 

        /// <summary>
        /// Tries to parse the response
        /// </summary>
        /// <returns>Whether the response could be parsed</returns>
        private static bool TryParseResponse(IAsyncResult result, ContentType format, out HttpStatusCode statusCode, out TResponse response)
        {
            var request = result.AsyncState as HttpWebRequest;

            if (request != null)
            {
                try
                {
                    var webResponse = request.EndGetResponse(result) as HttpWebResponse;

                    if (webResponse != null)
                    {
                        statusCode = webResponse.StatusCode;

                        switch (format)
                        {
                            case ContentType.Json:
                                return new JsonSerializer<TResponse>().TryParse(webResponse, out response);

                            case ContentType.Xml:
                                return new XmlSerializer<TResponse>().TryParse(webResponse, out response);

                            case ContentType.Text:
                                return new TextSerializer<TResponse>().TryParse(webResponse, out response);

                        }
                    }
                }
                catch (WebException exception)
                {
                    var webResponse = exception.Response as HttpWebResponse;

                    if(webResponse != null) {
                        statusCode = webResponse.StatusCode;
                        //response = 
                        //TODO parse response and throw error
                    }
                }
            }

            statusCode = HttpStatusCode.NoContent;
            response = default(TResponse);
            return false;
        }
    }
}
