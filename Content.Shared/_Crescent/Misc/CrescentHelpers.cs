using System.Numerics;

public static class CrescentHelpers
{
    #region Geometry

    /// <summary>
    /// Returns value of angle in 0~2pi range
    /// </summary>
    public static Angle AngNormal(Angle x)
    {
        x = x.Reduced();
        if (x < 0)
            x += 2 * Math.PI;
        return x;
    }

    public static bool AngInSector(Angle x, Angle start, Angle width)
    {
        Angle dist = Angle.ShortestDistance(start, x);
        return Math.Sign(width) == Math.Sign(dist) ? Math.Abs(width) >= Math.Abs(dist) : Math.Abs(width) >= Math.Tau - Math.Abs(dist);
    }

    public static bool AngSectorsOverlap(Angle start0, Angle width0, Angle start1, Angle width1)
    {
        Angle end0 = AngNormal(start0 + width0);
        Angle end1 = AngNormal(start1 + width1);
        return AngInSector(start0, start1, width1) || AngInSector(start1, start0, width0) ||
               AngInSector(end0, start1, width1) || AngInSector(end1, start0, width0);
    }

    /// <summary>
    /// Sectors must have positive widths
    /// </summary>
    public static (Angle, Angle) AngCombinedSector(Angle start0, Angle width0, Angle start1, Angle width1)
    {
        Angle startlow, widthlow, starthigh, widthhigh, disthigh;
        (startlow, widthlow, starthigh, widthhigh) = AngInSector(start1, start0, width0) ? (start0, width0, start1, width1) : (start1, width1, start0, width0);
        disthigh = AngNormal(starthigh + widthhigh) > startlow ? starthigh + widthhigh - startlow : Math.Tau - startlow + AngNormal(starthigh + widthhigh);

        return (startlow, Math.Min(Math.Max(widthlow, disthigh), Math.Tau));
    }

    public static bool RectCircleIntersect(Box2 rect, Vector2 circPos, float circRadius)
    {
        Vector2 delta = circPos - rect.Center;

        if (delta.X > rect.Width / 2 + circRadius || delta.Y > rect.Height / 2 + circRadius)
            return false;

        if (delta.X < rect.Width / 2 || delta.Y < rect.Height / 2)
            return true;

        delta.X -= rect.Width / 2;
        delta.Y -= rect.Height / 2;

        return delta.Length() < circRadius;
    }

    #endregion
}