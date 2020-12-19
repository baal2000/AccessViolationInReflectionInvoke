# AccessViolationInReflectionInvoke
Repro project demonstrating a fatal CLR error (AccessViolationException) happening intermittently when Reflection is used to Invoke a method that concurently rethrows a shared instance of exception using `ExceptionDispatchInfo.Capture(exception).Throw()`.

Replacing 'ExceptionDispatchInfo.Capture(exception).Throw()' with 'throw exception' or 'throw new Exception("", exception)' avoids the problem.
Also the issue is not present when using BindingFlags.DoNotWrapExceptions.

Framework: .Net Core version 2.1 and above
OS: Windows 64 bit

Issue: https://github.com/dotnet/runtime/issues/45929

Compile the console app and run. It would produce output similar to:

The test started.
Progress:
12/18/2020 8:31:22 PM : 001.1%
12/18/2020 8:31:23 PM : 002.0%
12/18/2020 8:31:24 PM : 003.0%
12/18/2020 8:31:25 PM : 004.0%
12/18/2020 8:31:26 PM : 005.0%
12/18/2020 8:31:27 PM : 006.0%
12/18/2020 8:31:28 PM : 007.0%
12/18/2020 8:31:29 PM : 008.0%
12/18/2020 8:31:32 PM : 009.0%
12/18/2020 8:31:35 PM : 010.0%
12/18/2020 8:31:40 PM : 011.0%
12/18/2020 8:31:44 PM : 012.0%
12/18/2020 8:31:50 PM : 013.0%
12/18/2020 8:31:56 PM : 014.0%
12/18/2020 8:32:03 PM : 015.0%
Fatal error. Internal CLR error. (0x80131506)
   at System.RuntimeMethodHandle.InvokeMethod(System.Object, System.Object[], System.Signature, Boolean, Boolean)
   at CrashTestApp.Program+Test.Invoke()
   at CrashTestApp.Program+Test+<>c__DisplayClass3_0.<Run>b__0(Int32)
   at System.Threading.Tasks.Parallel+<>c__DisplayClass19_0`1[[System.__Canon, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]].<ForWorker>b__1(System.Threading.Tasks.RangeWorker ByRef, Int32, Boolean ByRef)
   at System.Threading.Tasks.TaskReplicator+Replica.Execute()
   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(System.Threading.Thread, System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object)
   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(System.Threading.Tasks.Task ByRef, System.Threading.Thread)
   at System.Threading.ThreadPoolWorkQueue.Dispatch()
   
The above output was produced on Windows 10 8 physical cores x64 laptop.

Because the crash is likely caused by a race condition, it may take time to happen after a random number of the test repetitions. Please be patient.
