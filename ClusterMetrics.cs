using System;
using System.Linq;

public static class ClusterMetrics
{
    // Easiest math-heavy source I could find:
    // https://en.wikipedia.org/wiki/Silhouette_(clustering)
    public static double Silhouette(this (int[] clusters, double[][] data) pair, string hint = "?")
    {
        var clusters = pair.clusters;
        var datat = pair.data.Transpose();
        var K = clusters.Max() + 1;
        var nSamples = clusters.Length;
        var kSize = new int[K];
        var ais = new double[nSamples];
        var bis = new double[nSamples];
        var sis = new double[nSamples];
        for (int s = 0; s < nSamples; s++)
            kSize[clusters[s]] += 1;
        for (int i = 0; i < nSamples; i++)
            if (kSize[clusters[i]] > 1)
            {
                double distSum = 0;
                for (int j = 0; j < nSamples; j++)
                    if (i != j && clusters[i] == clusters[j])
                        distSum += (datat[i], datat[j]).EuclideanDistance();
                ais[i] = distSum / (kSize[clusters[i]] - 1);
                if (i % 100 == 0)
                {
                    Console.Write("\x1b[2K");
                    Console.Write("> Silhouette " + hint + " a[" + i + "]" + " " + Math.Round(100d / 2 * i / nSamples, 0) + "% " + DateTime.Now);
                    Console.Write("\r");
                }
            }
        for (int i = 0; i < nSamples; i++)
            if (kSize[clusters[i]] > 1)
            {
                int I = clusters[i];
                double[] ds = new double[K];
                for (int j = 0; j < nSamples; j++)
                    if (clusters[j] != I)
                        ds[clusters[j]] += (datat[i], datat[j]).EuclideanDistance();
                double[] JS = new double[K];
                for (int k = 0; k < K; k++)
                    JS[k] = ds[k] / kSize[k];
                JS[I] = double.PositiveInfinity;
                bis[i] = JS.Min();
                if (i % 100 == 0)
                {
                    Console.Write("\x1b[2K");
                    Console.Write("> Silhouette " + hint + " b[" + i + "]" + " " + Math.Round(100d / 2 * i / nSamples + (100 / 2), 0) + "% " + DateTime.Now);
                    Console.Write("\r");
                }
            }
        for (int i = 0; i < nSamples; i++)
        {
            if (ais[i] == bis[i])
                sis[i] = 0;
            else if (ais[i] < bis[i])
                sis[i] = 1 - ais[i] / bis[i];
            else if (ais[i] > bis[i])
                sis[i] = bis[i] / ais[i] - 1;
        }
        Console.Write("\r");
        Console.Write("\x1b[2K");
        // if (sis.Sum() / sis.Length == 0)
        // {
        //     Console.WriteLine("> ais=[" + string.Join(",", ais) + "]");
        //     Console.WriteLine("> bis=[" + string.Join(",", bis) + "]");
        //     Console.WriteLine("> sis=[" + string.Join(",", sis) + "]");
        // }
        return sis.Sum() / sis.Length;
    }

    public static double DistancesToWSS(this double[] distances) => distances.Sum(x => x * x) / distances.Length;

    public static double OutlierScore(this (double distance2, double wss, double sil) inpdata, double param0) =>
        OutlierScores(([inpdata.distance2], inpdata.wss, inpdata.sil), param0)[0];
    public static double[] OutlierScores(this (double[] distance2, double wss, double sil) inpdata, double param0)
    {
        var (distances, wss, sil) = inpdata;
        distances = distances.Copy1D();
        double adjWss = Math.Max(0, Math.Min(1, wss));
        double adjSil = Math.Max(0, Math.Min(1, sil));
        double ratSil = Math.Max(0, Math.Min(1, adjWss / Math.Max(0.000000001, Math.Pow(adjSil, param0))));
        for (int s = 0; s < distances.Length; s++)
            distances[s] = Math.Sqrt(Math.Max(0, distances[s] - adjWss));
        for (int s = 0; s < distances.Length; s++)
            distances[s] /= Math.Max(0.000000001, ratSil);
        return distances;
    }
}