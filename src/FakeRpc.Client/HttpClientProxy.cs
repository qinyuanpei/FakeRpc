using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Threading;
using FakeRpc.Core.Mvc;
using System.Linq;
using FakeRpc.Core;
using FakeRpc.Core.Mics;
using FakeRpc.Core.Serialize;
using FakeRpc.Core.Binder.Http;

namespace FakeRpc.Client
{
    public class HttpClientProxy<T> : DispatchProxy
    {
        public HttpClient HttpClient { get; set; }

        public IMessageSerializer Serializer { get; set; }

        public IHtttpCallInvoker CallInvoker { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var baseUrl = HttpClient.BaseAddress.ToString();
            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            var serviceRoute = typeof(T).GetServiceRoute(targetMethod.Name);

            var serviceUrl = $"{baseUrl}{serviceRoute}";

            if (args.Length == 1)
            {
                // Unary Request Call
                var requestType = args[0].GetType();
                var responseType = targetMethod.ReturnType.GenericTypeArguments[0];
                dynamic result = CallUnaryRequest(CallInvoker, requestType, responseType, new Uri(serviceUrl), args[0]);
                return Task.FromResult(result.Result);
            }
            else if (args.Length == 0)
            {
                // Empty Request Call
                var responseType = targetMethod.ReturnType.GenericTypeArguments[0];
                dynamic result = CallEmptyRequest(CallInvoker, responseType, new Uri(serviceUrl));
                return Task.FromResult(result.Result);
            }

            throw new Exception("FakeRpc only support a RPC method with 0 or 1 parameter");
        }

        private dynamic CallUnaryRequest(IHtttpCallInvoker callInvoker, Type requestType, Type responseType, Uri uri, object request)
        {
            // IHtttpCallInvoker.CallAsync<TRequest,TResponse>(uri, request);
            var callMethod = Expression.Call(
                Expression.Constant(callInvoker),
                callInvoker.GetType().GetMethods().ToList().First(x => x.Name == "CallAsync" && x.GetParameters().Length == 2).MakeGenericMethod(requestType, responseType),
                new Expression[]
                {
                    Expression.Constant(uri, uri.GetType()),
                    Expression.Constant(request,request.GetType()),
                }
            );

            // () => IFakeRpcCalls.CallAsync<TRequest, TResponse>(uri, request);
            var lambdaExp = Expression.Lambda(callMethod, null);

            var caller = lambdaExp.Compile();
            return caller.DynamicInvoke();
        }

        private dynamic CallEmptyRequest(IHtttpCallInvoker callInvoker, Type responseType, Uri uri)
        {
            // IHtttpCallInvoker.CallAsync<TResponse>(uri);
            var callMethod = Expression.Call(
                Expression.Constant(callInvoker),
                callInvoker.GetType().GetMethods().ToList().First(x => x.Name == "CallAsync" && x.GetParameters().Length == 1).MakeGenericMethod(responseType),
                new Expression[]
                {
                    Expression.Constant(uri, uri.GetType()),
                }
            );

            // () => IHtttpCallInvoker.CallAsync<TResponse>(uri);
            var lambdaExp = Expression.Lambda(callMethod, null);

            var caller = lambdaExp.Compile();
            return caller.DynamicInvoke();
        }
    }
}
