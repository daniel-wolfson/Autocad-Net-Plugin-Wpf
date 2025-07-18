using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ID.Infrastructure.General
{
    public static class ProcessManager
    {
        private static TaskCompletionSource<string> tcs;

        public static Task<string> RunAsync(string processName, string arguments, Action<string> messageCallBack, Action<string> completeCallBack)
        {
            tcs = new TaskCompletionSource<string>();
            try
            {
                string target = processName.Contains("\\") ? processName : GetRunProcessName(processName);
                if (!File.Exists(target))
                {
                    //tcs.SetResult(processName + " not fount");
                    //tcs.TrySetCanceled();
                    tcs.TrySetResult(processName + " not fount");
                    messageCallBack(processName + " not fount");
                    return tcs.Task;
                }
                if (IsProcessRunning(target))
                {
                    //tcs.SetResult(processName + " already started");
                    //tcs.TrySetCanceled();
                    tcs.TrySetResult(processName + " already started");
                    messageCallBack(processName + " already started");
                    return tcs.Task;
                }

                Process process = new Process
                {
                    StartInfo =
                        {
                            FileName = target,
                            Arguments = arguments,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        },
                    EnableRaisingEvents = true
                };

                process.Exited += (p, args) =>
                {
                    messageCallBack(((Process)p).ProcessName + " not started");
                    //tcs.SetResult(process.ExitCode);
                    //if (!acadApp.DocumentManager.CurrentDocument.UserData.ContainsKey(CommandNames.XIdleOnHubMessage))
                    //    acadApp.DocumentManager.CurrentDocument.UserData.Add(CommandNames.XIdleOnHubMessage, message);
                };
                process.OutputDataReceived += (p, args) =>
                {
                    messageCallBack(((Process)p).ProcessName + " " + args.Data);
                };
                process.ErrorDataReceived += (p, args) =>
                {
                    messageCallBack(((Process)p).ProcessName + " not started");
                };

                bool started = process.Start();
                if (started)
                {
                    Task<bool> t = WaitForStartAsync(process, new TimeSpan(0, 0, 0, 0, 10));
                    t.ContinueWith((task, p) =>
                    {
                        if (task.IsCompleted)
                        {
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();
                            completeCallBack(process.ProcessName + " started");
                            tcs.SetResult(process.ProcessName + " started");
                        }
                        else
                        {
                            tcs.SetResult(process.ProcessName + " not started");
                            messageCallBack(process.ProcessName + " not started");
                        }
                    }, process);
                }
                else
                {
                    tcs.SetResult(processName + " not started");
                }
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        public static void RunAsync1(string processName, string arguments, Action<string> dataReceivedCallBack, Action initCallBack)
        {
            //var tcs = new TaskCompletionSource<int>();
            Task.Run(() =>
            {
                try
                {
                    string target = processName.Contains("\\") ? processName : GetRunProcessName(processName);
                    if (!File.Exists(target))
                    {
                        dataReceivedCallBack(processName + " not fount");
                        //tcs.TrySetCanceled();
                        //return tcs.Task;
                    }

                    if (IsProcessRunning(target))
                    {
                        dataReceivedCallBack(processName + " already started");
                        //tcs.TrySetCanceled();
                        //return tcs.Task;
                    }

                    var process = new Process
                    {
                        StartInfo =
                        {
                            FileName = target,
                            Arguments = arguments,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        },
                        EnableRaisingEvents = true
                    };

                    process.Exited += (s, args) =>
                    {
                        //tcs.SetResult(process.ExitCode);
                    };
                    process.OutputDataReceived += (s, args) => dataReceivedCallBack(args.Data);
                    process.ErrorDataReceived += (s, args) => dataReceivedCallBack("Error: " + args.Data);

                    bool started = process.Start();
                    dataReceivedCallBack?.Invoke((!started ? "Could not start process: " : "started process: ") + process);

                    if (!started)
                        return;

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    initCallBack?.Invoke();

                    var result = process.Id;
                    //tcs.SetResult(result);
                }
                catch (Exception)
                {
                    //ignore
                }
            });
            //return tcs.Task;
        }

        private static Task<bool> WaitForStartAsync(Process process, TimeSpan timeout)
        {
            ManualResetEvent processWaitObject = new ManualResetEvent(false);
            processWaitObject.SafeWaitHandle = new SafeWaitHandle(process.Handle, false);

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            RegisteredWaitHandle registeredProcessWaitHandle = null;
            registeredProcessWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                processWaitObject,
                delegate (object state, bool timedOut)
                {
                    if (!timedOut)
                        registeredProcessWaitHandle.Unregister(null);

                    processWaitObject.Dispose();
                    tcs.SetResult(!timedOut);
                },
                null /* state */,
                timeout,
                true /* executeOnlyOnce */);

            return tcs.Task;
        }

        public static Task<int> RunAsync(Process process, string arguments, Action<string> dataReceivedCallBack, Action initCallBack)
        {
            var tcs = new TaskCompletionSource<int>();
            var target = process.MainModule.FileName;
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = target,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            process.EnableRaisingEvents = true;
            process.Exited += (s1, args1) =>
            {
                tcs.SetResult(process.ExitCode);
            };
            process.OutputDataReceived += (s, args) => dataReceivedCallBack(args.Data);
            process.ErrorDataReceived += (s, args) => dataReceivedCallBack("Error: " + args.Data);

            process.Refresh();

            //string output = process.StandardOutput.ReadToEnd();
            process.BeginOutputReadLine();
            //process.BeginErrorReadLine();
            //Console.WriteLine(output);
            initCallBack?.Invoke();

            return tcs.Task;
        }

        public static bool IsOpenPort(string hostName, int port)
        {
            var tcpListener = default(TcpListener);
            try
            {
                var ipAddress = Dns.GetHostEntry(hostName).AddressList[0]; //var endPoint = new IPEndPoint(IPAddress.Any, port);

                tcpListener = new TcpListener(ipAddress, port);
                tcpListener.Start();

                return false;
            }
            catch (SocketException)
            {
            }
            finally
            {
                tcpListener?.Stop();
            }

            return true;
        }

        public static bool IsProcessRunning(string processName)
        {
            processName = processName.Contains("\\") ? Path.GetFileNameWithoutExtension(processName) : processName;
            return Process.GetProcessesByName(processName).Length > 0;
        }

        public static string GetRunProcessName(string processName)
        {
            var pathName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (pathName == null) return null;
            var path = Path.Combine(pathName.Replace("Win64", ""), "BinHost", processName + ".exe");
            return path;
        }

        public static void ResetTaskCompletionSource()
        {
            tcs = new TaskCompletionSource<string>();
        }
    }
}