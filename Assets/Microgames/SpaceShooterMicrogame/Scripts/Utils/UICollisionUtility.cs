using UnityEngine;

public static class UICollisionUtility
{
    public static Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] c = new Vector3[4];
        rt.GetWorldCorners(c);
        return new Rect(c[0].x, c[0].y, c[2].x - c[0].x, c[2].y - c[0].y);
    }

    public static bool Overlap(RectTransform a, RectTransform b)
    {
        return GetWorldRect(a).Overlaps(GetWorldRect(b));
    }
}
