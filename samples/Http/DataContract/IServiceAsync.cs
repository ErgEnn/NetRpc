﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetRpc;

namespace DataContract
{
    [HttpRoute("Service", true)]
    public interface IServiceAsync
    {
        /// <summary>
        /// summary of Call
        /// </summary>
        /// <response code="201">Returns the newly created item</response>
        [HttpRoute("Service1/Call2")]
        Task<CustomObj> CallAsync(string p1, int p2);

        /// <summary>
        /// summary of Call
        /// </summary>
        [FaultException(typeof(CustomException), 400, 1, "errorCode1 error description")]
        [FaultException(typeof(CustomException2), 400, 2, "errorCode2 error description")]
        Task CallByCustomExceptionAsync();

        Task CallByDefaultExceptionAsync();

        Task CallByCancelAsync(CancellationToken token);

        /// <response code="701">return the pain text.</response>
        [ResponseText(701)]
        Task CallByResponseTextExceptionAsync();

        Task<ComplexStream> ComplexCallAsync(CustomObj obj, string p1, Stream stream, Action<CustomCallbackObj> cb, CancellationToken token);
    }
}