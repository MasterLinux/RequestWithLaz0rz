namespace RequestWithLaz0rz.Data
{
    public interface IValidationTask<TResponse>
    {
        Response<TResponse> Validate(Response<TResponse> response);
    }
}