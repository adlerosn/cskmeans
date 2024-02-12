using System;
using System.Linq;

public static class Scalers
{
    public static ((double, double)[], double[][]) Scaled(this double[][] data, Func<double[], (double, double)> scaler)
    {
        var scalers = data.Select(scaler).ToArray();
        var scaledData = data.Shape().Allocate2D<double>(0);
        for (int f = 0; f < scaledData.Length; f++)
            for (int l = 0; l < scaledData[0].Length; l++)
                scaledData[f][l] = scalers[f].Scale(data[f][l]);
        return (scalers, scaledData);
    }
    public static double[][] Descaled(this double[][] data, (double, double)[] scalers)
    {
        var descaledData = data.Shape().Allocate2D<double>(0);
        for (int f = 0; f < descaledData.Length; f++)
            for (int l = 0; l < descaledData[0].Length; l++)
                descaledData[f][l] = scalers[f].Descale(data[f][l]);
        return descaledData;
    }
    public static double Scale(this (double min, double max) scale, double point) => (point - scale.min) / (scale.max - scale.min);
    public static double Descale(this (double min, double max) scale, double point) => point * (scale.max - scale.min) + scale.min;

    public static (double, double) MaxMinScale(this double[] data) => (data.Min(), data.Max());
    public static (double, double) StandardScale(this double[] data)
    {
        double avg = data.Sum() / data.Length;
        double diffsum2 = data.Select((d) => Math.Pow(d - avg, 2)).Sum();
        double variance = diffsum2 / data.Length;
        double stddev = Math.Sqrt(variance);
        double[] x = [avg - (stddev / 2), avg + (stddev / 2)];
        if (x[0] > x[1])
        {
            (x[0], x[1]) = (x[1], x[0]);
        }
        return (x[0], x[1]);
    }
    public static void PrettyPrintScalers(this (double min, double max)[] scalers)
    {
        Console.WriteLine("[\n    " + string.Join(", ", scalers.Select(p => "[" + p.min + ", " + p.max + "]")) + "\n]");
    }
}