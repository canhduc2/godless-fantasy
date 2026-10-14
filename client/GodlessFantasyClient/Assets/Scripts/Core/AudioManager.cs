using System;
using UnityEngine;

namespace GodlessFantasy.Core
{
    public enum BgmTrack
    {
        None,
        Village,
        Combat
    }

    /// <summary>
    /// Bộ Quản Lý Âm Thanh Chiến Đấu & Nhạc Nền (Procedural Audio SFX & BGM Engine):
    /// - 100% Thuật toán tổng hợp âm thanh thủ tục (Procedural Audio Synthesis) qua AudioClip.Create.
    /// - Không phụ thuộc bất kỳ file .wav / .mp3 ngoài nào, tương thích tuyệt đối mọi nền tảng (WebGL, Standalone, Editor).
    /// - Đầy đủ hiệu ứng SFX: Chém kiếm, Bắn phép, Nổ chiêu, Lướt Dash, Nhận đòn (Hit), Nhặt đồ, Thăng cấp, Báo chiêu Boss Telegraph, Rung chấn mặt đất, Cổng dịch chuyển.
    /// - 2 Bản nhạc nền BGM lặp tự nhiên: Thôn Làng Yên Bình (Village Ambient) & Chiến Trường Kịch Tính (Combat Pulse).
    /// - Phím nóng [M]: Bật / Tắt âm thanh tức thì (Mute Toggle).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        private AudioSource sfxSource;
        private AudioSource bgmSource;

        [Header("Volume Settings")]
        public float MasterVolume => GameSettings.MasterVolume;
        public float SfxVolume => GameSettings.SfxVolume;
        public float BgmVolume => GameSettings.BgmVolume;
        public bool IsMuted => GameSettings.IsMasterMuted;
        public bool IsBgmMuted => GameSettings.IsBgmMuted;
        public bool IsSfxMuted => GameSettings.IsSfxMuted;

        private BgmTrack currentBgmTrack = BgmTrack.None;

        // Cached Audio Clips
        private AudioClip clipSlash;
        private AudioClip clipCast;
        private AudioClip clipExplosion;
        private AudioClip clipDash;
        private AudioClip clipHit;
        private AudioClip clipPickup;
        private AudioClip clipLevelUp;
        private AudioClip clipTelegraph;
        private AudioClip clipBossSlam;
        private AudioClip clipTeleport;
        private AudioClip clipPetEvolve;
        private AudioClip clipBgmVillage;
        private AudioClip clipBgmCombat;

        private const int SampleRate = 44100;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            // Phím tắt [M]: Bật/tắt mute âm thanh bất cứ lúc nào
            if (GameInput.GetToggleMuteKeyDown())
            {
                ToggleMute();
            }
        }

        private void InitializeAudio()
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f; // 2D Sound

            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f; // 2D Sound

            // Tạo các clip âm thanh bằng thuật toán tổng hợp toán học
            GenerateAllClips();

            // Mặc định phát nhạc nền Làng Tân Thủ
            PlayBGM(BgmTrack.Village);
        }

        public static void EnsureExists()
        {
            if (Instance == null)
            {
                var go = new GameObject("AudioManager");
                Instance = go.AddComponent<AudioManager>();
            }
        }

        #region Procedural SFX Generators

        private void GenerateAllClips()
        {
            clipSlash = CreateSlashClip();
            clipCast = CreateCastClip();
            clipExplosion = CreateExplosionClip();
            clipDash = CreateDashClip();
            clipHit = CreateHitClip();
            clipPickup = CreatePickupClip();
            clipLevelUp = CreateLevelUpClip();
            clipTelegraph = CreateTelegraphClip();
            clipBossSlam = CreateBossSlamClip();
            clipTeleport = CreateTeleportClip();
            clipPetEvolve = CreatePetEvolveClip();

            clipBgmVillage = CreateVillageBgmClip();
            clipBgmCombat = CreateCombatBgmClip();
        }

        private AudioClip CreateSlashClip()
        {
            float duration = 0.14f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(42);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                // Vết chém: Giảm tần số từ 1200Hz xuống 250Hz kết hợp nhiễu trắng và decay mũ
                float freq = Mathf.Lerp(1200f, 250f, t);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SampleRate));
                float noise = ((float)rand.NextDouble() * 2f - 1f) * 0.7f;
                float envelope = Mathf.Exp(-t * 14f);
                data[i] = (tone * 0.4f + noise * 0.6f) * envelope * 0.85f;
            }

            var clip = AudioClip.Create("SFX_Slash", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateCastClip()
        {
            float duration = 0.26f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                // Phép thuật: Quét tần số vút lên 380Hz -> 820Hz kết hợp sóng hài
                float freq = Mathf.Lerp(380f, 820f, Mathf.Pow(t, 0.7f));
                float tone1 = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SampleRate));
                float tone2 = Mathf.Sin(4f * Mathf.PI * freq * (i / (float)SampleRate)) * 0.4f;
                float envelope = Mathf.Sin(t * Mathf.PI) * (1f - t * 0.4f);
                data[i] = (tone1 + tone2) * envelope * 0.65f;
            }

            var clip = AudioClip.Create("SFX_Cast", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateExplosionClip()
        {
            float duration = 0.45f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(108);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                // Nổ lớn: Âm trầm sub-bass 75Hz rền vang kết hợp nhiễu bùng nổ
                float bass = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(90f, 40f, t) * (i / (float)SampleRate));
                float noise = ((float)rand.NextDouble() * 2f - 1f);
                float envelope = Mathf.Exp(-t * 7.5f);
                data[i] = (bass * 0.6f + noise * 0.4f) * envelope;
            }

            var clip = AudioClip.Create("SFX_Explosion", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateDashClip()
        {
            float duration = 0.18f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(77);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                // Gió lướt ai: Tiếng rít gió vút qua nhanh
                float noise = ((float)rand.NextDouble() * 2f - 1f);
                float sweep = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(450f, 150f, t) * (i / (float)SampleRate));
                float envelope = Mathf.Sin(t * Mathf.PI) * Mathf.Exp(-t * 5f);
                data[i] = (noise * 0.65f + sweep * 0.35f) * envelope * 0.75f;
            }

            var clip = AudioClip.Create("SFX_Dash", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateHitClip()
        {
            float duration = 0.12f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(99);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                // Va đập trúng đòn: Transient đập mạnh + decay tức thì
                float thump = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(220f, 80f, t) * (i / (float)SampleRate));
                float crunch = ((float)rand.NextDouble() * 2f - 1f) * 0.5f;
                float envelope = Mathf.Exp(-t * 22f);
                data[i] = (thump * 0.7f + crunch * 0.3f) * envelope;
            }

            var clip = AudioClip.Create("SFX_Hit", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreatePickupClip()
        {
            float duration = 0.18f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                // Hai nốt chuông vàng thánh thót: Nốt 1 1320Hz, Nốt 2 1760Hz
                float freq = (t < 0.45f) ? 1320f : 1760f;
                float tone = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SampleRate));
                float envelope = (t < 0.45f) ? Mathf.Exp(-t * 8f) : Mathf.Exp(-(t - 0.45f) * 12f);
                data[i] = tone * envelope * 0.75f;
            }

            var clip = AudioClip.Create("SFX_Pickup", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateLevelUpClip()
        {
            float duration = 0.75f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            // 4 nốt rực rỡ C5 (523Hz), E5 (659Hz), G5 (784Hz), C6 (1046Hz)
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f };
            float noteDur = duration / notes.Length;

            for (int i = 0; i < samples; i++)
            {
                float timeSec = (float)i / SampleRate;
                int noteIdx = Mathf.Clamp((int)(timeSec / noteDur), 0, notes.Length - 1);
                float noteTime = timeSec - noteIdx * noteDur;

                float freq = notes[noteIdx];
                float tone = Mathf.Sin(2f * Mathf.PI * freq * timeSec) + 0.35f * Mathf.Sin(4f * Mathf.PI * freq * timeSec);
                float env = Mathf.Exp(-noteTime * 4.5f);
                data[i] = tone * env * 0.6f;
            }

            var clip = AudioClip.Create("SFX_LevelUp", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateTelegraphClip()
        {
            float duration = 0.38f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                // Chuông báo nguy hiểm: Tần số tăng nhanh 440Hz -> 880Hz kèm xung nhịp
                float freq = Mathf.Lerp(440f, 880f, t);
                float pulse = (Mathf.Sin(2f * Mathf.PI * 18f * (i / (float)SampleRate)) > 0f) ? 1f : 0.2f;
                float tone = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SampleRate));
                data[i] = tone * pulse * Mathf.Sin(t * Mathf.PI) * 0.55f;
            }

            var clip = AudioClip.Create("SFX_Telegraph", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateBossSlamClip()
        {
            float duration = 0.65f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            System.Random rand = new System.Random(333);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                // Cú đập của Boss chấn động mặt đất: Sub-bass 55Hz cực mạnh + rung chuyển kéo dài
                float sub = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(65f, 35f, t) * (i / (float)SampleRate));
                float debris = ((float)rand.NextDouble() * 2f - 1f) * 0.5f;
                float envelope = Mathf.Exp(-t * 5.2f);
                data[i] = (sub * 0.75f + debris * 0.25f) * envelope * 0.95f;
            }

            var clip = AudioClip.Create("SFX_BossSlam", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateTeleportClip()
        {
            float duration = 0.55f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                // Xoáy không gian (Vortex): Tần số điều biến FM dao động nhanh
                float lfo = Mathf.Sin(2f * Mathf.PI * 12f * t);
                float freq = Mathf.Lerp(300f, 950f, t) + lfo * 150f;
                float tone = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SampleRate));
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = tone * env * 0.6f;
            }

            var clip = AudioClip.Create("SFX_Teleport", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreatePetEvolveClip()
        {
            float duration = 0.7f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            // 3 chùm sao lấp lánh (Sparkle Chime)
            float[] freqs = { 880f, 1320f, 1760f, 2200f };
            float segDur = duration / freqs.Length;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                int idx = Mathf.Clamp((int)(t / segDur), 0, freqs.Length - 1);
                float segT = t - idx * segDur;

                float f = freqs[idx];
                float tone = Mathf.Sin(2f * Mathf.PI * f * t);
                float env = Mathf.Exp(-segT * 7.0f);
                data[i] = tone * env * 0.5f;
            }

            var clip = AudioClip.Create("SFX_PetEvolve", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        #endregion

        #region Procedural BGM Synthesizers

        private AudioClip CreateVillageBgmClip()
        {
            // Vòng lặp nhạc êm đềm 8 giây phong cách Lute đồng quê (C - G - Am - F)
            float duration = 8.0f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            // Chuỗi hợp âm và giai điệu ngũ cung
            float[][] chords = new float[][]
            {
                new float[] { 261.63f, 329.63f, 392.00f }, // C Major (C - E - G)
                new float[] { 196.00f, 246.94f, 293.66f }, // G Major (G - B - D)
                new float[] { 220.00f, 261.63f, 329.63f }, // A Minor (A - C - E)
                new float[] { 174.61f, 220.00f, 261.63f }  // F Major (F - A - C)
            };

            float chordDuration = duration / chords.Length; // 2.0s mỗi hợp âm

            for (int i = 0; i < samples; i++)
            {
                float timeSec = (float)i / SampleRate;
                int chordIdx = Mathf.Clamp((int)(timeSec / chordDuration), 0, chords.Length - 1);
                float[] currentChord = chords[chordIdx];
                float chordTime = timeSec - chordIdx * chordDuration;

                // Arpeggio rải nốt nhẹ nhàng
                float noteSpeed = 0.5f;
                int noteInChord = ((int)(chordTime / noteSpeed)) % currentChord.Length;
                float freq = currentChord[noteInChord];

                float wave = Mathf.Sin(2f * Mathf.PI * freq * timeSec) * 0.5f
                           + Mathf.Sin(2f * Mathf.PI * (freq * 0.5f) * timeSec) * 0.3f; // Bass đệm

                float noteT = chordTime % noteSpeed;
                float env = Mathf.Exp(-noteT * 3.2f);

                // Thêm âm hưởng gió nhẹ ambient
                float ambientWind = Mathf.Sin(2f * Mathf.PI * 0.2f * timeSec) * 0.05f;

                data[i] = (wave * env * 0.28f + ambientWind);
            }

            var clip = AudioClip.Create("BGM_Village", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateCombatBgmClip()
        {
            // Vòng lặp chiến đấu 6 giây: Nhịp đập 120 BPM kịch tính, dồn dập (D Minor)
            float duration = 6.0f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            float beatInterval = 0.5f; // 120 BPM = 0.5s/beat

            for (int i = 0; i < samples; i++)
            {
                float timeSec = (float)i / SampleRate;
                float beatTime = timeSec % beatInterval;

                // Nhịp trống kick dồn dập (Sub-pulse 80Hz -> 45Hz)
                float kickFreq = Mathf.Lerp(80f, 45f, beatTime / beatInterval);
                float kick = Mathf.Sin(2f * Mathf.PI * kickFreq * timeSec) * Mathf.Exp(-beatTime * 14f);

                // Dòng bass điện tử rền rĩ theo nốt D2 (73.4Hz) và F2 (87.3Hz)
                float bassFreq = ((int)(timeSec / 1.5f) % 2 == 0) ? 73.42f : 87.31f;
                float bassWave = Mathf.Sin(2f * Mathf.PI * bassFreq * timeSec) * 0.22f;

                // Giai điệu căng thẳng
                float arpHack = Mathf.Sin(2f * Mathf.PI * (bassFreq * 4f) * timeSec) * 0.08f;

                data[i] = (kick * 0.45f + bassWave + arpHack) * 0.45f;
            }

            var clip = AudioClip.Create("BGM_Combat", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        #endregion

        #region Public SFX Playback

        public static void PlaySlash() => Play(Instance?.clipSlash, 0.9f);
        public static void PlayCast() => Play(Instance?.clipCast, 0.85f);
        public static void PlayExplosion() => Play(Instance?.clipExplosion, 1.0f);
        public static void PlayDash() => Play(Instance?.clipDash, 0.85f);
        public static void PlayHit() => Play(Instance?.clipHit, 0.95f);
        public static void PlayPickup() => Play(Instance?.clipPickup, 0.8f);
        public static void PlayEquip() => Play(Instance?.clipPickup, 0.9f);
        public void PlayEquipItemSound() => PlayEquip();
        public static void PlayDrop() => Play(Instance?.clipPickup, 0.75f);
        public static void PlayLevelUp() => Play(Instance?.clipLevelUp, 1.0f);
        public static void PlayTelegraphCharge() => Play(Instance?.clipTelegraph, 0.9f);
        public static void PlayBossSlam() => Play(Instance?.clipBossSlam, 1.0f);
        public static void PlayTeleport() => Play(Instance?.clipTeleport, 0.9f);
        public static void PlayPetEvolve() => Play(Instance?.clipPetEvolve, 0.95f);

        public static void PlaySlash(Vector3 pos) => PlayAt(Instance?.clipSlash, pos, 0.9f);
        public static void PlayCast(Vector3 pos) => PlayAt(Instance?.clipCast, pos, 0.85f);
        public static void PlayExplosion(Vector3 pos) => PlayAt(Instance?.clipExplosion, pos, 1.0f);
        public static void PlayHit(Vector3 pos) => PlayAt(Instance?.clipHit, pos, 0.95f);

        public static void PlayAt(AudioClip clip, Vector3 worldPos, float volumeScale = 1.0f, float maxDist = 28f)
        {
            if (Instance == null || clip == null) return;
            if (GameSettings.IsMasterMuted || GameSettings.IsSfxMuted) return;

            if (Camera.main != null)
            {
                Vector3 camPos = Camera.main.transform.position;
                camPos.z = worldPos.z;
                float dist = Vector3.Distance(camPos, worldPos);
                if (dist > maxDist) return; // Khoảng cách > 28m: Bỏ qua âm thanh để giải phóng CPU
                float falloff = 1f - (dist / maxDist);
                Play(clip, volumeScale * falloff);
                return;
            }
            Play(clip, volumeScale);
        }

        private static void Play(AudioClip clip, float volumeScale = 1.0f)
        {
            if (Instance == null || clip == null) return;
            if (GameSettings.IsMasterMuted || GameSettings.IsSfxMuted) return;

            if (Instance.sfxSource != null)
            {
                float finalVol = volumeScale * GameSettings.SfxVolume * GameSettings.MasterVolume;
                Instance.sfxSource.PlayOneShot(clip, finalVol);
            }
        }

        #endregion

        #region Public BGM Control

        public void ApplyAudioSettings()
        {
            if (bgmSource != null)
            {
                bool isMuted = GameSettings.IsMasterMuted || GameSettings.IsBgmMuted;
                bgmSource.volume = isMuted ? 0f : (GameSettings.BgmVolume * GameSettings.MasterVolume);
            }
        }

        public void PlayBGM(BgmTrack track)
        {
            if (bgmSource == null) return;
            if (currentBgmTrack == track && bgmSource.isPlaying) return;

            currentBgmTrack = track;
            AudioClip clipToPlay = null;

            switch (track)
            {
                case BgmTrack.Village:
                    clipToPlay = clipBgmVillage;
                    break;
                case BgmTrack.Combat:
                    clipToPlay = clipBgmCombat;
                    break;
                case BgmTrack.None:
                    bgmSource.Stop();
                    return;
            }

            if (clipToPlay != null)
            {
                bgmSource.clip = clipToPlay;
                bool isMuted = GameSettings.IsMasterMuted || GameSettings.IsBgmMuted;
                bgmSource.volume = isMuted ? 0f : (GameSettings.BgmVolume * GameSettings.MasterVolume);
                bgmSource.Play();
            }
        }

        public static void SwitchBGM(BgmTrack track)
        {
            EnsureExists();
            Instance?.PlayBGM(track);
        }

        public void ToggleMute()
        {
            bool nextState = !GameSettings.IsMasterMuted;
            GameSettings.SetMasterMute(nextState);

            string status = nextState ? "TẮT (Muted)" : "BẬT (Unmuted)";
            Combat.FloatingDamageManager.ShowFloatingText(
                Vector3.zero,
                $"🎵 Âm thanh: {status} [M]",
                nextState ? new Color(0.85f, 0.4f, 0.4f) : new Color(0.4f, 0.95f, 0.5f)
            );
            Debug.Log($"<color=cyan>[AUDIO]</color> Trạng thái âm thanh: {status}");
        }

        #endregion
    }
}
