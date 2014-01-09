namespace RequestWithLaz0rz
{
    public class Header
    {
        public Header(string key, string value)
        {
            Value = value;
            Key = key;
        }

        public string Key { private set; get; }
        public string Value { private set; get; }
    }
}
