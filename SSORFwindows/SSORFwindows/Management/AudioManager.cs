using Microsoft.Xna.Framework.Audio;
using System;

namespace SSORF.Management
{
    class AudioManager
    {
        private AudioEngine audioEngine;
        private WaveBank waveBank;
        private SoundBank soundBank;

        private Cue menuMusic;
        private Cue missionMusic;
        private Cue engineSounds;
        private Cue miscSounds;

        public void LoadAudioContent()
        {
            audioEngine = new AudioEngine("Content/Audio/Background Music.xgs");
            waveBank = new WaveBank(audioEngine, "Content/Audio/Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content/Audio/Sound Bank.xsb");
        }

        public Cue getCue(String cueName)
        {
            return soundBank.GetCue(cueName);
        }

        public void playCue(String cueName)
        {
            this.getCue(cueName).Play();
        }

        public void stopCue(String cueName)
        {
            this.getCue(cueName).Stop(AudioStopOptions.AsAuthored);
        }

        public void pauseCue(String cueName)
        {
            this.getCue(cueName).Pause();
        }

        public void resumeCue(String cueName)
        {
            this.getCue(cueName).Resume();
        }

        public Cue getMenuMusic()
        {
            return menuMusic;
        }

        public Cue getMissionMusic()
        {
            return missionMusic;
        }

        public Cue getEngineSounds()
        {
            return engineSounds;
        }
    }
}
