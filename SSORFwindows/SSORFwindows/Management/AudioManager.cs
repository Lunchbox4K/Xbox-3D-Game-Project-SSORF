using Microsoft.Xna.Framework.Audio;
using System;

namespace SSORF.Management
{
    public static class AudioManager
    {
        private static AudioEngine audioEngine;
        private static WaveBank waveBank;
        private static SoundBank soundBank;

        private static Cue menuMusic;
        private static Cue missionMusic;
        private static Cue engineSounds;

        public static void LoadAudioContent()
        {
            audioEngine = new AudioEngine("Content/Audio/Background Music.xgs");
            waveBank = new WaveBank(audioEngine, "Content/Audio/Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content/Audio/Sound Bank.xsb");

            menuMusic = soundBank.GetCue("Exciting Ride");
            missionMusic = soundBank.GetCue("Journey");
            engineSounds = soundBank.GetCue("Engine");
        }

        public static void Update()
        {
            audioEngine.Update();
        }

        public static Cue getCue(String cueName)
        {
            return soundBank.GetCue(cueName);
        }

        public static Cue getMenuMusic()
        {
            return menuMusic;
        }

        public static void resetMenuMusic()
        {
            menuMusic = soundBank.GetCue("Exciting Ride");
        }

        public static Cue getMissionMusic()
        {
            return missionMusic;
        }

        public static void resetMissionMusic()
        {
            missionMusic = soundBank.GetCue("Journey");
        }

        public static Cue getEngineSounds()
        {
            return engineSounds;
        }

        public static void resetEngineSounds()
        {
            engineSounds = soundBank.GetCue("At Speed");
        }

        public static void setEngineSounds(String cueName)
        {
            engineSounds = getCue(cueName);
        }
    }
}