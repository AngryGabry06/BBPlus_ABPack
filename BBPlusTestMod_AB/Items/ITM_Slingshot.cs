using System.Collections.Generic;
using UnityEngine;

namespace bbplus.angrybirdspack.Items
{
    public class ITM_Slingshot : Item
    {
        public int birdType = 0;

        public List<SoundObject> sndUse = new List<SoundObject>();
        public OBJ_Red redPrefab;

        public void PlayRandomSound(PropagatedAudioManager audioMgr, List<SoundObject> sounds) { if (audioMgr != null && sounds != null && sounds.Count > 0) { audioMgr.PlayRandomAudio(sounds.ToArray()); } }

        public override bool Use(PlayerManager pm)
        {
            PropagatedAudioManager playerAudio = pm.GetComponent<PropagatedAudioManager>();

            if (sndUse != null && sndUse.Count > 0) { PlayRandomSound(playerAudio, sndUse); }

            Vector3 spawnPosition = pm.transform.position + Vector3.up * 2f;
            Vector3 aimDirection = pm.cameraBase != null ? pm.cameraBase.transform.forward : pm.transform.forward;

            switch (birdType)
            {
                // Red
                case 0:
                    ExecuteRedAction(spawnPosition, aimDirection);
                    break;
                // Chuck
                case 1:
                    Debug.Log("Used Chuck.");
                    break;
                // Bomb
                case 2:
                    Debug.Log("Used Bomb.");
                    break;
                default:
                    Debug.LogWarning($"[ITM_Slingshot] Unrecognized birdType value: {birdType}");
                    break;
            }

            return true;
        }

        private void ExecuteRedAction(Vector3 spawnPosition, Vector3 aimDirection)
        {
            if (redPrefab != null)
            {
                OBJ_Red launchedRed = Instantiate(redPrefab, spawnPosition, Quaternion.LookRotation(aimDirection));
                launchedRed.gameObject.SetActive(true);
                launchedRed.Initialize(aimDirection);
            }
        }

        //private void ExecuteChuckAction(Vector3 spawnPosition, Vector3 aimDirection)
        //{
        // Chuck slingshot functionality goes here
        //}

        //private void ExecuteBombAction(Vector3 spawnPosition, Vector3 aimDirection)
        //{
        // Bomb slingshot functionality goes here
        //}
    }
}