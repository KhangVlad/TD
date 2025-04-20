
    using UnityEngine;
    using System.Collections;
    using DG.Tweening;

    public class Flag : MonoBehaviour
    {
        [SerializeField] private float animationDuration = 0.5f;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }

        public IEnumerator AnimateAppearance()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            // Fade in
            gameObject.SetActive(true);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
            yield return spriteRenderer.DOFade(1f, animationDuration).WaitForCompletion();

            // Hold
            yield return new WaitForSeconds(animationDuration);

            // Fade out
            yield return spriteRenderer.DOFade(0f, animationDuration).WaitForCompletion();
            gameObject.SetActive(false);
        }
    }
