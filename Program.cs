using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace baal2000
{
    /// <summary>
    /// This program demonsrates a fatal CLR error (AccessViolationException) in Reflection Invoke.
    /// See README.md for more details.
    /// </summary>
    public class Program
    {
        static void Main(string[] _)
        {
            Console.WriteLine("The test started.");
            Console.WriteLine("Progress:");

            ThreadPool.GetMinThreads(out int _, out int cptMin);
            ThreadPool.SetMinThreads(3500, cptMin);

            Test.Run();

            Console.WriteLine($"Finished successfully, can Press Any Key now and then Try Again");
            Console.ReadKey();
        }

        class Test
        {
            readonly TestCore methods;
            MethodInfo methodInfo;

            public Test()
            {
                methods = new TestCore();
                methodInfo = GetMethod("ExceptionDispatchInfoCaptureThrow");
                if (methodInfo is null)
                {
                    throw new InvalidOperationException("The methodInfo object is missing or empty.");
                }
            }

            public static void Run()
            {
                long progress = 0;
                var test = new Test();
                const int MaxCount = 1000000;
                Parallel.For(
                    0,
                    MaxCount,
                    new ParallelOptions() { MaxDegreeOfParallelism = 3000 },
                    i =>
                    {
                        if (Interlocked.Increment(ref progress) % 10000 == 0)
                        {
                            Console.WriteLine($"{DateTime.Now} : {progress * 100D / MaxCount:000.0}%");
                        }
                        test.Invoke();
                    });
            }

            /// <summary>
            /// Invokes the method using Reflection .
            /// </summary>
            public void Invoke()
            {
                try
                {
                    methodInfo.Invoke(methods, null);

                    // As a workaround, try:
                    // methodInfo.Invoke(methods, BindingFlags.DoNotWrapExceptions, null, null, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    // Ignore
                }
            }

            static MethodInfo GetMethod(string methodName)
            {
                foreach (MethodInfo method in typeof(TestCore).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (methodName == method.Name)
                    {
                        return method;
                    }
                }
                return null;
            }

            class TestCore
            {
                const int ExpirySeconds = 10;

                // An exception instance that gets refreshed every ExpirySeconds.
                (Exception Exception, DateTime CacheExpiryDateTime) exceptionCache;

                /// <summary>
                /// Captures and throws the a cached instance of an exception.
                /// </summary>
                public void ExceptionDispatchInfoCaptureThrow()
                {
                    var error = GetCachedError();

                    // Looks to be not thread-safe when the same error is captures and thrown, then gets wrapped into TargetInvocationException by System.RuntimeMethodHandle.InvokeMethod.
                    ExceptionDispatchInfo.Capture(error).Throw();

                    // As a workaround, try:
                    // throw new Exception("", error);
                }

                Exception GetCachedError()
                {
                    try
                    {
                        var cache = exceptionCache;
                        if (cache.Exception != null)
                        {
                            if (exceptionCache.CacheExpiryDateTime > DateTime.UtcNow)
                            {
                                return cache.Exception;
                            }
                        }
                        throw new Exception("Test");
                    }
                    catch (Exception ex)
                    {
                        exceptionCache = (ex, DateTime.UtcNow.AddSeconds(ExpirySeconds));
                        return ex;
                    }
                }
            }
        }
    }
}
