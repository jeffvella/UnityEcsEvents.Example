/*using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor;
using Debug = UnityEngine.Debug;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Vella.Common
{
    public interface INativeDebuggable
    {
      
    }

    public enum DebugType
    {
        None = 0,
        Log,
    }
    
    public struct NativeDebugLogger
    {
        [NativeSetThreadIndex] public int ThreadIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Log(FixedString128 message) => NativeDebugger.Log(ThreadIndex, message);
    }

    public static class NativeDebugger
    {
        private static int _lastFrameCount;

        [InitializeOnLoadMethod]
        static void OnRuntimeMethodLoad()
        {
            Debug.Log("NativeDebugger.InitializeOnLoadMethod");
            DebugQueue.Allocate(Allocator.Persistent);
            SceneView.duringSceneGui += SceneViewOnDuringSceneGui;
        }

        private static unsafe void SceneViewOnDuringSceneGui(SceneView obj)
        {
            /*if (DebugQueue.IsEmpty)
                return;
            
            if (Time.frameCount == _lastFrameCount)
                return;

            _lastFrameCount = Time.frameCount;

            var queue = DebugQueue.Capture();
            var totalSize = queue.Size();
            var reader = queue.AsReader();
            var start = (byte*) UnsafeUtility.Malloc(totalSize, 4, Allocator.Temp);
            reader.CopyTo(start, totalSize);

            var ptr = start;
            while (ptr - start <= totalSize)
            {
                var header = *(MessageHeader*) ptr;
                ptr += sizeof(MessageHeader);
                
                switch (header.Type)
                {
                    case DebugType.Log:
                        var message = *(LogMessage*)ptr;
                        FormatMessage(message);
                        break;
                }

                ptr += header.DataSize;
            }#1#
        }

        private static void FormatMessage(LogMessage message)
        {
            string str = $"[{message.ThreadId}]: {message.Message}";
            ;
            switch (message.DisplayType)
            {
                case LogDisplayType.None: break;
                case LogDisplayType.Info:
                    Debug.Log(str);
                    break;
                case LogDisplayType.Warning:
                    Debug.LogWarning(str);
                    break;
                case LogDisplayType.Error:
                    Debug.LogError(str);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            return;
        }

        public struct MessageHeader
        {
            public DebugType Type;
            public int DataSize;
        }

        public struct MessageHeader<T>
        {
            public DebugType Type;
            public int DataSize;
            public T Data;
        }
        
        public static void Queue<T>(DebugType type, int threadIndex, T item) where T : struct, INativeDebuggable
        {
            //DebugQueue.Enqueue(threadIndex, new MessageHeader<T> {Type = type, DataSize = UnsafeUtility.SizeOf<T>(), Data = item});
        }

        public static void Log(int threadIndex, FixedString128 text)
        {
            Queue(DebugType.Log, threadIndex, new LogMessage
            {
                DisplayType = LogDisplayType.Info, 
                FromJob = JobsUtility.IsExecutingJob, 
                ThreadId = threadIndex, 
                Message = text
            });
        }

        public static void LogWarning(int threadIndex, FixedString128 text)
        {
            Queue(DebugType.Log, threadIndex, new LogMessage
            {
                Type = DebugType.Log, 
                FromJob = JobsUtility.IsExecutingJob, 
                DisplayType = LogDisplayType.Warning, 
                Message = text
            });
        }

        public static void LogError(int threadIndex, FixedString128 text)
        {
            Queue(DebugType.Log, threadIndex, new LogMessage
            {
                Type = DebugType.Log, 
                FromJob = JobsUtility.IsExecutingJob, 
                DisplayType = LogDisplayType.Error, 
                Message = text
            });
        }

        public struct LogMessage : INativeDebuggable
        {
            public DebugType Type;
            public FixedString128 Message;
            public LogDisplayType DisplayType;
            public int ThreadId;
            public bool FromJob;
        }

        public enum LogDisplayType
        {
            None = 0,
            Info,
            Warning,
            Error,
        }

        public static class DebugQueue
        {
            private static readonly SharedStatic<Container> SharedData = SharedStatic<Container>.GetOrCreate<Key>();

            private class Key
            {
            }

            public struct Container
            {
                public bool IsCreated;
                public MultiAppendBuffer ActiveQueue;
                public MultiAppendBuffer InactiveQueue;
            }

            public static ref MultiAppendBuffer Capture()
            {
                if (!IsCreated)
                    throw new InvalidOperationException("Container has not been created; call Allocate() first.");

                InactiveQueue.Clear();
                var tmp = ActiveQueue;
                ActiveQueue = InactiveQueue;
                InactiveQueue = tmp;
                return ref InactiveQueue;
            }

            private static ref MultiAppendBuffer ActiveQueue => ref SharedData.Data.ActiveQueue;

            private static ref MultiAppendBuffer InactiveQueue => ref SharedData.Data.InactiveQueue;

            public static bool IsCreated => SharedData.Data.IsCreated;

            public static bool IsEmpty => SharedData.Data.ActiveQueue.IsEmpty;

            public static bool Allocate(Allocator allocator)
            {
                if (SharedData.Data.IsCreated)
                    return false;

                SharedData.Data.IsCreated = true;
                SharedData.Data.ActiveQueue = new MultiAppendBuffer(allocator);
                SharedData.Data.InactiveQueue = new MultiAppendBuffer(allocator);
                return true;
            }

            public static bool Dispose()
            {
                if (!SharedData.Data.IsCreated)
                    return false;

                SharedData.Data.IsCreated = false;
                SharedData.Data.ActiveQueue.Dispose();
                SharedData.Data.InactiveQueue.Dispose();
                return true;
            }

            public static bool Clear()
            {
                if (!SharedData.Data.IsCreated)
                    return false;

                SharedData.Data.ActiveQueue.Clear();
                SharedData.Data.InactiveQueue.Clear();
                return true;
            }

            public static void Enqueue<T>(int threadIndex, T value) where T : struct
            {
                if (!SharedData.Data.IsCreated)
                    Allocate(Allocator.Persistent);

                SharedData.Data.ActiveQueue.Enqueue(threadIndex, value);
            }

        }

        /// <summary>
        /// A collection of <see cref="UnsafeAppendBuffer"/> intended to allow one buffer per thread.
        /// </summary>
        [DebuggerDisplay("IsEmpty={IsEmpty}")]
        public unsafe struct MultiAppendBuffer
        {
            public const int DefaultThreadIndex = -1;
            public const int MaxThreadIndex = JobsUtility.MaxJobThreadCount - 1;
            public const int MinThreadIndex = DefaultThreadIndex;

            [NativeDisableUnsafePtrRestriction] public UnsafeAppendBuffer* Ptr;

            internal byte* BaseAddress => (byte*) Ptr - sizeof(Allocator);
            public Allocator Allocator => *(Allocator*) BaseAddress;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsInvalidThreadIndex(int index) => index < MinThreadIndex || index > MaxThreadIndex;

            public bool IsEmpty => Size() == 0;

            public MultiAppendBuffer(Allocator allocator, int initialCapacityPerThread = JobsUtility.CacheLineSize)
            {
                var bufferSize = UnsafeUtility.SizeOf<UnsafeAppendBuffer>();
                var bufferCount = JobsUtility.MaxJobThreadCount + 1;
                var allocationSize = bufferSize * bufferCount;
                var initialBufferCapacityBytes = initialCapacityPerThread;

                var ptr = (byte*) UnsafeUtility.Malloc(allocationSize, UnsafeUtility.AlignOf<int>(), allocator);
                UnsafeUtility.MemClear(ptr, allocationSize);
                UnsafeUtility.CopyStructureToPtr(ref allocator, ptr);

                var dataStartPtr = ptr + sizeof(Allocator);
                for (int i = 0; i < bufferCount; i++)
                {
                    var bufferPtr = (UnsafeAppendBuffer*) (dataStartPtr + bufferSize * i);
                    var buffer = new UnsafeAppendBuffer(initialBufferCapacityBytes, UnsafeUtility.AlignOf<int>(), allocator);
                    UnsafeUtility.CopyStructureToPtr(ref buffer, bufferPtr);
                }

                Ptr = (UnsafeAppendBuffer*) dataStartPtr;
            }

            /// <summary>
            /// Adds data to the collection.
            /// </summary>
            /// <typeparam name="T">the type of the item being added</typeparam>
            /// <param name="threadIndex">the currently used thread index (or -1 for a shared channel)</param>
            /// <param name="item">the item to be added</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Enqueue<T>(int threadIndex, T item) where T : struct
            {
                GetBuffer(threadIndex).Add(item);
            }

            /// <summary>
            /// Retrieve buffer for a specific thread index.
            /// </summary>
            /// <param name="threadIndex"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref UnsafeAppendBuffer GetBuffer(int threadIndex)
            {
                // All indexes are offset by +1; Unspecified ThreadIndex 
                // (main thread without explicitly checking for ThreadId) 
                // should use first index by providing threadIndex of -1;

                return ref UnsafeUtilityEx.ArrayElementAsRef<UnsafeAppendBuffer>(Ptr, threadIndex + 1);
            }

            /// <summary>
            /// Calculates the current total size of data that has been added.
            /// </summary>
            public int Size()
            {
                var totalSize = 0;
                for (int i = -1; i < JobsUtility.MaxJobThreadCount; i++)
                {
                    totalSize += GetBuffer(i).Length;
                }

                return totalSize;
            }

            public Reader AsReader()
            {
                Reader reader;
                reader.Data = this;
                reader.WrittenTotal = 0;
                reader.WrittenFromIndex = 0;
                reader.Index = DefaultThreadIndex;
                return reader;
            }

            /// <summary>
            /// A reader instance lets you keep track of the current read position and therefore easily
            /// copy data to different destinations (e.g. chunks); each time continuing from where it left off.
            /// </summary>
            public struct Reader
            {
                public MultiAppendBuffer Data;
                public int WrittenTotal;
                public int WrittenFromIndex;
                public int Index;

                public T PeekStart<T>() where T : unmanaged
                {
                    for (; Index < JobsUtility.MaxJobThreadCount; Index++)
                    {
                        ref var buffer = ref Data.GetBuffer(Index);
                        if (buffer.Length > 0)
                        {
                            return UnsafeUtility.AsRef<T>(buffer.Ptr);
                        }
                    }

                    return default;
                }

                /// <summary>
                /// Copies from the pool of data remaining to be read, to the provided destination.
                /// </summary>
                /// <param name="destinationPtr">where to write the data</param>
                /// <param name="maxSizeBytes">the maximum amount of data that can be written to <paramref name="destinationPtr"/> (in bytes)</param>
                /// <returns></returns>
                public int CopyTo(void* destinationPtr, int maxSizeBytes)
                {
                    if (destinationPtr == null)
                        throw new NullReferenceException();

                    byte* pos = (byte*) destinationPtr;
                    int bytesWritten = 0;

                    for (; Index < JobsUtility.MaxJobThreadCount; Index++)
                    {
                        ref var buffer = ref Data.GetBuffer(Index);
                        if (buffer.Length > 0)
                        {
                            var amountToWrite = math.min(maxSizeBytes, buffer.Length);

                            bytesWritten += amountToWrite;
                            if (bytesWritten > maxSizeBytes)
                                throw new Exception("Attempt to write data beyond the target allocation");

                            UnsafeUtility.MemCpy(pos, buffer.Ptr + WrittenFromIndex, amountToWrite);

                            pos += amountToWrite;

                            WrittenTotal += amountToWrite;
                            WrittenFromIndex += amountToWrite;

                            if (WrittenFromIndex >= buffer.Length)
                            {
                                WrittenFromIndex = 0;
                            }

                            if (maxSizeBytes <= buffer.Length)
                            {
                                return bytesWritten;
                            }
                        }
                    }

                    return bytesWritten;
                }
            }

            public void Dispose()
            {
                for (int i = -1; i < JobsUtility.MaxJobThreadCount; i++)
                {
                    GetBuffer(i).Dispose();
                }

                UnsafeUtility.Free(BaseAddress, Allocator);
            }

            public void Clear()
            {
                for (int i = -1; i < JobsUtility.MaxJobThreadCount; i++)
                {
                    GetBuffer(i).Reset();
                }
            }
        }

    }
}*/

