using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Providers
{
    public interface INativeProvider
    {
        void Allocate(SystemBase system, Allocator allocator);

        void Dispose();
    }

    public interface IProvider : IDisposable
    {
    }

    public unsafe interface IProvider<T>
    {
        void* GetUnsafePtr();

        ref T Data { get; }
    }

    public unsafe class Provider<T> : IProvider<T>, IProvider where T : struct, INativeProvider 
    {
        public byte* Ptr;

        public ref T Data => ref UnsafeUtilityEx.AsRef<T>(Ptr);

        // Regarding Memory Leak Exceptions:
        // <T> will go out of managed scope, because there is no class holding a reference.
        // That means any NativeContainers with a DisposeSentinel Safety will also be 
        // out of scope and then garbage collected, triggering an incorrect error in the 
        // DOTS leak detection systems. You can however use Unsafe versions of collections
        // to avoid producing these errors.

        public void Allocate(SystemBase system, Allocator allocator)
        {
            Ptr = (byte*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
            Data.Allocate(system, allocator);
        }

        public void Dispose()
        {
            Data.Dispose();
            UnsafeUtility.Free(Ptr, Allocator.Persistent);
        }

        public void* GetUnsafePtr() => Ptr;
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public unsafe class ProviderSystem : SystemBase
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

    public unsafe static class ProviderExtensions
    {
        public static IProvider<T> GetOrCreateProvider<T>(this World world) where T : struct, INativeProvider
        {
            var key = typeof(T).GetHashCode();
            var providerSystem = world.GetOrCreateSystem<ProviderSystem>();
            if (!providerSystem.Providers.TryGetValue(key, out IProvider provider))
            {
                var newProvider = new Provider<T>();
                newProvider.Allocate(providerSystem, Allocator.Persistent);
                providerSystem.Providers.Add(key, newProvider);
                return newProvider;
            }
            return (IProvider<T>)provider;
        }
    }
}
