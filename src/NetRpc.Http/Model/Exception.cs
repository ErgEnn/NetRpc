﻿using System;
using System.Runtime.Serialization;

namespace NetRpc.Http
{
    internal class HttpNotMatchedException : Exception
    {
        public HttpNotMatchedException(string message) : base(message)
        {
        }
    }

    internal class HttpFailedException : Exception
    {
        public HttpFailedException(string message) : base(message)
        {
        }
    }

    [Serializable]
    public class ResponseTextException : Exception
    {
        public string Text { get; set; }

        public int StatusCode { get; set; }

        public ResponseTextException(string text, int statusCode)
        {
            Text = text;
            StatusCode = statusCode;
        }

        protected ResponseTextException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.SetObjectData(info);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            this.GetObjectData(info);
        }

        public ResponseTextException()
        {
        }
    }
}