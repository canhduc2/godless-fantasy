using System;
using UnityEngine;

namespace GodlessFantasy.Core
{
    /// <summary>
    /// Hệ Thống Cài Đặt Toàn Diện Cho Game (Godless Fantasy Settings Manager):
    /// - Quản lý Âm thanh (Master, BGM, SFX volume & mute toggles).
    /// - Quản lý Tốc độ hiệu ứng / Giả lập game (Simulation Speed / Slow-Motion Slider: 0.1x -> 2.0x).
    ///   Cho phép tester/dev làm chậm chuyển động để soi từng frame đồ họa, quỹ đạo bay và bộc phá hạt.
    /// - Quản lý Tiện ích hiển thị: Số sát thương (Damage Numbers), Rung màn hình (Screen Shake), FPS counter.
    /// - Tự động ghi nhớ và lưu trữ liên tục qua Unity PlayerPrefs.
    /// </summary>
    public static class GameSettings
    {
        private const string KeyMasterVol = "GF_MasterVolume";
        private const string KeyBgmVol = "GF_BgmVolume";
        private const string KeySfxVol = "GF_SfxVolume";
        private const string KeyMasterMuted = "GF_MasterMuted";
        private const string KeyBgmMuted = "GF_BgmMuted";
        private const string KeySfxMuted = "GF_SfxMuted";
        private const string KeySimSpeed = "GF_SimSpeed";
        private const string KeyShowDamage = "GF_ShowDamage";
        private const string KeyScreenShake = "GF_ScreenShake";
        private const string KeyShowFps = "GF_ShowFps";
        private const string KeyDistanceRuler = "GF_DistanceRuler";

        // ================= ÂM THANH =================
        public static float MasterVolume { get; private set; } = 1.0f;
        public static float BgmVolume { get; private set; } = 0.50f;
        public static float SfxVolume { get; private set; } = 0.85f;
        public static bool IsMasterMuted { get; private set; } = false;
        public static bool IsBgmMuted { get; private set; } = false;
        public static bool IsSfxMuted { get; private set; } = false;

        // ================= TỐC ĐỘ HIỆU ỨNG & GIẢ LẬP (SLOW-MO) =================
        public static float SimulationSpeed { get; private set; } = 1.0f;

        // ================= TIỆN ÍCH & ĐỒ HỌA =================
        public static bool ShowDamageNumbers { get; private set; } = true;
        public static bool ScreenShakeEnabled { get; private set; } = true;
        public static bool ShowFpsCounter { get; private set; } = true;
        public static bool IsDistanceRulerEnabled { get; private set; } = true;

        private static bool isInitialized = false;

        static GameSettings()
        {
            LoadSettings();
        }

        public static void Initialize()
        {
            if (!isInitialized)
            {
                LoadSettings();
                isInitialized = true;
            }
        }

        public static void LoadSettings()
        {
            MasterVolume = PlayerPrefs.GetFloat(KeyMasterVol, 1.0f);
            BgmVolume = PlayerPrefs.GetFloat(KeyBgmVol, 0.50f);
            SfxVolume = PlayerPrefs.GetFloat(KeySfxVol, 0.85f);

            IsMasterMuted = PlayerPrefs.GetInt(KeyMasterMuted, 0) == 1;
            IsBgmMuted = PlayerPrefs.GetInt(KeyBgmMuted, 0) == 1;
            IsSfxMuted = PlayerPrefs.GetInt(KeySfxMuted, 0) == 1;

            SimulationSpeed = PlayerPrefs.GetFloat(KeySimSpeed, 1.0f);
            Time.timeScale = SimulationSpeed;

            ShowDamageNumbers = PlayerPrefs.GetInt(KeyShowDamage, 1) == 1;
            ScreenShakeEnabled = PlayerPrefs.GetInt(KeyScreenShake, 1) == 1;
            ShowFpsCounter = PlayerPrefs.GetInt(KeyShowFps, 1) == 1;
            IsDistanceRulerEnabled = PlayerPrefs.GetInt(KeyDistanceRuler, 1) == 1;
        }

        public static void SetMasterVolume(float vol)
        {
            MasterVolume = Mathf.Clamp01(vol);
            PlayerPrefs.SetFloat(KeyMasterVol, MasterVolume);
            AudioManager.Instance?.ApplyAudioSettings();
        }

        public static void SetBgmVolume(float vol)
        {
            BgmVolume = Mathf.Clamp01(vol);
            PlayerPrefs.SetFloat(KeyBgmVol, BgmVolume);
            AudioManager.Instance?.ApplyAudioSettings();
        }

        public static void SetSfxVolume(float vol)
        {
            SfxVolume = Mathf.Clamp01(vol);
            PlayerPrefs.SetFloat(KeySfxVol, SfxVolume);
            AudioManager.Instance?.ApplyAudioSettings();
        }

        public static void SetMasterMute(bool muted)
        {
            IsMasterMuted = muted;
            PlayerPrefs.SetInt(KeyMasterMuted, IsMasterMuted ? 1 : 0);
            AudioManager.Instance?.ApplyAudioSettings();
        }

        public static void SetBgmMute(bool muted)
        {
            IsBgmMuted = muted;
            PlayerPrefs.SetInt(KeyBgmMuted, IsBgmMuted ? 1 : 0);
            AudioManager.Instance?.ApplyAudioSettings();
        }

        public static void SetSfxMute(bool muted)
        {
            IsSfxMuted = muted;
            PlayerPrefs.SetInt(KeySfxMuted, IsSfxMuted ? 1 : 0);
            AudioManager.Instance?.ApplyAudioSettings();
        }

        public static void SetSimulationSpeed(float speed)
        {
            SimulationSpeed = Mathf.Clamp(speed, 0.1f, 2.0f);
            Time.timeScale = SimulationSpeed;
            PlayerPrefs.SetFloat(KeySimSpeed, SimulationSpeed);
        }

        public static void SetShowDamageNumbers(bool enabled)
        {
            ShowDamageNumbers = enabled;
            PlayerPrefs.SetInt(KeyShowDamage, ShowDamageNumbers ? 1 : 0);
        }

        public static void SetScreenShake(bool enabled)
        {
            ScreenShakeEnabled = enabled;
            PlayerPrefs.SetInt(KeyScreenShake, ScreenShakeEnabled ? 1 : 0);
        }

        public static void SetShowFpsCounter(bool enabled)
        {
            ShowFpsCounter = enabled;
            PlayerPrefs.SetInt(KeyShowFps, ShowFpsCounter ? 1 : 0);
        }

        public static void SetDistanceRulerEnabled(bool enabled)
        {
            IsDistanceRulerEnabled = enabled;
            PlayerPrefs.SetInt(KeyDistanceRuler, IsDistanceRulerEnabled ? 1 : 0);
        }
    }
}
