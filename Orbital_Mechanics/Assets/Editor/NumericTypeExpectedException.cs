using System;

namespace Exceptions
{
    /// <summary>
    /// An exception that is thrown whenever a numeric type is expected as an input somewhere but the input wasn't numeric.
    /// </summary>
    [Serializable]
    public class NumericTypeExpectedException : Exception
    {
        public NumericTypeExpectedException() { }

        public NumericTypeExpectedException(string message) : base(message) { }

        public NumericTypeExpectedException(string message, Exception inner) : base(message, inner) { }

        protected NumericTypeExpectedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// An exception that is thrown whenever a property was not found inside of an object when using Reflection.
    /// </summary>
    [Serializable]
    public class PropertyNotFoundException : Exception
    {
        public PropertyNotFoundException() { }

        public PropertyNotFoundException(string message) : base(message) { }

        public PropertyNotFoundException(string message, Exception inner) : base(message, inner) { }

        protected PropertyNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// An exception that is thrown whenever a field or a property was not found inside of an object when using Reflection.
    /// </summary>
    [Serializable]
    public class PropertyOrFieldNotFoundException : Exception
    {
        public PropertyOrFieldNotFoundException() { }

        public PropertyOrFieldNotFoundException(string message) : base(message) { }

        public PropertyOrFieldNotFoundException(string message, Exception inner) : base(message, inner) { }

        protected PropertyOrFieldNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// An exception that is thrown whenever a field was not found inside of an object when using Reflection.
    /// </summary>
    [Serializable]
    public class FieldNotFoundException : Exception
    {
        public FieldNotFoundException() { }

        public FieldNotFoundException(string message) : base(message) { }

        public FieldNotFoundException(string message, Exception inner) : base(message, inner) { }

        protected FieldNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}