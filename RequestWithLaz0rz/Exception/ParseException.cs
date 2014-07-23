using RequestWithLaz0rz.Type;

namespace RequestWithLaz0rz.Exception
{
    public class ParseException : System.Exception
    {
        public ParseException(System.Exception innerException)
            : base(null, innerException)
        {

        }

        public override string Message
        {
            get
            {
                return "Unable to parse"; //TODO optimize exception message
            }
        }
    }
}
