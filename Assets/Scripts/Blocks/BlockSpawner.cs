// ----------------------------------------------------------------------------
// BlockSpawner.cs
// Owns the piece slots at the bottom of the screen. Each frame, it ensures
// the slots are filled with random pieces drawn from the configured shape
// library. Special pieces (Bomb / Rainbow) are mixed in at the configured
// rates.
//
// Spawner decides whether the player can fit any of their current pieces
// (game-over detection).
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Config;
using Game.BlockPuzzle.Core;
using UnityEngine;

namespace Game.BlockPuzzle.Blocks
{
    public sealed class BlockSpawner : MonoBehaviour, IService
    {
        [SerializeField] private int _slotCount = Constants.PieceSlots;
        [SerializeField] private float _bombChance = 0.04f;
        [SerializeField] private float _rainbowChance = 0.04f;

        private BoardController _board;
        private Security.SecureRandom _rng;
        private BlockPiece[] _slots;
        private int _pieceIdCounter;

        public IReadOnlyList<BlockPiece> Slots => _slots;

        public void Initialize()
        {
            _board = ServiceLocator.Resolve<BoardController>();
            _rng = new Security.SecureRandom();
            _slots = new BlockPiece[_slotCount];
            for (int i = 0; i < _slotCount; i++)
            {
                _slots[i] = CreateSlotInstance(i);
                _slots[i].gameObject.SetActive(false);
            }
            RefillAll();
        }

        public void Shutdown()
        {
            _slots = null;
        }

        public void OnRunStart()
        {
            _pieceIdCounter = 0;
            for (int i = 0; i < _slotCount; i++) _slots[i].gameObject.SetActive(false);
            RefillAll();
        }

        public BlockPiece CreateSlotInstance(int slotIndex)
        {
            var go = new GameObject($"PieceSlot_{slotIndex}");
            go.transform.SetParent(transform, false);
            var piece = go.AddComponent<BlockPiece>();
            return piece;
        }

        /// <summary>
        /// Returns true if at least one piece in the current hand fits ANY
        /// position on the board. False means the player has lost.
        /// </summary>
        public bool AnyFits()
        {
            var lib = GameConfig.Instance.Shapes;
            if (lib == null || _board?.Board == null) return false;
            for (int s = 0; s < _slots.Length; s++)
            {
                var piece = _slots[s];
                if (piece == null || piece.Shape == null) continue;
                if (CanFitAnywhere(piece.Shape)) return true;
            }
            return false;
        }

        public bool CanFitAnywhere(BlockShape shape)
        {
            var board = _board.Board;
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    if (board.CanPlace(shape, x, y)) return true;
                }
            }
            return false;
        }

        public void RefillSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length) return;
            var lib = GameConfig.Instance.Shapes;
            if (lib == null) return;

            var piece = _slots[slotIndex];
            if (piece == null) return;

            var shape = lib.PickRandom(_rng);
            if (shape == null) return;

            byte colorIndex = (byte)_rng.NextInt(0, GameConfig.Instance.Palette.Standard.Length);
            var kind = RollPieceKind();
            piece.Setup(shape, colorIndex, ++_pieceIdCounter, kind);
            piece.gameObject.SetActive(true);
        }

        public void RefillAll()
        {
            for (int i = 0; i < _slots.Length; i++) RefillSlot(i);
        }

        private BlockPiece.PieceKind RollPieceKind()
        {
            float r = _rng.NextFloat01();
            if (GameConfig.Instance != null && GameConfig.Instance.AllowBomb && r < _bombChance)
                return BlockPiece.PieceKind.Bomb;
            if (GameConfig.Instance != null && GameConfig.Instance.AllowRainbow && r < _bombChance + _rainbowChance)
                return BlockPiece.PieceKind.Rainbow;
            return BlockPiece.PieceKind.Normal;
        }

        /// <summary>Returns the slot index of the given piece, or -1.</summary>
        public int IndexOf(BlockPiece piece)
        {
            for (int i = 0; i < _slots.Length; i++)
                if (ReferenceEquals(_slots[i], piece)) return i;
            return -1;
        }
    }
}
