// ----------------------------------------------------------------------------
// ObjectPool.cs
// Generic GameObject pool. Used by BoardView, ParticleService, and the FX
// layer. Allocation-free at steady state.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Game.BlockPuzzle.Utils
{
    public sealed class ObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly Stack<GameObject> _available;
        private readonly HashSet<GameObject> _inUse;

        public int CountInactive => _available.Count;
        public int CountActive => _inUse.Count;

        public ObjectPool(GameObject prefab, int initialSize, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            _available = new Stack<GameObject>(initialSize);
            _inUse = new HashSet<GameObject>();
            for (int i = 0; i < initialSize; i++)
            {
                var go = Object.Instantiate(_prefab, _parent);
                go.SetActive(false);
                _available.Push(go);
            }
        }

        public GameObject Get()
        {
            var go = _available.Count > 0 ? _available.Pop() : Object.Instantiate(_prefab, _parent);
            go.SetActive(true);
            _inUse.Add(go);
            return go;
        }

        public void Return(GameObject go)
        {
            if (go == null) return;
            if (!_inUse.Remove(go)) return;
            go.SetActive(false);
            if (_parent != null) go.transform.SetParent(_parent, false);
            _available.Push(go);
        }

        public void Clear()
        {
            foreach (var go in _inUse) if (go != null) Object.Destroy(go);
            foreach (var go in _available) if (go != null) Object.Destroy(go);
            _inUse.Clear();
            _available.Clear();
        }
    }
}
