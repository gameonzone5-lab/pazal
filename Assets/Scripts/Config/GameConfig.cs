// ----------------------------------------------------------------------------
// GameConfig.cs
// Root ScriptableObject that aggregates every sub-config asset. Loaded once
// at boot from Resources/GameConfig/GameConfig.asset.
// ----------------------------------------------------------------------------

using UnityEngine;

namespace Game.BlockPuzzle.Config
{
    [CreateAssetMenu(menuName = "BlockCraft/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("General")]
        public int DefaultBoardSize = 10;
        public int PieceSlots = 3;
        public bool AllowBomb = true;
        public bool AllowRainbow = true;
        public bool AllowFrozen = true;
        public bool AllowLocked = true;

        [Header("Assets")]
        public BlockShapeLibrary Shapes;
        public BlockColorPalette Palette;
        public ThemeConfig Themes;
        public AudioConfig Audio;
        public EconomyConfig Economy;
        public MissionLibrary Missions;
        public LevelLibrary Levels;
        public BattlePassConfig BattlePass;
        public DailyLoginConfig DailyLogin;
        public LuckySpinConfig LuckySpin;
        public IAPProductLibrary Products;
        public AdUnitConfig Ads;
        public LocalizationConfig Localization;

        private static GameConfig _instance;
        public static GameConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GameConfig>("GameConfig/GameConfig");
                    if (_instance == null)
                    {
                        Log.Warn("GameConfig", "No GameConfig.asset in Resources/GameConfig — using defaults.");
                        _instance = CreateInstance<GameConfig>();
                    }
                }
                return _instance;
            }
        }
    }
}
