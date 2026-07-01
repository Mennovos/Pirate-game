using UnityEngine;

public class ChildSizeFitter : MonoBehaviour
{
    [SerializeField] private Vector2 margin;
    [SerializeField] private RectTransform child;

    private RectTransform self;

    private void Start()
    {
        self = (RectTransform)transform;
    }

    private void Update()
    {
        self.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, child.rect.width + margin.x);
        self.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, child.rect.height + margin.y);
    }
}
