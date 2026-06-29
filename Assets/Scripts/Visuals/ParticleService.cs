// ----------------------------------------------------------------------------
// ParticleService.cs
// Spawns particle bursts at board coordinates. Particles come from a small
// pool of ParticleSystems attached to a single "FX" GameObject; we re-parent
// + re-play rather than Instantiate / Destroy to stay allocation-free.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Game.BlockPuzzle.Board;
using UnityEngine;

namespace Game.BlockPuzzle.Visuals
{
    public sealed class ParticleService : MonoBehaviour, IService
    {
        [SerializeField] private ParticleSystem _lineClearPrefab;
        [SerializeField] private ParticleSystem _bombPrefab;
        [SerializeField] private ParticleSystem _rainbowPrefab;
        [SerializeField] private ParticleSystem _coinPrefab;

        private readonly Queue<ParticleSystem> _lineClearPool = new();
        private readonly Queue<ParticleSystem> _bombPool = new();
        private readonly Queue<ParticleSystem> _rainbowPool = new();
        private readonly Queue<ParticleSystem> _coinPool = new();

        public void Initialize() { }
        public void Shutdown() { }

        public void SpawnLineClear(BoardCoord c) => SpawnFrom(_lineClearPrefab, _lineClearPool, c);
        public void SpawnBombBurst(BoardCoord c) => SpawnFrom(_bombPrefab, _bombPool, c);
        public void SpawnRainbowBurst(BoardCoord c) => SpawnFrom(_rainbowPrefab, _rainbowPool, c);
        public void SpawnCoinBurst(BoardCoord c) => SpawnFrom(_coinPrefab, _coinPool, c);

        private void SpawnFrom(ParticleSystem prefab, Queue<ParticleSystem> pool, BoardCoord c)
        {
            if (prefab == null) return;
            var ps = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, transform);
            ps.transform.position = new Vector3(c.X, 0.1f, c.Y); // map to world if needed
            ps.Clear();
            ps.Play();
            StartCoroutine(ReturnAfter(ps, pool, ps.main.duration + ps.main.startLifetime.constantMax));
        }

        private System.Collections.IEnumerator ReturnAfter(ParticleSystem ps, Queue<ParticleSystem> pool, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            ps.Stop();
            pool.Enqueue(ps);
        }
    }
}
