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

        private const String MENU_CUE = "Exciting Ride";
        private const String MISSION_CUE = "Journey";
        private const String ENGINE_CUE = "Engine";

        //Start music off so people dont kill me
        private static Boolean isMusicPlaying = false;
        private static Boolean isSoundPlaying = true;
        private static Boolean isMusicOn = false;
        private static Boolean isSoundOn = true;

        /// <summary>
        /// Loads all the audio files etc.
        /// Must be called before using AudioManager!
        /// </summary>
        public static void LoadAudioContent()
        {
            audioEngine = new AudioEngine("Content\\Audio\\Background Music.xgs");
            waveBank = new WaveBank(audioEngine, "Content\\Audio\\Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content\\Audio\\Sound Bank.xsb");

            menuMusic = soundBank.GetCue(MENU_CUE);
            missionMusic = soundBank.GetCue(MISSION_CUE);
            engineSounds = soundBank.GetCue(ENGINE_CUE);
        }

        /// <summary>
        /// Updates the music across all game states
        /// Must be called in an update method in state manager
        /// </summary>
        public static void UpdateMusic(GameState state)
        {
            switch (state)
            {
                //If we are viewing title screen...
                case GameState.TitleScreen:
                    break;

                //If we are viewing the menus...
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
            //Update to make sure things get stopped etc.
            Update();
        }

        /// <summary>
        /// Updates the engine sounds for the scooter
        /// Must be called inside the vehicle class
        /// </summary>
        /// <param name="throttleValue"></param>
        /// <param name="speed"></param>
        public static void UpdateEngine(float throttleValue, float speed)
        {
            if (isSoundPlaying)
            {
                engineSounds.SetVariable("throttleValue", throttleValue + speed);
                engineSounds.SetVariable("Speed", speed);
                if (throttleValue != 0 || speed > 0.05)
                {

                    if (engineSounds.IsPaused)
                        engineSounds.Resume();
                    else if (engineSounds.IsStopped)
                    {
                        resetEngineSounds();
                        engineSounds.Play();
                    }
                    else if (engineSounds.IsPlaying == false &&
                        engineSounds.IsPrepared)
                        engineSounds.Play();
                }
                else if (speed < 0.05)
                {
                    engineSounds.Stop(AudioStopOptions.AsAuthored);
                }
            }
        }

        /// <summary>
        /// Updates the audio engine
        /// Must be called inside an update method
        /// </summary>
        public static void Update()
        {
            audioEngine.Update();
        }

        /// <summary>
        /// Pauses all Audio
        /// </summary>
        public static void PauseAudio()
        {
            isMusicOn = isMusicPlaying;
            isSoundOn = isSoundPlaying;
            isMusicPlaying = false;
            isSoundPlaying = false;

            if (missionMusic.IsPlaying)
                missionMusic.Stop(AudioStopOptions.AsAuthored);
            if (menuMusic.IsPlaying)
                menuMusic.Stop(AudioStopOptions.AsAuthored);
            if (engineSounds.IsPlaying)
                engineSounds.Stop(AudioStopOptions.AsAuthored);
        }

        /// <summary>
        /// Turns music back on allowing it to be played again
        /// </summary>
        public static void ResumeAudio()
        {
            //Turn the music back on if it should be turned back on
            if (isMusicOn)
            {
                isMusicPlaying = true;
            }
            //Turn the sound back on if it should be turned back on
            if (isSoundOn)
            {
                isSoundPlaying = true;
            }
            UpdateEngine(0, 0);
        }

        //Gets a cue from the sound bank
        public static Cue getCue(String cueName)
        {
            return soundBank.GetCue(cueName);
        }

        #region Methods to get and reset the Menu Music
        public static Cue getMenuMusic()
        {
            return menuMusic;
        }
        public static void resetMenuMusic()
        {
            menuMusic = soundBank.GetCue(MENU_CUE);
        }
        #endregion

        #region Methods to get and reset the Mission Music
        public static Cue getMissionMusic()
        {
            return missionMusic;
        }
        public static void resetMissionMusic()
        {
            missionMusic = soundBank.GetCue(MISSION_CUE);
        }
        #endregion

        #region Methods to get, set, and reset the Engine Sounds
        public static Cue getEngineSounds()
        {
            return engineSounds;
        }
        public static void resetEngineSounds()
        {
            engineSounds = soundBank.GetCue(ENGINE_CUE);
        }
        public static void setEngineSounds(String cueName)
        {
            engineSounds = getCue(cueName);
        }
        #endregion

        /// <summary>
        /// Return a boolean for if music can be played
        /// </summary>
        /// <returns>isMusicPlaying</returns>
        public static Boolean getMusicPlaying()
        {
            return isMusicPlaying;
        }

        /// <summary>
        /// Set the state of music to On or Off
        /// </summary>
        /// <param name="isPlaying"></param>
        public static void setMusicPlaying(Boolean isPlaying)
        {
            isMusicPlaying = isPlaying;
            if (!isPlaying)
            {
                //Make sure music that is already playing stops
                if (menuMusic.IsPlaying)
                    menuMusic.Stop(AudioStopOptions.AsAuthored);
                if (missionMusic.IsPlaying)
                    missionMusic.Stop(AudioStopOptions.AsAuthored);
            }
        }

        /// <summary>
        /// Return boolean for if the sound is on
        /// </summary>
        /// <returns>isSoundPlaying</returns>
        public static Boolean getSoundOn()
        {
            return isSoundPlaying;
        }

        /// <summary>
        /// Set the state of sound to On of Off
        /// </summary>
        /// <param name="isPlaying"></param>
        public static void setSoundPlaying(Boolean isPlaying)
        {
            isSoundPlaying = isPlaying;
            if (!isPlaying)
            {
                //Make sure sounds that are already playing stop
                if (engineSounds.IsPlaying)
                    engineSounds.Stop(AudioStopOptions.AsAuthored);
            }
        }
    }
}