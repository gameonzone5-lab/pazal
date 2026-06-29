// ----------------------------------------------------------------------------
// SettingsController.cs
// Reads / writes player settings: theme, colorblind mode, volume, haptics,
// language, privacy toggles. Persists via SaveManager.MarkDirty().
// ----------------------------------------------------------------------------

using Game.BlockPuzzle.Audio;
using Game.BlockPuzzle.Core;
using Game.BlockPuzzle.Save;
using Game.BlockPuzzle.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BlockPuzzle.UI
{
    public sealed class SettingsController : MonoBehaviour
    {
        [Header("Theme")]
        public Toggle DarkToggle;
        public Toggle LightToggle;

        [Header("Colorblind")]
        public TMP_Dropdown ColorblindDropdown;

        [Header("Volume")]
        public Slider MasterSlider;
        public Slider MusicSlider;
        public Slider SfxSlider;

        [Header("Toggles")]
        public Toggle HapticsToggle;
        public Toggle ShowGhostToggle;
        public Toggle ParticleHighToggle;

        [Header("Privacy")]
        public Toggle AnalyticsToggle;
        public Toggle PersonalizedAdsToggle;

        [Header("Language")]
        public TMP_Dropdown LanguageDropdown;

        [Header("Buttons")]
        public Button CloseButton;
        public Button DeleteSaveButton;
        public Button PrivacyPolicyButton;
        public Button TermsButton;

        private void OnEnable()
        {
            LoadFromSave();
            Hook(true);
        }

        private void OnDisable()
        {
            Hook(false);
        }

        private void LoadFromSave()
        {
            var save = ServiceLocator.Resolve<SaveManager>();
            var s = save.Current.Settings;

            if (DarkToggle != null) DarkToggle.isOn = s.Theme == ThemeMode.Dark;
            if (LightToggle != null) LightToggle.isOn = s.Theme == ThemeMode.Light;
            if (ColorblindDropdown != null)
                ColorblindDropdown.value = (int)s.ColorBlind;
            if (MasterSlider != null) MasterSlider.value = s.MasterVolume;
            if (MusicSlider != null) MusicSlider.value = s.MusicVolume;
            if (SfxSlider != null) SfxSlider.value = s.SfxVolume;
            if (HapticsToggle != null) HapticsToggle.isOn = s.HapticsEnabled;
            if (ShowGhostToggle != null) ShowGhostToggle.isOn = s.ShowGhostPiece;
            if (ParticleHighToggle != null) ParticleHighToggle.isOn = s.ParticleQualityHigh;
            if (AnalyticsToggle != null) AnalyticsToggle.isOn = s.PrivacyAnalytics;
            if (PersonalizedAdsToggle != null) PersonalizedAdsToggle.isOn = s.PersonalizedAds;
        }

        private void Hook(bool on)
        {
            Bind(DarkToggle, on, v => SetTheme(ThemeMode.Dark));
            Bind(LightToggle, on, v => SetTheme(ThemeMode.Light));
            BindDropdown(ColorblindDropdown, on, SetColorBlind);
            BindSlider(MasterSlider, on, v => { ApplyVolume("master", v); });
            BindSlider(MusicSlider, on, v => { ApplyVolume("music", v); });
            BindSlider(SfxSlider, on, v => { ApplyVolume("sfx", v); });
            Bind(HapticsToggle, on, v => SetBool(s => s.HapticsEnabled = v, v));
            Bind(ShowGhostToggle, on, v => SetBool(s => s.ShowGhostPiece = v, v));
            Bind(ParticleHighToggle, on, v => SetBool(s => s.ParticleQualityHigh = v, v));
            Bind(AnalyticsToggle, on, v => SetBool(s => s.PrivacyAnalytics = v, v));
            Bind(PersonalizedAdsToggle, on, v => SetBool(s => s.PersonalizedAds = v, v));
            Bind(CloseButton, on, () => GameManager.Instance.GoToMainMenu());
            Bind(DeleteSaveButton, on, () =>
            {
                ServiceLocator.Resolve<SaveManager>().Delete();
                LoadFromSave();
            });
            Bind(PrivacyPolicyButton, on, () => Application.OpenURL("https://example.com/privacy"));
            Bind(TermsButton, on, () => Application.OpenURL("https://example.com/terms"));
        }

        // --------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------
        private void SetTheme(ThemeMode mode)
        {
            var save = ServiceLocator.Resolve<SaveManager>();
            save.Current.Settings.Theme = mode;
            save.MarkDirty();
            ServiceLocator.Resolve<ThemeManager>().ApplyTheme(mode);
        }

        private void SetColorBlind(int idx)
        {
            var mode = (ColorBlindMode)idx;
            var save = ServiceLocator.Resolve<SaveManager>();
            save.Current.Settings.ColorBlind = mode;
            save.MarkDirty();
            ServiceLocator.Resolve<ThemeManager>().SetColorBlindMode(mode);
        }

        private void ApplyVolume(string which, float value)
        {
            var save = ServiceLocator.Resolve<SaveManager>();
            var s = save.Current.Settings;
            switch (which)
            {
                case "master": s.MasterVolume = value; break;
                case "music": s.MusicVolume = value; break;
                case "sfx": s.SfxVolume = value; break;
            }
            save.MarkDirty();
            ServiceLocator.Resolve<AudioManager>().ApplyVolumes();
        }

        private void SetBool(System.Action<PlayerSettings> setter, bool value)
        {
            var save = ServiceLocator.Resolve<SaveManager>();
            setter(save.Current.Settings);
            save.MarkDirty();
        }

        private static void Bind(Toggle t, bool on, System.Action<bool> act)
        {
            if (t == null) return;
            t.onValueChanged.RemoveAllListeners();
            if (on) t.onValueChanged.AddListener(v => { SFXPlayer.Play(SFXPlayer.Pop); act(v); });
        }

        private static void Bind(Button b, bool on, System.Action act)
        {
            if (b == null) return;
            b.onClick.RemoveAllListeners();
            if (on) b.onClick.AddListener(() => { SFXPlayer.Play(SFXPlayer.Click); act?.Invoke(); });
        }

        private static void BindDropdown(TMP_Dropdown d, bool on, System.Action<int> act)
        {
            if (d == null) return;
            d.onValueChanged.RemoveAllListeners();
            if (on) d.onValueChanged.AddListener(act);
        }

        private static void BindSlider(Slider s, bool on, System.Action<float> act)
        {
            if (s == null) return;
            s.onValueChanged.RemoveAllListeners();
            if (on) s.onValueChanged.AddListener(act);
        }
    }
}
