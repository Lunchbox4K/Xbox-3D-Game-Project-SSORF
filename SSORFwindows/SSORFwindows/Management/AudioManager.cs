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

        private static Boolean isMusicPlaying;
        private static Boolean isSoundPlaying;

        public static void LoadAudioContent()
        {
            audioEngine = new AudioEngine("Content/Audio/Background Music.xgs");
            waveBank = new WaveBank(audioEngine, "Content/Audio/Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content/Audio/Sound Bank.xsb");

            menuMusic = soundBank.GetCue("Exciting Ride");
            missionMusic = soundBank.GetCue("Journey");
            engineSounds = soundBank.GetCue("Engine");
        }

        public static void UpdateMusic(GameState state)
        {
            switch (state)
            {
                // If we are viewing title screen...
                case GameState.TitleScreen:
                    break;

                // If we are viewing the menus...
                case GameState.MenuScreen:
                    //Only play the music if music is on
                    if (isMusicPlaying)
                    {
                        if (!menuMusic.IsPlaying && menuMusic.IsPrepared)
                            menuMusic.Play();
                        else if (menuMusic.IsStopped)
                        {
                            resetMenuMusic();
                            menuMusic.Play();
                        }
                    }
                    if (missionMusic.IsPlaying)
                        missionMusic.Stop(AudioStopOptions.AsAuthored);
                    if (engineSounds.IsPlaying)
                        engineSounds.Stop(AudioStopOptions.AsAuthored);
                    break;

                // If we are playing a mission...
                case GameState.MissionScreen:
                    //Only play the music if music is on
                    if (isMusicPlaying)
                    {
                        if (!missionMusic.IsPlaying && missionMusic.IsPrepared)
                            missionMusic.Play();
                        else if (missionMusic.IsStopped)
                        {
                            resetMissionMusic();
                            missionMusic.Play();
                        }
                    }
                    if (menuMusic.IsPlaying)
                        menuMusic.Stop(AudioStopOptions.AsAuthored);
                    break;
            }
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
            engineSounds = soundBank.GetCue("Engine");
        }

        public static void setEngineSounds(String cueName)
        {
            engineSounds = getCue(cueName);
        }

        public static void setMusicPlaying(Boolean isPlaying)
        {
            isMusicPlaying = isPlaying;
        }

        public static void setSoundPlaying(Boolean isPlaying)
        {
            isSoundPlaying = isPlaying;
        }
    }
}