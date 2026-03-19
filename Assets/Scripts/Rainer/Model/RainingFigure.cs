using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RainingFigure : MonoBehaviour
{
    public event Action<RainingFigure, Collision> OnCollisionEntering;

    private void Awake()
    {
        if (TryGetComponent<Renderer>(out Renderer renderer) == false)
        {
            Debug.LogError($"No renderer found on {nameof(RainingFigure)} object. Creating fallback renderer", this);
            renderer = this.AddComponent<Renderer>();
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        OnCollisionEntering?.Invoke(this, collision);
    }

    public void SetColor(Color color)
    {
        if (TryGetComponent<Renderer>(out var renderer))
            renderer.material.color = color;
        else
            throw new NullReferenceException();
    }

    public void ChangeColor()
    {
        SetColor(UserUtilities.GenerateRandomRGBColor());
    }

    public bool TryGetColor(out Color color)
    {
        color = default;

        if (TryGetComponent<Renderer>(out Renderer renderer))
        {
            color = renderer.material.color;
            return true;
        }

        return false;
    }
}
