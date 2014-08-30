using System;
using System.Threading.Tasks;
using RequestWithLaz0rz.Handler;
using RequestWithLaz0rz.Type;

namespace RequestWithLaz0rz
{
    /// <summary>
    /// Interface for requests with a priority
    /// </summary>
    public interface IPriorityRequest : IComparable<IPriorityRequest>
    {
        /// <summary>
        /// Gets the execution priority. A request with a higher
        /// priority will be executed before a request
        /// with a lower one.
        /// </summary>
        RequestPriority Priority { get; }

        /// <summary>
        /// Executes the request
        /// </summary>
        Task RunAsync();

        /// <summary>
        /// Stops the execution of this request.
        /// </summary>
        /// <returns>This request</returns>
        void Abort();
    }

    /// <summary>
    /// Interface for requests
    /// </summary>
    public interface IRequest<TResponse> : IPriorityRequest
    {

        #region Events

        /// <summary>
        /// Event which is invoked whenever the request is
        /// successfully executed. 
        /// </summary>
        event CompletedHandler<TResponse> Completed;

        /// <summary>
        /// Event which is invoked whenever an error occured
        /// during the request executing.
        /// </summary>
        event CompletedHandler<TResponse> Error;

        /// <summary>
        /// Event which is invoked when the validation process
        /// starts.
        /// </summary>
        event ValidationHandler<TResponse> Validation;

        #endregion


        #region Properties      

        /// <summary>
        /// Gets the base URI of this request
        /// </summary>
        string BaseUri { get; }

        /// <summary>
        /// Gets the path of this request
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the accept header of this request
        /// </summary>
        string Accept { get; }

        /// <summary>
        /// Gets the user-agent of this request
        /// </summary>
        string UserAgent { get; }

        /// <summary>
        /// Gets the expected content type of the response 
        /// </summary>
        ContentType ContentType { get; }

        /// <summary>
        /// Gets the HTTP method of this request
        /// </summary>
        HttpMethod HttpMethod { get; }

        /// <summary>
        /// Flag which indicates whether the 
        /// request is currently executing.
        /// </summary>
        bool IsBusy { get; }

        #endregion


        /// <summary>
        /// Adds a new parameter. An already existing 
        /// parameter will be overridden.
        /// </summary>
        /// <param name="keyValuePair">The KeyValuePair to add</param>
        /// <returns>This request</returns>
        Request<TResponse> AddParameter(KeyValuePair keyValuePair);

        /// <summary>
        /// Adds a new header. An already existing 
        /// header will be overridden.
        /// </summary>
        /// <param name="header">The header to add</param>
        /// <returns>This request</returns>
        Request<TResponse> AddHeader(KeyValuePair header);

        /// <summary>
        /// Adds a key value pair to the body. If this method
        /// is used the body is added as FormUrlEncodedContent
        /// </summary>
        /// <param name="body">The body to add</param>
        /// <returns>This request</returns>
        Request<TResponse> AddBody(KeyValuePair body);


        #region Execution 

        /// <summary>
        /// Adds the request into request queue and
        /// executes the request asynchroniously.
        /// </summary>
        /// <returns>The response</returns>
        Task<CompletedEventArgs<TResponse>> GetResponseAsync();       

        #endregion
    }
}
