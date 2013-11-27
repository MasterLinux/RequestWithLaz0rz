using RequestWithLaz0rz.Resources;
using RequestWithLaz0rz.Type;

namespace RequestWithLaz0rz.Exception
{
    public class ParseException : System.Exception
    {
        private readonly ContentType _expected;
        private readonly string _actual;

        public ParseException(ContentType expected, string actual)
        {
            _expected = expected;
            _actual = actual;
        }

        public override string Message
        {
            get
            {
                return string.Format(
                    Strings.Exception_Parse_Message, 
                    _expected, 
                    _actual
                );
            }
        }
    }
}
