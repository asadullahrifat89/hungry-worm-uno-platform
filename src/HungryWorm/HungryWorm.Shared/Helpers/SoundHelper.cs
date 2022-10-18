using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HungryWorm
{
    public static class SoundHelper
    {
        #region Fields

        private static readonly Random _random = new();

        private static Sound[] _sounds;
        private static List<Sound> _playingSounds;

        #endregion

        #region Methods

        public static void LoadGameSounds(Action completed = null)
        {
            if (_sounds is null)
            {
                _sounds = Constants.SOUND_TEMPLATES.Select(x =>
                {
                    Sound sound = null;

                    switch (x.Key)
                    {
                        case SoundType.BACKGROUND:
                            {
                                sound = new Sound(soundType: x.Key, soundSource: x.Value, volume: 0.7, loop: true);
                            }
                            break;
                        case SoundType.INTRO:
                            {
                                sound = new Sound(soundType: x.Key, soundSource: x.Value, volume: 0.9, loop: true);
                            }
                            break;
                        case SoundType.ATE_FOOD:
                            {
                                sound = new Sound(soundType: x.Key, soundSource: x.Value, volume: 0.9);
                            }
                            break;
                        default:
                            {
                                sound = new Sound(soundType: x.Key, soundSource: x.Value);
                            }
                            break;
                    }

                    return sound;

                }).ToArray();

                _playingSounds = new List<Sound>();

                // add all sounds except background and intro as these will be randomized before playing
                _playingSounds.AddRange(_sounds.Where(x => x.SoundType is not SoundType.BACKGROUND and not SoundType.INTRO));

                completed?.Invoke();
            }
            else
            {
                completed?.Invoke();
            }

#if DEBUG
            Console.WriteLine("LOADED SOUNDS: " + _playingSounds.Count);
#endif
        }

        public static void RandomizeSound(SoundType soundType)
        {
            foreach (var sound in _playingSounds.Where(x => x.SoundType == soundType))
            {
                sound.Stop();
            }

            var introSounds = _sounds.Where(x => x.SoundType == soundType).ToArray();
            var introSound = introSounds[_random.Next(0, introSounds.Length)];

            _playingSounds.RemoveAll(x => x.SoundType == soundType);
            _playingSounds.Add(introSound);
        }

        public static bool IsSoundPlaying(SoundType soundType)
        {
            return _playingSounds.Any(x => x.SoundType == soundType && x.IsPlaying);
        }

        public static void PlaySound(SoundType soundType)
        {
            if (_playingSounds.FirstOrDefault(x => x.SoundType == soundType) is Sound playingSound)
                playingSound.Play();
        }

        public static void PlayRandomSound(SoundType soundType)
        {
            var sounds = _playingSounds.Where(x => x.SoundType == soundType).ToArray();

            if (sounds.Length > 1)
            {
                var sound = sounds[new Random().Next(0, sounds.Length)];
                sound.Play();
            }
            else
            {
                if (sounds.FirstOrDefault(x => x.SoundType == soundType) is Sound playingSound)
                    playingSound.Play();
            }
        }

        public static void StopSound(SoundType soundType)
        {
            if (_playingSounds.FirstOrDefault(x => x.SoundType == soundType) is Sound playingSound)
                playingSound.Stop();
        }

        public static void PauseSound(SoundType soundType)
        {
            if (_playingSounds.FirstOrDefault(x => x.SoundType == soundType && x.IsPlaying) is Sound playingSound)
                playingSound.Pause();
        }

        public static void ResumeSound(SoundType soundType)
        {
            if (_playingSounds.FirstOrDefault(x => x.SoundType == soundType && x.IsPaused) is Sound playingSound)
                playingSound.Resume();
        }

        #endregion
    }
}
