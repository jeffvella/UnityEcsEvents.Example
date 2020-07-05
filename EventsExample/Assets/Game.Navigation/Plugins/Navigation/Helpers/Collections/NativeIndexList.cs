﻿using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace SimpleBurstCollision
{
    /// <summary>
    ///     Lightweight collection to store a list of indexes to a NativeArray within a job.
    ///     Calling ToList() returns only the subset of values that were flagged with Add(int index) from the original
    ///     NativeArray
    ///     <T>
    ///         ;
    ///         Similar in practice to pre-allocating a NativeList<T> with a length to avoid in-job allocations.
    /// </summary>
    public struct NativeIndexList<T> : IDisposable where T : unmanaged
    {
        private NativeArray<int> _indices;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<T> _referenceArray;

        private bool _disposeSource;

        public NativeIndexList(NativeArray<T> source, Allocator allocator = Allocator.Persistent)
        {
            _referenceArray = source;
            _disposeSource = false;
            // Allocate an extra int of capacity to store a length counter at the start of the array.
            _indices = new NativeArray<int>(source.Length + 1, allocator);
        }

        public NativeIndexList(T[] source, Allocator allocator = Allocator.Persistent)
        {
            _referenceArray = new NativeArray<T>(source, allocator);
            _disposeSource = true;

            // Allocate an extra int of capacity to store a length counter at the start of the array.
            _indices = new NativeArray<int>(source.Length + 1, allocator);
        }

        public void AddIndex(int index)
        {
            _indices[++_indices[0]] = index;
        }

        public int[] IndicesToArray()
        {
            return _indices.ToArray(Length);
        }

        public List<T> ToList()
        {
            var result = new List<T>();
            var maxIndex = _indices[0];
            for (var i = 1; i < maxIndex; i++)
                result.Add(_referenceArray[i]);
            return result;
        }

        public int Length => _indices[0] + 1;

        public void Dispose()
        {
            _indices.Dispose();
            if (_disposeSource)
                _referenceArray.Dispose();
        }
    }
}