// ----------------------------------------------------------------------------
// AsyncHelpers.cs
// Tiny set of helpers around Unity 6's Awaitable.
// ----------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.BlockPuzzle.Core
{
    public static class AsyncHelpers
    {
        /// <summary>
        /// Run a function on a thread-pool thread, return result on main thread.
        /// </summary>
        public static async Awaitable<T> RunOnThreadPoolAsync<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            await Task.Run(() =>
            {
                try { tcs.SetResult(func()); }
                catch (Exception ex) { tcs.SetException(ex); }
            });
            return tcs.Task.GetAwaiter().GetResult();
        }

        /// <summary>Wait for one frame.</summary>
        public static async Awaitable NextFrame() => await Awaitable.NextFrameAsync();

        /// <summary>Wait for N seconds (using realtime, not game time).</summary>
        public static async Awaitable WaitRealtime(float seconds)
        {
            var end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
                await Awaitable.NextFrameAsync();
        }

        /// <summary>Wait until a condition is true, polling each frame.</summary>
        public static async Awaitable WaitUntil(Func<bool> condition)
        {
            while (!condition()) await Awaitable.NextFrameAsync();
        }

        /// <summary>Cancellation-aware wait.</summary>
        public static async Awaitable WaitCancellable(float seconds, CancellationToken token)
        {
            var end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
            {
                if (token.IsCancellationRequested) return;
                await Awaitable.NextFrameAsync();
            }
        }
    }
}
