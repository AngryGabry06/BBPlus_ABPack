using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionAnimation : MonoBehaviour
{
    [SerializeField] private float frameRate = 12f;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 10;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = sortingOrder;
    }

    public void PlayAnimation(List<Sprite> sprites)
    {
        if (sprites == null || sprites.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(AnimateFrames(sprites));
    }

    private IEnumerator AnimateFrames(List<Sprite> sprites)
    {
        float timePerFrame = 1f / frameRate;

        for (int i = 0; i < sprites.Count; i++)
        {
            spriteRenderer.sprite = sprites[i];
            yield return new WaitForSeconds(timePerFrame);
        }

        Destroy(gameObject);
    }
}