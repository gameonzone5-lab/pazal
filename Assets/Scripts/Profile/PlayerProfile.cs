// ----------------------------------------------------------------------------
// PlayerProfile.cs
// The "face" of the player in the game: nickname, avatar, level, XP. Reads
// from SaveManager; nothing here persists directly.
// ----------------------------------------------------------------------------

using System;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Save;
using UnityEngine;

namespace Game.BlockPuzzle.Profile
{
    public sealed class PlayerProfile : MonoBehaviour, IService
    {
        public string PlayerId { get; private set; }
        public string DisplayName { get; private set; } = "Player";
        public string AvatarId { get; private set; } = "default";
        public int Level { get; private set; } = 1;
        public int LevelXp { get; private set; }
        public int XpForNextLevel => Level * 200;

        public event Action OnProfileChanged;

        public void Initialize()
        {
            var save = ServiceLocator.Resolve<SaveManager>();
            PlayerId = SystemInfo.deviceUniqueIdentifier;
            DisplayName = save.Current.OwnedCosmetics != null && save.Current.OwnedCosmetics.Count > 0
                ? save.Current.OwnedCosmetics[0]
                : "Player";
        }

        public void Shutdown() { }

        public void SetDisplayName(string name)
        {
            DisplayName = string.IsNullOrEmpty(name) ? "Player" : name;
            OnProfileChanged?.Invoke();
        }

        public void SetAvatar(string id)
        {
            AvatarId = id ?? "default";
            OnProfileChanged?.Invoke();
        }

        public void AwardXp(int amount)
        {
            LevelXp += amount;
            while (LevelXp >= XpForNextLevel)
            {
                LevelXp -= XpForNextLevel;
                Level++;
                OnProfileChanged?.Invoke();
            }
        }
    }
}
