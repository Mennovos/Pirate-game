using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteBlink : MonoBehaviour
{
    [SerializeField, Min(0f)] private float blinkInterval = 0.5f;
    
    [Space]
    [SerializeField] private Sprite sprite1;
    [SerializeField] private Sprite sprite2;
    
    private Image img;
    
    private void Start()
    {
        TryGetComponent(out img);

        StartCoroutine(BlinkCoroutine());
    }

    private IEnumerator BlinkCoroutine()
    {
        while (true)
        {
            img.sprite = sprite1;
            yield return new WaitForSeconds(blinkInterval);
            img.sprite = sprite2;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
