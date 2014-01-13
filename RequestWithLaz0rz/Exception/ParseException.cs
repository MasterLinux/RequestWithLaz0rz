using RequestWithLaz0rz.Resources;
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
                return string.Format(
                    Strings.Exception_Parse_Message
                );
            }
        }
    }
}
