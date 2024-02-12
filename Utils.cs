using System;
using System.Globalization;
using System.Linq;

public static class Utils
{
    public static T[] Allocate1D<T>(this int dim, T initial)
    {
        var m = new T[dim];
        for (int i = 0; i < dim; i++)
            m[i] = initial;
        return m;
    }
    public static T[][] Allocate2D<T>(this (int first, int second) dim, T initial)
    {
        var m = new T[dim.first][];
        for (int i = 0; i < dim.first; i++)
            m[i] = dim.second.Allocate1D(initial);
        return m;
    }
    public static T[][][] Allocate3D<T>(this (int first, int second, int third) dim, T initial)
    {
        var m = new T[dim.first][][];
        for (int i = 0; i < dim.first; i++)
            m[i] = (dim.second, dim.third).Allocate2D(initial);
        return m;
    }

    public static int Shape<T>(this T[] matrix) => matrix.Length;
    public static (int, int) Shape<T>(this T[][] matrix) => (matrix.Length, matrix[0].Length);
    public static (int, int, int) Shape<T>(this T[][][] matrix) => (matrix.Length, matrix[0].Length, matrix[0][0].Length);
    public static T[] Copy1D<T>(this T[] matrix)
    {
        var t = matrix.Length.Allocate1D(matrix[0]);
        for (int i = 0; i < matrix.Length; i++)
            t[i] = matrix[i];
        return t;
    }
    public static T[][] Copy2D<T>(this T[][] matrix)
    {
        var t = (matrix.Length, matrix[0].Length).Allocate2D(matrix[0][0]);
        for (int i = 0; i < matrix.Length; i++)
            for (int j = 0; j < matrix[0].Length; j++)
                t[i][j] = matrix[i][j];
        return t;
    }
    public static T[][] Transpose<T>(this T[][] matrix)
    {
        var t = (matrix[0].Length, matrix.Length).Allocate2D<T>(matrix[0][0]);
        for (int i = 0; i < matrix.Length; i++)
            for (int j = 0; j < matrix[0].Length; j++)
                t[j][i] = matrix[i][j];
        return t;
    }

    public static double EuclideanDistance(this (double[] a, double[] b) points)
    {
        var t = new double[points.a.Length];
        for (int i = 0; i < t.Length; i++)
            t[i] += Math.Pow(points.a[i] - points.b[i], 2);
        return Math.Sqrt(t.Sum());
    }

    public static int IndexOfMax(this double[] a)
    {
        double m = double.NegativeInfinity;
        int x = -1;
        for (int i = 0; i < a.Length; i++)
            if (a[i] > m)
                (x, m) = (i, a[i]);
        return x;
    }

    public static int IndexOfMin(this double[] a)
    {
        double m = double.PositiveInfinity;
        int x = -1;
        for (int i = 0; i < a.Length; i++)
            if (a[i] < m)
                (x, m) = (i, a[i]);
        return x;
    }

    public static string ToIsoString(this DateTime a) => a.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
    public static string ToJSON(this double[] x) => "[" + string.Join(",", x) + "]";
    public static string ToJSON(this double[][] x) => "[" + string.Join(",", x.Select(ToJSON)) + "]";
}