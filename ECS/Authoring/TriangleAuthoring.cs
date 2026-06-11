using Unity.Entities;
using UnityEngine;

public struct TriangleEntity : IComponentData, IEnableableComponent
{
    public Color hoverColor;
    public Color disabledColor;
    public Color selectedColor;
    public Color activeColor;
    public Color colorTarget;
}