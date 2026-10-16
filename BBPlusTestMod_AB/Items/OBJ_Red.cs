using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bbplus.angrybirdspack.Items
{
    public class OBJ_Red : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float baseSpeed = 40f;
        public float stunDuration = 19f;

        [Header("References & Audio")]
        public List<SoundObject> sndRedLaunch = new List<SoundObject>();
        public PropagatedAudioManager audioManager;

        private Vector3 direction;
        private LayerMask wallMask = LayerMask.GetMask("Block");
        private HashSet<NPC> hitNPCs = new HashSet<NPC>();

        public void Initialize(Vector3 launchDirection)
        {
            direction = launchDirection.normalized;
            direction.y = 0f;

            if (sndRedLaunch != null && sndRedLaunch.Count > 0) { PlayRandomSound(sndRedLaunch); }
        }

        public void PlayRandomSound(List<SoundObject> sounds)
        {
            if (audioManager != null && sounds != null && sounds.Count > 0) { audioManager.PlayRandomAudio(sounds.ToArray()); }
        }

        private void Update()
        {
            float moveDistance = baseSpeed * Time.deltaTime;

            if (Physics.Raycast(transform.position, direction, moveDistance + 0.5f, wallMask))
            {
                Destroy(gameObject);
                return;
            }

            IntVector2 currentPos = IntVector2.GetGridPosition(transform.position);
            IntVector2 nextPos = IntVector2.GetGridPosition(transform.position + direction * moveDistance);

            if (BaseGameManager.Instance != null && BaseGameManager.Instance.Ec != null)
            {
                EnvironmentController ec = BaseGameManager.Instance.Ec;
                if (!ec.ContainsCoordinates(currentPos) || !ec.ContainsCoordinates(nextPos))
                {
                    Destroy(gameObject);
                    return;
                }
            }

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.2f, ABPackPlugin.onlyNpcLayers);
            foreach (var col in hitColliders)
            {
                NPC npc = col.GetComponent<NPC>();
                if (npc != null && !hitNPCs.Contains(npc))
                {
                    hitNPCs.Add(npc);
                    StunNPC(npc);
                }
            }

            transform.position += direction * moveDistance;
        }

        private void StunNPC(NPC npc) { npc.StartCoroutine(StunRoutine(npc)); }

        private IEnumerator StunRoutine(NPC npc)
        {
            MovementModifier moveMod = new MovementModifier(Vector3.zero, 0f);

            if (npc != null && npc.Navigator != null && npc.Navigator.Am != null) { npc.Navigator.Am.moveMods.Add(moveMod); }

            yield return new WaitForSeconds(stunDuration);

            if (npc != null && npc.Navigator != null && npc.Navigator.Am != null) { npc.Navigator.Am.moveMods.Remove(moveMod); }
        }
    }
}