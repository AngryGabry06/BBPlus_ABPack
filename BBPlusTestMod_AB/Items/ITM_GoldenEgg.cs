using System;
using System.Collections.Generic;
using UnityEngine;

namespace bbplus.angrybirdspack.Items
{
    public class ITM_GoldenEgg : Item
    {
        public override bool Use(PlayerManager pm)
        {
            if (MainGameManager.Instance != null && MainGameManager.Instance.FoundNotebooks >= MainGameManager.Instance.NotebookTotal)
            {
                int randomYtp = UnityEngine.Random.Range(750, 1200);

                Singleton<CoreGameManager>.Instance.AddPoints(randomYtp, 0, true);

                return true;
            }

            return false;
        }
    }
}