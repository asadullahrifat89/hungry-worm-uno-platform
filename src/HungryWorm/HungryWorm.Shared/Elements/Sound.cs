using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HungryWorm
{
    public class Sound
    {
        #region Fields

        private readonly AudioPlayer _audioPlayer;

        #endregion

        public Sound(SoundType soundType, string soundSource, double volume = 1.0, bool loop = false)
        {
            SoundType = soundType;
            SoundSource = soundSource;

            var baseUrl = AssetHelper.GetBaseUrl();
            var source = $"{baseUrl}/{SoundSource}";
            _audioPlayer = new AudioPlayer(source: source, volume: volume, loop: loop);
        }

        #region Properties

        public SoundType SoundType { get; set; }

        public string SoundSource { get; set; }

        public bool IsPlaying { get; set; }

        public bool IsPaused { get; set; }

        #endregion

        #region Methods

        public void Play()
        {
            _audioPlayer.Play();
            IsPlaying = true;
        }

        public void Stop()
        {
            _audioPlayer.Stop();
            IsPlaying = false;
        }

        public void Pause()
        {
            _audioPlayer.Pause();
            IsPaused = true;
        }

        public void Resume()
        {
            _audioPlayer.Resume();
        }

        #endregion
    }

    public enum SoundType
    {
        MENU_SELECT,
        INTRO,
        BACKGROUND,
        GAME_OVER,
        POWER_UP,
        POWER_DOWN,
        ATE_FOOD,
    }
}
