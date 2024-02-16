using System;
using System.Linq;

public class KMeans
{
    public static int Precision = 2;
    public int K { get; set; }
    public int Features { get; set; }
    public double[][] Centroids { get; set; }

    public static KMeans FromCentroids(double[][] centroids) => new KMeans(centroids.Length, centroids[0].Length) { Centroids = centroids };
    public static KMeans FromData(int k, double[][] data) => new KMeans(k, data.Length).Fit(data);

    public KMeans(int k, int features)
    {
        K = k;
        Features = features;
        Centroids = [];
        _ColdInit();
    }

    public KMeans Clone() => FromCentroids(_DuplicateCentroids());
    public KMeans CloneWithExtraCentroid(double[] extraCentroid) => CloneWithExtraCentroids([extraCentroid]);
    public KMeans CloneWithExtraCentroids(double[][] extraCentroids)
    {
        var cloned = _DuplicateCentroids();
        var (x, y) = Centroids.Shape();
        var newCentroids = (x + extraCentroids.Length, y).Allocate2D<double>(0);
        for (int i = 0; i < x; i++)
            newCentroids[i] = cloned[i];
        for (int i = x; i < x + extraCentroids.Length; i++)
            newCentroids[i] = extraCentroids[i - x];
        return FromCentroids(newCentroids);
    }

    protected void _ColdInit()
    {
        Centroids = _GenerateEmptyCentroids();
    }

    protected double[][] _GenerateEmptyCentroids() => (K, Features).Allocate2D<double>(0);

    protected double[][] _DuplicateCentroids()
    {
        var centroids = _GenerateEmptyCentroids();
        foreach (int k in Enumerable.Range(0, K))
            foreach (int f in Enumerable.Range(0, Features))
                centroids[k][f] = Centroids[k][f];
        return centroids;
    }

    protected bool _CentroidsIsEqualsTo(double[][] centroids)
    {
        foreach (int k in Enumerable.Range(0, K))
            foreach (int f in Enumerable.Range(0, Features))
                if (centroids[k][f] != Centroids[k][f])
                    return false;
        return true;
    }

    public KMeans Fit(double[][] data)
    {
        // data[col][lin]
        // datat[lin][col]
        var datat = data.Transpose();
        _ColdInit();
        int nSamples = datat.Length;
        // candidateCentroids[k][col]
        var candidateCentroids = _GenerateEmptyCentroids();
        if (K == 1)
            for (int f = 0; f < Features; f++)
                candidateCentroids[0][f] = data[f].Sum() / nSamples;
        else
        {
            for (int f = 0; f < Features; f++)
                candidateCentroids[0][f] = datat[0][f];
            int sample = 0;
            for (int k = 0; k < K; k++)
            {
                while (
                    k > 0 &&
                    Enumerable.Range(0, k).Any( // se qualquer linha anterior for igual à selecionada
                        p => Enumerable.Range(0, Features).All(f => candidateCentroids[p][f] == datat[sample][f])
                    )
                )
                {
                    sample++; // passa para a próxima linha
                    if (sample >= nSamples)
                        throw new IndexOutOfRangeException("sample >= nSamples");
                }
                for (int f = 0; f < Features; f++)
                    candidateCentroids[k][f] = data[f][sample];
                sample++; // essa linha foi usada... usar a próxima
                if (sample >= nSamples)
                    throw new IndexOutOfRangeException("sample >= nSamples");
            }
        } // fim do escopo de sample
        int loopCount = 0;
        double prevMinDist = 0;
        do
        {
            // candidateCentroids[k][col]
            Centroids = candidateCentroids;
            // remember: don't reuse candidateCentroids reference; the reference ownership was transfered into Centroids
            // data[col][lin]
            // distances2t[k][lin]
            var distances2t = (K, data[0].Length).Allocate2D<double>(0);
            for (int k = 0; k < K; k++)
                for (int f = 0; f < Features; f++)
                    for (int s = 0; s < nSamples; s++)
                        distances2t[k][s] += Math.Pow(data[f][s] - Centroids[k][f], 2);
            // distances2[lin][k]
            var distances2 = distances2t.Transpose();
            double minDistances = 0;
            // clusters[lin]
            var clusters = new int[nSamples];
            for (int s = 0; s < nSamples; s++)
                for (int k = 1; k < K; k++)
                    if (distances2[s][k] < distances2[s][clusters[s]])
                        clusters[s] = k;
            for (int s = 0; s < nSamples; s++)
                minDistances += distances2[s][clusters[s]];
            if (prevMinDist == 0) prevMinDist = minDistances;
            var clusterCount = new int[K];
            for (int s = 0; s < nSamples; s++)
                clusterCount[clusters[s]]++;
            // aquiring new reference into candidateCentroids; now it's safe to use it
            candidateCentroids = _GenerateEmptyCentroids();
            // candidateCentroids[k][col]
            for (int f = 0; f < Features; f++)
                for (int s = 0; s < nSamples; s++)
                    candidateCentroids[clusters[s]][f] += data[f][s];
            for (int k = 0; k < K; k++)
                for (int f = 0; f < Features; f++)
                    candidateCentroids[k][f] /= clusterCount[k];
            for (int k = 0; k < K; k++)
                for (int f = 0; f < Features; f++)
                    candidateCentroids[k][f] = Math.Round(candidateCentroids[k][f], Precision);
            loopCount++;
            Console.Write("\x1b[2K");
            //Console.WriteLine("[\n    " + string.Join(",\n    ", candidateCentroids.Select(centroid => "[" + string.Join(", ", centroid) + "]")) + "\n]");
            Console.Write("> Fitting K=" + K + " Loop #" + loopCount + " " + DateTime.Now + " sum(dist)=" + minDistances + " delta=" + (prevMinDist - minDistances));
            Console.Write("\r");
            if ((prevMinDist - minDistances) < 0)
                break;
            prevMinDist = minDistances;
        } while (!_CentroidsIsEqualsTo(candidateCentroids));
        Console.Write("\x1b[2K");
        //Console.WriteLine("[\n    " + string.Join(",\n    ", Centroids.Select(centroid => "[" + string.Join(", ", centroid) + "]")) + "\n]");
        Console.Write("> Fitting Converged!");
        Console.Write("\r");
        Console.Write("\x1b[2K");
        return this;
    }

    public int[] Predict(double[][] data) => PredictWithDebug(data).Item1;
    public (int[], double[]) PredictWithDebug(double[][] data)
    {
        var nSamples = data[0].Length;
        var clusters = new int[nSamples];
        var distances2 = (nSamples, K).Allocate2D<double>(0);
        for (int f = 0; f < Features; f++)
            for (int s = 0; s < nSamples; s++)
                for (int k = 0; k < K; k++)
                    distances2[s][k] += Math.Pow(data[f][s] - Centroids[k][f], 2);
        // https://miro.medium.com/v2/resize:fit:552/1*bgpKrYZIVBuDirYk0JMnGg.png
        // If offline, go to quicklatex.com and render the formula: $$\sum_{i=1}^{m}{(x_i-c_i)^2}$$
        var distances = new double[nSamples];
        for (int s = 0; s < nSamples; s++)
        {
            clusters[s] = 0;
            double d = distances2[s][0];
            for (int k = 1; k < K; k++)
                if (distances2[s][k] < d)
                {
                    d = distances2[s][k];
                    clusters[s] = k;
                }
            distances[s] = Math.Sqrt(d);
        }
        return (clusters, distances);
    }

    public void PrettyPrintCentroid()
    {
        Console.WriteLine("[\n    " + string.Join(",\n    ", Centroids.Select(centroid => "[" + string.Join(", ", centroid) + "]")) + "\n]");
    }

    public void PrettyPrintDescaledCentroid((double, double)[] scalers)
    {
        Console.WriteLine("[\n    " + string.Join(",\n    ", Centroids.Select(centroid => "[" + string.Join(", ", centroid.Zip(scalers).Select(tpl => tpl.Second.Descale(tpl.First))) + "]")) + "\n]");
    }
}