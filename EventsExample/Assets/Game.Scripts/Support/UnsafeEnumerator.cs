using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Assets.Scripts.Support
{
    public unsafe ref struct UnsafeEnumerator<T> where T : unmanaged
    {
        public byte* _ptr;
        public int _index;
        public int _length;

        public bool MoveNext() => _index++ < _length;

        public void Reset() => _index = -1;

        public ref T Current => ref *(T*)(_ptr + sizeof(T) * _index);
    }

    public unsafe ref struct UnsafeEnumerator<K, V> where K : unmanaged where V : unmanaged
    {
        public UnsafePair _current;
        public int _length;

        public UnsafeEnumerator(NativeKeyValueArrays<K, V> pairs)
        {
            _length = pairs.Keys.Length;
            _current = new UnsafePair
            {
                _keysPtr = (byte*)pairs.Keys.GetUnsafePtr(),
                _valuesPtr = (byte*)pairs.Values.GetUnsafePtr(),
                _index = -1
            };
        }

        public static void CreateReinterpret<KI, VI>(NativeKeyValueArrays<KI, VI> pairs)  where KI : unmanaged where VI : unmanaged
        {
            UnsafeEnumerator<K, V> enu;
            enu._length = pairs.Keys.Length;
            enu._current = new UnsafePair
            {
                _keysPtr = (byte*)pairs.Keys.GetUnsafePtr(),
                _valuesPtr = (byte*)pairs.Values.GetUnsafePtr(),
                _index = -1
            };
        }

        public unsafe ref struct UnsafePair
        {
            internal byte* _keysPtr;
            internal byte* _valuesPtr;
            internal int _index;

            public ref K Key => ref *(K*)(_keysPtr + sizeof(K) * _index);

            public ref V Value => ref *(V*)(_valuesPtr + sizeof(V) * _index);
        }

        public bool MoveNext() => ++_current._index < _length;

        public void Reset() => _current._index = -1;

        public UnsafePair Current => _current;
    }
}
