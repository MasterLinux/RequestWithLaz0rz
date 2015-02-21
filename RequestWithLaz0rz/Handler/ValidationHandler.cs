namespace RequestWithLaz0rz.Handler
{
    public delegate bool ValidationHandler<TResponse>(
        Request<TResponse> sender, 
        Response<TResponse> args
    );
}
