using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using RequestWithLaz0rz.Serializer;

namespace RequestWithLaz0rz
{
    public abstract class Request<TResponse>    
    {
        private readonly Dictionary<string, string> _parameter = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _header = new Dictionary<string, string>();
        private HttpWebRequest _request;

        protected abstract string BaseUri
        {
            get;
        }

        protected abstract string Path
        {
            get;
        }

        protected abstract ResponseFormat Format
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
            throw new NotImplementedException();
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
            if (paramDict.Count == 0)
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
            
            AddHeader(ref _request);          

            if (_request != null)
            {
                _request.BeginGetResponse(result =>
                {
                    TResponse response;

                    if (TryParseResponse(result, Format, out response))
                    {
                        //TODO handle events
                    }
                    else
                    {
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

        private void AddHeader(ref HttpWebRequest request)
        {
            if(_header.Count == 0) return;

            foreach (var header in _header)
            {
                request.Headers[header.Key] = header.Value;
            }
        }

        /// <summary>
        /// Tries to parse the response
        /// </summary>
        /// <returns>Whether the response could be parsed</returns>
        private static bool TryParseResponse(IAsyncResult result, ResponseFormat format, out TResponse response)
        {
            var request = result.AsyncState as HttpWebRequest;

            if (request != null)
            {
                try
                {
                    var res = request.EndGetResponse(result);

                    switch (format)
                    {
                        case ResponseFormat.JSON:
                            return new JsonSerializer<TResponse>().TryParse(res, out response);
         
                        case ResponseFormat.XML:
                            return new XmlSerializer<TResponse>().TryParse(res, out response);

                        case ResponseFormat.Text:
                            return new TextSerializer<TResponse>().TryParse(res, out response);

                    }
                }
                catch (WebException exception)
                {
                    //TODO throw error
                }
            }

            response = default(TResponse);
            return false;
        }
    }
}
