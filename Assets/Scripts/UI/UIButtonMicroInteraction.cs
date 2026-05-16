using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BaslangicSeviye.SayiTahminOyunu.UI
{
    /// <summary>
    /// Butonlara hover ve tıklama sırasında küçük scale animasyonu verir.
    /// </summary>
    public class UIButtonMicroInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float normalScale = 1f;
        [SerializeField] private float hoverScale = 1.04f;
        [SerializeField] private float pressedScale = 0.97f;
        [SerializeField] private float animationDuration = 0.1f;

        private Coroutine scaleRoutine;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one * normalScale;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            AnimateScale(hoverScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            AnimateScale(normalScale);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            AnimateScale(pressedScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            AnimateScale(hoverScale);
        }

        private void AnimateScale(float targetScale)
        {
            if (rectTransform == null)
            {
                return;
            }

            if (scaleRoutine != null)
            {
                StopCoroutine(scaleRoutine);
            }

            scaleRoutine = StartCoroutine(ScaleRoutine(targetScale));
        }

        private IEnumerator ScaleRoutine(float targetScale)
        {
            Vector3 start = rectTransform.localScale;
            Vector3 target = Vector3.one * targetScale;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);
                rectTransform.localScale = Vector3.Lerp(start, target, t);
                yield return null;
            }

            rectTransform.localScale = target;
            scaleRoutine = null;
        }
    }
}
