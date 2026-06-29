// ----------------------------------------------------------------------------
// DragHandler.cs
// Translates raw touch / mouse input into drag events for BlockPiece.
// Uses the legacy Input class for portability; swap to Input System in the
// project settings if desired — the public API is the same either way.
// ----------------------------------------------------------------------------

using System;
using Game.BlockPuzzle.Board;
using Game.BlockPuzzle.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BlockPuzzle.Blocks
{
    /// <summary>
    /// Single source of truth for input. Provides drag begin / update / end
    /// callbacks.
    /// </summary>
    public sealed class DragHandler : MonoBehaviour
    {
        public event Action<BlockPiece> OnDragBegin;
        public event Action<BlockPiece, Vector2> OnDragUpdate;
        public event Action<BlockPiece, Vector2, bool> OnDragEnd;   // bool = placedOnBoard

        private BlockPiece _dragging;
        private Camera _camera;
        private bool _overUI;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            HandleTouch();
            HandleMouse();
        }

        private void HandleTouch()
        {
            if (Input.touchCount == 0) return;
            var t = Input.GetTouch(0);
            Process(t.phase, t.position, t.fingerId);
        }

        private void HandleMouse()
        {
            if (Input.touchSupported && Input.touchCount > 0) return;
            if (Input.GetMouseButtonDown(0)) Process(TouchPhase.Began, Input.mousePosition, -1);
            else if (Input.GetMouseButton(0)) Process(TouchPhase.Moved, Input.mousePosition, -1);
            else if (Input.GetMouseButtonUp(0)) Process(TouchPhase.Ended, Input.mousePosition, -1);
        }

        private void Process(TouchPhase phase, Vector2 screenPos, int fingerId)
        {
            switch (phase)
            {
                case TouchPhase.Began:
                    BeginDrag(screenPos);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (_dragging != null) UpdateDrag(screenPos);
                    break;
                case TouchPhase.Ended:
                    if (_dragging != null) EndDrag(screenPos, true);
                    break;
                case TouchPhase.Canceled:
                    if (_dragging != null) EndDrag(screenPos, false);
                    break;
            }
        }

        private void BeginDrag(Vector2 screen)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // Only consider non-UI hits for drag begin — but we still want
                // to allow drag from piece buttons; rely on piece raycaster.
            }

            var piece = RaycastForPiece(screen);
            if (piece == null) return;
            _dragging = piece;
            _dragging.IsDragging = true;
            _dragging.transform.SetAsLastSibling();
            OnDragBegin?.Invoke(piece);
            UpdateDrag(screen);
        }

        private void UpdateDrag(Vector2 screen)
        {
            if (_dragging == null) return;
            Vector3 world = _camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10f));
            _dragging.transform.position = new Vector3(world.x, world.y, _dragging.transform.position.z);
            OnDragUpdate?.Invoke(_dragging, screen);
        }

        private void EndDrag(Vector2 screen, bool placedOnBoard)
        {
            if (_dragging == null) return;
            OnDragEnd?.Invoke(_dragging, screen, placedOnBoard);
            _dragging = null;
        }

        private BlockPiece RaycastForPiece(Vector2 screen)
        {
            var list = UnityEngine.Object.FindObjectsByType<BlockPiece>(FindObjectsSortMode.None);
            for (int i = 0; i < list.Length; i++)
            {
                var p = list[i];
                if (p == null) continue;
                var rt = (RectTransform)p.transform;
                if (RectTransformUtility.RectangleContainsScreenPoint(rt, screen, _camera))
                    return p;
            }
            return null;
        }

        public void ForceCancelDrag()
        {
            if (_dragging == null) return;
            _dragging.IsDragging = false;
            _dragging = null;
        }
    }
}
