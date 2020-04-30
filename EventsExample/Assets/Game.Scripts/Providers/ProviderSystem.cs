// ReSharper disable ClassNeverInstantiated.Global

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;


namespace Assets.Scripts.Providers
{
    public interface INativeProvider
    {
        void Allocate(SystemBase system, Allocator allocator);

        void Dispose();
    }

    public interface IProvider : IDisposable { }

    public unsafe interface IProvider<T> where T : struct
    {
        Native<T> Data { get; }
    }
    
    public readonly unsafe struct Unmanaged<T> where T : unmanaged
    {
        public readonly T* Ptr;
        public Unmanaged(Allocator allocator) => Ptr = (T*)UnsafeUtility.Malloc(sizeof(T), UnsafeUtility.AlignOf<T>(), allocator);
        public void Dispose(Allocator allocator) => UnsafeUtility.Free(Ptr, allocator);
        public void Clear() => UnsafeUtility.MemClear(Ptr, UnsafeUtility.SizeOf<T>());
        public ref T Ref => ref UnsafeUtilityEx.AsRef<T>(Ptr);
        public static implicit operator Native<T>(Unmanaged<T> o) => UnsafeUtilityEx.AsRef<Native<T>>(o.Ptr);
    }

    public readonly unsafe struct Native<T> where T : struct
    {
        public readonly byte* Ptr;
        public Native(Allocator allocator) => Ptr = (byte*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), allocator);
        public void Dispose(Allocator allocator) => UnsafeUtility.Free(Ptr, allocator);
        public void Clear() => UnsafeUtility.MemClear(Ptr, UnsafeUtility.SizeOf<T>());
        public ref T Ref => ref UnsafeUtilityEx.AsRef<T>(Ptr);
        public ref T1 As<T1>() where T1 : struct => ref UnsafeUtilityEx.AsRef<T1>(Ptr);
    }
    
    public unsafe class Provider<T> : IProvider<T>, IProvider where T : struct, INativeProvider
    {
        /// <summary>
        /// Access to the struct <typeparamref name="T"/>; Assign to local variable to pass into Entities.ForEach lambda.
        /// </summary>
        public Native<T> Data { get; private set; }

        private Allocator _allocator;

        public void Allocate(SystemBase system, Allocator allocator)
        {
            _allocator = allocator;
            Data = new Native<T>(_allocator);
            Data.Clear();
            Data.Ref.Allocate(system, allocator);
        }

        public void Dispose() => Data.Dispose(_allocator);
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public sealed class ProviderSystem : SystemBase
    {
        public Dictionary<int, IProvider> Providers = new Dictionary<int, IProvider>(); 

        protected override void OnCreate() => Enabled = false;

        protected override void OnUpdate() => throw new NotImplementedException();

        protected override void OnDestroy()
        {
            foreach(var pair in Providers)
            {
                pair.Value.Dispose();
            }
            Providers = null;
        }
    }

    public static unsafe class ProviderExtensions
    {
        public static IProvider<T> GetOrCreateProvider<T>(this World world) where T : struct, INativeProvider
        {
            var key = typeof(T).GetHashCode();
            var providerSystem = world.GetOrCreateSystem<ProviderSystem>();
            if (providerSystem.Providers.TryGetValue(key, out var provider))
                return (IProvider<T>) provider;
            
            var newProvider = new Provider<T>();
            newProvider.Allocate(providerSystem, Allocator.Persistent);
            providerSystem.Providers.Add(key, newProvider);
            return newProvider;
        }
    }
}
