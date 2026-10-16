using System;
using System.Collections.Generic;
using System.Text;
using MidiPlayerTK;
using MTM101BaldAPI.Reflection;

namespace bbplus.angrybirdspack
{
    public class MusicManaging
    {
        private MusicManager mMan;
        private MidiFilePlayer mPlayer;

        public MusicManaging()
        {
            mMan = Singleton<MusicManager>.Instance;
            mPlayer = (MidiFilePlayer)ReflectionHelpers.ReflectionGetVariable(mMan, "midiPlayer");
        }

        public static void PlayMidiMusic(string mFile)
        {
            var midiAsset = ABPackPlugin.birdModMan.Get<string>(mFile);
            Singleton<MusicManager>.Instance.PlayMidi(midiAsset, false);
        }

        public static void PlayLoopMidiMusic(string mFile)
        {
            var midiAsset = ABPackPlugin.birdModMan.Get<string>(mFile);
            Singleton<MusicManager>.Instance.PlayMidi(midiAsset, true);
        }

        public static void StopMidiMusic() { Singleton<MusicManager>.Instance.StopMidi(); }
    }
}
