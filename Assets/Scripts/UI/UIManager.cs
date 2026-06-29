// ----------------------------------------------------------------------------
// UIManager.cs
// Coordinates UI screens. Knows which screens are open and pushes / pops
// them on state changes. Listens to EventBus GameStateChangedEvent.
//
// The actual screen content is authored in Unity scenes / prefabs; this
// controller is the routing layer.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Game.BlockPuzzle.Core;
using UnityEngine;

namespace Game.BlockPuzzle.UI
{
    public enum ScreenId
    {
        MainMenu,
        ModeSelect,
        Hud,
        Pause,
        GameOver,
        Settings,
        Shop,
        Profile,
        Leaderboard,
        Achievements,
        DailyLogin,
        BattlePass,
        LuckySpin,
        Missions,
        ShopResult
    }

    public sealed class UIManager : MonoBehaviour, IService
    {
        [SerializeField] private ScreenId _initial = ScreenId.MainMenu;

        private readonly Stack<ScreenId> _stack = new();
        private readonly Dictionary<ScreenId, GameObject> _screens = new();

        public ScreenId Current => _stack.Count > 0 ? _stack.Peek() : ScreenId.MainMenu;

        public void Initialize()
        {
            AutoCollectScreens();
            Show(_initial, push: false);
        }

        public void Shutdown()
        {
            _stack.Clear();
            _screens.Clear();
        }

        private void AutoCollectScreens()
        {
            foreach (var id in System.Enum.GetValues(typeof(ScreenId)))
            {
                var go = GameObject.Find($"Screen_{id}");
                if (go != null) _screens[(ScreenId)id] = go;
            }
        }

        public void RegisterScreen(ScreenId id, GameObject root)
        {
            _screens[id] = root;
        }

        public void Show(ScreenId id, bool push = true)
        {
            if (_screens.TryGetValue(id, out var screen))
            {
                screen.SetActive(true);
                if (push && (_stack.Count == 0 || _stack.Peek() != id)) _stack.Push(id);
            }
        }

        public void Hide(ScreenId id)
        {
            if (_screens.TryGetValue(id, out var screen))
                screen.SetActive(false);
            if (_stack.Count > 0 && _stack.Peek() == id) _stack.Pop();
        }

        public void Back()
        {
            if (_stack.Count <= 1) return;
            var top = _stack.Pop();
            Hide(top);
            var now = _stack.Peek();
            if (_screens.TryGetValue(now, out var go)) go.SetActive(true);
        }
    }
}
