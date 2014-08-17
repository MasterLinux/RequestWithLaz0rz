using System;

namespace RequestWithLaz0rz
{
    public class KeyValuePair
    {
        /// <summary>
        /// Initializes the KeyValuePair with a string value
        /// </summary>
        /// <param name="key">The key of the KeyValuePair</param>
        /// <param name="value">The value of the KeyValuePair</param>
        public KeyValuePair(string key, string value)
        {
            Value = value;
            Key = key;
        }

        /// <summary>
        /// Initializes the KeyValuePair with an integer value
        /// </summary>
        /// <param name="key">The key of the KeyValuePair</param>
        /// <param name="value">The value of the KeyValuePair</param>
        public KeyValuePair(string key, int value) 
            : this(key, Convert.ToString(value))
        {
            //does nothing
        }

        /// <summary>
        /// Initializes the KeyValuePair with a boolean value
        /// </summary>
        /// <param name="key">The key of the KeyValuePair</param>
        /// <param name="value">The value of the KeyValuePair</param>
        public KeyValuePair(string key, bool value)
            : this(key, Convert.ToString(value))
        {
            //does nothing
        }

        /// <summary>
        /// Gets the key of the KeyValuePair
        /// </summary>
        public string Key { private set; get; }

        /// <summary>
        /// Gets the value of the KeyValuePair
        /// </summary>
        public string Value { private set; get; }
    }
}
