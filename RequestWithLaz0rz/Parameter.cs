using System;

namespace RequestWithLaz0rz
{
    public class Parameter
    {
        /// <summary>
        /// Initializes the parameter with a string value
        /// </summary>
        /// <param name="key">The key of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        public Parameter(string key, string value)
        {
            Value = value;
            Key = key;
        }

        /// <summary>
        /// Initializes the parameter with an integer value
        /// </summary>
        /// <param name="key">The key of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        public Parameter(string key, int value) 
            : this(key, Convert.ToString(value))
        {
            //does nothing
        }

        /// <summary>
        /// Initializes the parameter with a boolean value
        /// </summary>
        /// <param name="key">The key of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        public Parameter(string key, bool value)
            : this(key, Convert.ToString(value))
        {
            //does nothing
        }

        /// <summary>
        /// Gets the key of the parameter
        /// </summary>
        public string Key { private set; get; }

        /// <summary>
        /// Gets the value of the parameter
        /// </summary>
        public string Value { private set; get; }
    }
}
