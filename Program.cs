using System;
using System.IO;
using System.Linq;

public class Program
{
    public static void Main()
    {
        var boot = DateTime.Now;
        var kBgn = 2;
        var kQty = 10;
        var mDig = 5;
        var oTests = 100;
        var start = DateTime.Now;
        Console.WriteLine("Started         " + start.ToString() + " (" + (start - boot).TotalSeconds + "s)");
        var (rawIdentities, rawLabels, rawData) = SWITRS.LoadDataSet();
        // identities[lin] ; labels[lin] ; values[col][lin]
        string?[] labels = rawLabels.Distinct().Order().ToArray();
        string[] labelsNN = labels.WhereNotNull().ToArray();
        var loaded = DateTime.Now;
        Console.WriteLine("Loaded          " + loaded.ToString() + " (" + (loaded - start).TotalSeconds + "s, DatasetLines=" + rawData[0].Length + ")");
        var (shuffledRawIdentities, shuffledRawLabels, shuffledRawData) = (rawIdentities, rawLabels, rawData).Shuffle(42);
        var shuffled = DateTime.Now;
        Console.WriteLine("Shuffled        " + shuffled.ToString() + " (" + (shuffled - loaded).TotalSeconds + "s)");
        var (scalers, scaledData) = rawData.Scaled(Scalers.MaxMinScale);
        var scaled = DateTime.Now;
        Console.WriteLine("Scaled          " + scaled.ToString() + " (" + (scaled - shuffled).TotalSeconds + "s)");
        var (
            (idTrain, lbTrain, dsTrain),
            (idTest, lbTest, dsTest),
            (idValidation, lbValidation, dsValidation)
        ) = (shuffledRawIdentities, shuffledRawLabels, scaledData).SplitTTV(96, 2, 2, 8, true);
        if (dsTrain.Length <= 0 || idTrain.Length <= 0)
            throw new Exception("Reduce the number of SplitTTV subdivisions");
        var dsCentroid = KMeans.FromData(1, dsTrain).Centroids[0];
        Console.WriteLine("> TrainCentroids=[" + string.Join(",", dsCentroid) + "]");
        var splitTTV = DateTime.Now;
        Console.WriteLine("SplitTTV        " + splitTTV.ToString() + " (" + (splitTTV - scaled).TotalSeconds + "s, Train=" + dsTrain[0].Length + ", Test=" + dsTest[0].Length + ", Validation=" + dsValidation[0].Length + ")");
        var kmeanss = new KMeans[kQty];
        for (int k = 0; k < kQty; k++)
            kmeanss[k] = new KMeans(kBgn + k, dsTrain.Length);
        for (int k = 0; k < kQty; k++)
            kmeanss[k].Fit(dsTrain);
        var fit = DateTime.Now;
        Console.WriteLine("" + kQty + "x Fits        " + fit.ToString() + " (" + (fit - splitTTV).TotalSeconds + "s)");
        var labeleds = new int[kQty][];
        var distances = new double[kQty][];
        for (int k = 0; k < kQty; k++)
            (labeleds[k], distances[k]) = kmeanss[k].PredictWithDebug(dsTest);
        Console.WriteLine("> WSSs=[" + string.Join(",", distances.Select(x => Math.Round(x.DistancesToWSS(), mDig))) + "]");
        var test = DateTime.Now;
        Console.WriteLine("" + kQty + "x Tests       " + test.ToString() + " (" + (test - fit).TotalSeconds + "s)");
        var silhouettes = new double[kQty];
        for (int k = 0; k < kQty; k++)
            silhouettes[k] = Math.Round((labeleds[k], dsTest).Silhouette("K=" + (k + kBgn)), mDig);
        Console.WriteLine("> SILs=[" + string.Join(",", silhouettes) + "]");
        var composite = new double[kQty];
        for (int k = 0; k < kQty; k++)
            composite[k] += Math.Round(silhouettes[k] - Math.Round(distances[k].DistancesToWSS(), mDig), mDig);
        Console.WriteLine("> AGGs=[" + string.Join(",", composite) + "]");
        var silhouette = DateTime.Now;
        Console.WriteLine("" + kQty + "x Silhouettes " + silhouette.ToString() + " (" + (silhouette - test).TotalSeconds + "s)");
        var bestk = composite.IndexOfMax();
        var bestKMeans = kmeanss[bestk];
        var (valClusters, valWssV) = bestKMeans.PredictWithDebug(dsValidation);
        int[] prevalenceTrue = (valClusters, lbValidation, labelsNN).PrevalenceByCluster();
        int[] prevalenceCompensated = (valClusters, lbValidation, labelsNN).PrevalenceByCluster(true);
        var valWss = valWssV.DistancesToWSS();
        var valSil = (valClusters, dsValidation).Silhouette("validation");
        var outlierScoress = new double[oTests][];
        var outlierScoressd = new double[oTests];
        for (int o = 0; o < oTests; o++)
            outlierScoress[o] = (
                bestKMeans.CloneWithExtraCentroid(dsCentroid).PredictWithDebug(dsValidation).Item2,
                valWss, valSil).OutlierScores(o / 10d);
        for (int o = 0; o < oTests; o++)
            outlierScoressd[o] = Math.Abs(.5d - outlierScoress[o].Average());
        var oMin = outlierScoressd.IndexOfMin();
        var outlierScoreMax = outlierScoress[oMin].Max();
        var outlierScoreMin = outlierScoress[oMin].Min();
        var outlierScoreAvg = outlierScoress[oMin].Average();
        Console.WriteLine("> OutlierScore(o=" + (oMin / 10d) + ", Min=" + outlierScoreMin + ", Avg=" + outlierScoreAvg + ", Max=" + outlierScoreMax + ")");
        Console.Write("Scalers=");
        scalers.PrettyPrintScalers();
        Console.Write("Centroids=");
        bestKMeans.PrettyPrintCentroid();
        Console.Write("DescaledCentroids=");
        bestKMeans.PrettyPrintDescaledCentroid(scalers);
        Console.WriteLine("Prevalence=[" + string.Join(",", prevalenceTrue) + "]");
        Console.WriteLine("CompensatedPrevalence=[" + string.Join(",", prevalenceCompensated) + "]");
        Console.WriteLine("bestK=" + (bestk + kBgn) + " clusters");
        Console.WriteLine("WSS=" + valWss);
        Console.WriteLine("Sil=" + valSil);
        Console.WriteLine("Agg=" + (valSil - valWss));
        var validation = DateTime.Now;
        Console.WriteLine("Validation        " + validation.ToString() + " (" + (validation - silhouette).TotalSeconds + "s)");
        File.WriteAllText(
            "model-as-a-db-row.txt",
            start.ToIsoString() + "@" + validation.ToIsoString() + "@" + (kBgn + bestk) + "@" +
            string.Join(";", labelsNN) + "@" + string.Join(";", prevalenceTrue) + "@" +
            string.Join(";", prevalenceCompensated) + "@" + (oMin / 10d) + "@" +
            valWss + "@" + valSil + "@" + (valSil - valWss) + "@" + string.Join(":", dsCentroid) + "@" +
            string.Join(";", scalers.Select(x => x.Item1 + ":" + x.Item2)) + "@" +
            string.Join(";", bestKMeans.Centroids.Select(x => string.Join(":", x)))
        );
        var done = DateTime.Now;
        Console.WriteLine("All Done!         " + done.ToString() + " (" + (done - validation).TotalSeconds + "s; Total=" + (done - boot).TotalSeconds + "s)");
    }
}
