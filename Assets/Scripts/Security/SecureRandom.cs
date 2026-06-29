// ----------------------------------------------------------------------------
// SecureRandom.cs
// Cryptographically secure RNG. Use for anything that affects rewards /
// pieces / server reconciliation. Do NOT use UnityEngine.Random for those;
// it's a Mersenne Twister and easily predicted once a few outputs are known.
// ----------------------------------------------------------------------------

using System;
using System.Security.Cryptography;

namespace Game.BlockPuzzle.Security
{
    public sealed class SecureRandom
    {
        private readonly RandomNumberGenerator _rng;
        private readonly byte[] _buf = new byte[4];

        public SecureRandom() { _rng = RandomNumberGenerator.Create(); }
        public SecureRandom(string seed)
        {
            _rng = RandomNumberGenerator.Create();
            // seed parameter is informational only — we deliberately do NOT
            // expose a deterministic seeded mode here so callers can't
            // accidentally use a predictable stream for reward draws.
        }

        /// <summary>Uniform float in [0, 1).</summary>
        public float NextFloat01()
        {
            uint v = NextUInt();
            return (v & 0x00FFFFFF) / (float)0x01000000;
        }

        /// <summary>Integer in [min, max).</summary>
        public int NextInt(int min, int max)
        {
            if (max <= min) return min;
            uint range = (uint)(max - min);
            uint v = NextUInt();
            return (int)(v % range) + min;
        }

        public uint NextUInt()
        {
            _rng.GetBytes(_buf);
            return (uint)(_buf[0] | (_buf[1] << 8) | (_buf[2] << 16) | (_buf[3] << 24));
        }
    }
}
