namespace RequestWithLaz0rz
{
    public class Parameter
    {
        public Parameter(string key, string value)
        {
            Value = value;
            Key = key;
        }

        public string Key { private set; get; }
        public string Value { private set; get; }
    }
}
