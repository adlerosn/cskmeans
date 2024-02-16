using System;
using System.Collections.Generic;
using System.Linq;

public static class TrainSplit
{
    public static int[] GenerateScrambleTemplate(this int size, int? randomStateSeed = null)
    {
        var randomizer = new Random(randomStateSeed ?? (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalNanoseconds);
        int[] scrambleTemplate = Enumerable.Range(0, size).ToArray();
        randomizer.Shuffle(scrambleTemplate);
        return scrambleTemplate;
    }
    public static double[][] Shuffle(this int[] scrambleTemplate, double[][] data)
    {
        var shuffled = data.Shape().Allocate2D<double>(0);
        for (int i = 0; i < data.Length; i++)
            for (int j = 0; j < data.Length; j++)
                shuffled[i][scrambleTemplate[j]] = data[i][j];
        return shuffled;
    }
    public static T[] Shuffle<T>(this int[] scrambleTemplate, T[] vec)
    {
        var shuffled = vec.Shape().Allocate1D(vec[0]);
        for (int i = 0; i < vec.Length; i++)
            shuffled[scrambleTemplate[i]] = vec[i];
        return shuffled;
    }
    public static double[][] Shuffle(this double[][] data, int? randomStateSeed = null)
    {
        int[] scrambleTemplate = data[0].Length.GenerateScrambleTemplate(randomStateSeed);
        return scrambleTemplate.Shuffle(data);
    }
    public static (string[], double[][]) Shuffle(this (string[] ids, double[][] data) tpl, int? randomStateSeed = null)
    {
        int[] scrambleTemplate = tpl.data[0].Length.GenerateScrambleTemplate(randomStateSeed);
        return (
            scrambleTemplate.Shuffle(tpl.ids),
            scrambleTemplate.Shuffle(tpl.data)
        );
    }
    public static (string[], string?[], double[][]) Shuffle(this (string[] ids, string?[] labels, double[][] data) tpl, int? randomStateSeed = null)
    {
        int[] scrambleTemplate = tpl.data[0].Length.GenerateScrambleTemplate(randomStateSeed);
        return (
            scrambleTemplate.Shuffle(tpl.ids),
            scrambleTemplate.Shuffle(tpl.labels),
            scrambleTemplate.Shuffle(tpl.data)
        );
    }
    public static ((string[], string?[], double[][]), (string[], string?[], double[][]), (string[], string?[], double[][])) SplitTTVSimple(this (string[] ids, string?[] labels, double[][] data) tpl, double ratioTrain, double ratioTest, double ratioValidation)
    {
        if (tpl.ids.Length == 0)
            return (([], [], []), ([], [], []), ([], [], []));
        var (ids, labels, data) = tpl;
        var features = data.Length;
        var nSamples = data[0].Length;
        int nTrain = (int)Math.Round(nSamples * ratioTrain / (ratioTrain + ratioTest + ratioValidation));
        int nTest = (int)Math.Round(nSamples * ratioTest / (ratioTrain + ratioTest + ratioValidation));
        int nVal = nSamples - (nTrain + nTest);
        var iTrain = nTrain.Allocate1D("");
        var iTest = nTest.Allocate1D("");
        var iVal = nVal.Allocate1D("");
        var lTrain = nTrain.Allocate1D<string?>(null);
        var lTest = nTest.Allocate1D<string?>(null);
        var lVal = nVal.Allocate1D<string?>(null);
        var dTrain = (features, nTrain).Allocate2D<double>(0);
        var dTest = (features, nTest).Allocate2D<double>(0);
        var dVal = (features, nVal).Allocate2D<double>(0);
        for (int f = 0; f < features; f++)
            for (int s = 0; s < nTrain; s++)
                dTrain[f][s] = data[f][s];
        for (int f = 0; f < features; f++)
            for (int s = 0; s < nTest; s++)
                dTest[f][s] = data[f][s + nTrain];
        for (int f = 0; f < features; f++)
            for (int s = 0; s < nVal; s++)
                dVal[f][s] = data[f][s + nTrain + nTest];
        for (int s = 0; s < nTrain; s++)
            iTrain[s] = ids[s];
        for (int s = 0; s < nTest; s++)
            iTest[s] = ids[s + nTrain];
        for (int s = 0; s < nVal; s++)
            iVal[s] = ids[s + nTrain + nTest];
        for (int s = 0; s < nTrain; s++)
            lTrain[s] = labels[s];
        for (int s = 0; s < nTest; s++)
            lTest[s] = labels[s + nTrain];
        for (int s = 0; s < nVal; s++)
            lVal[s] = labels[s + nTrain + nTest];
        return (
            (iTrain, lTrain, dTrain),
            (iTest, lTest, dTest),
            (iVal, lVal, dVal)
        );
    }

    public static long Bucketize(this ((double[], double[]), int) minsmaxs, double[] point)
    {
        var ((mins, maxs), cspas) = minsmaxs;
        long b = 0;
        for (int f = 0; f < point.Length; f++)
            b += Math.Min(
                (long)Math.Floor(
                    (mins[f], maxs[f]).Scale(point[f])
                    * cspas
                ),
                cspas - 1
            ) * ((long)Math.Pow(cspas, f));
        return b;
    }

    public static ((string[], string?[], double[][]), (string[], string?[], double[][]), (string[], string?[], double[][])) MergeTTVBuckets(this ((string[], string?[], double[][]), (string[], string?[], double[][]), (string[], string?[], double[][]))[] buckets)
    {
        var nTr = buckets.Sum(x => x.Item1.Item1.Length);
        var nTe = buckets.Sum(x => x.Item2.Item1.Length);
        var nVa = buckets.Sum(x => x.Item3.Item1.Length);
        if (nTr == 0 && nTe == 0 && nVa == 0)
            return (([], [], []), ([], [], []), ([], [], []));
        int nF = buckets.Select(b => new int[] { b.Item1.Item3.Length, b.Item2.Item3.Length, b.Item3.Item3.Length }.Max()).Max();
        if (nF == 0)
            return (([], [], []), ([], [], []), ([], [], []));
        var dsTr = (nTr.Allocate1D(""), nTr.Allocate1D<string?>(null), (nF, nTr).Allocate2D<double>(0));
        var dsTe = (nTe.Allocate1D(""), nTe.Allocate1D<string?>(null), (nF, nTe).Allocate2D<double>(0));
        var dsVa = (nVa.Allocate1D(""), nVa.Allocate1D<string?>(null), (nF, nVa).Allocate2D<double>(0));
        int cTr = 0;
        int cTe = 0;
        int cVa = 0;
        foreach (var (bTr, bTe, bVa) in buckets)
        {
            for (int s = 0; s < bTr.Item1.Length; s++, cTr++)
            {
                dsTr.Item1[cTr] = bTr.Item1[s];
                dsTr.Item2[cTr] = bTr.Item2[s];
                for (int f = 0; f < nF; f++)
                    dsTr.Item3[f][cTr] = bTr.Item3[f][s];
            }
            for (int s = 0; s < bTe.Item1.Length; s++, cTe++)
            {
                dsTe.Item1[cTe] = bTe.Item1[s];
                dsTe.Item2[cTe] = bTe.Item2[s];
                for (int f = 0; f < nF; f++)
                    dsTe.Item3[f][cTe] = bTe.Item3[f][s];
            }
            for (int s = 0; s < bVa.Item1.Length; s++, cVa++)
            {
                dsVa.Item1[cVa] = bVa.Item1[s];
                dsVa.Item2[cVa] = bVa.Item2[s];
                for (int f = 0; f < nF; f++)
                    dsVa.Item3[f][cVa] = bVa.Item3[f][s];
            }
        }
        return (dsTr, dsTe, dsVa);
    }

    public static ((string[], string?[], double[][]), (string[], string?[], double[][]), (string[], string?[], double[][])) SplitTTV(this (string[] ids, string?[] labels, double[][] data) tpl, double ratioTrain, double ratioTest, double ratioValidation, int cartesianStatificationPerAxis = 1, bool labelStatification = false)
    {
        if (tpl.ids.Length == 0)
            return (([], [], []), ([], [], []), ([], [], []));
        else if (cartesianStatificationPerAxis > 1)
        {
            var datat = tpl.data.Transpose();
            var minsmaxs = (tpl.data.Select(x => x.Min()).ToArray(), tpl.data.Select(x => x.Max()).ToArray());
            var bucketDef = (minsmaxs, cartesianStatificationPerAxis);
            var nSamples = datat.Length;
            var nCartesianBuckets = (long)Math.Pow(cartesianStatificationPerAxis, tpl.data.Length);
            var cartesianBuckets = new (List<string>, List<string?>, List<double>[])[nCartesianBuckets];
            var cartesianTTVBuckets = new ((string[], string?[], double[][]), (string[], string?[], double[][]), (string[], string?[], double[][]))[nCartesianBuckets];
            for (int b = 0; b < nCartesianBuckets; b++)
            {
                var datas = new List<double>[tpl.data.Length];
                for (int f = 0; f < tpl.data.Length; f++)
                    datas[f] = [];
                cartesianBuckets[b] = ([], [], datas);
            }
            for (int s = 0; s < nSamples; s++)
            {
                var (bi, bl, bd) = cartesianBuckets[bucketDef.Bucketize(datat[s])];
                bi.Add(tpl.ids[s]);
                bl.Add(tpl.labels[s]);
                for (int f = 0; f < tpl.data.Length; f++)
                    bd[f].Add(datat[s][f]);
            }
            for (int b = 0; b < nCartesianBuckets; b++)
            {
                var (bi, bl, bd) = cartesianBuckets[b];
                cartesianTTVBuckets[b] = (
                    bi.ToArray(),
                    bl.ToArray(),
                    bd.Select(x => x.ToArray()).ToArray()
                ).SplitTTV(ratioTrain, ratioTest, ratioValidation, 1, labelStatification);
            }
            return cartesianTTVBuckets.MergeTTVBuckets();
        }
        else if (labelStatification)
        {
            var datat = tpl.data.Transpose();
            var nSamples = datat.Length;
            var labels = tpl.labels.Distinct().ToArray();
            var nLabelBuckets = labels.Length;
            var labelBuckets = new (List<string>, List<string?>, List<double>[])[nLabelBuckets];
            var labelTTVBuckets = new ((string[], string?[], double[][]), (string[], string?[], double[][]), (string[], string?[], double[][]))[nLabelBuckets];
            for (int b = 0; b < nLabelBuckets; b++)
            {
                var datas = new List<double>[tpl.data.Length];
                for (int f = 0; f < tpl.data.Length; f++)
                    datas[f] = [];
                labelBuckets[b] = ([], [], datas);
            }
            for (int s = 0; s < nSamples; s++)
            {
                var ix = labels.IndexOf(tpl.labels[s]);
                var (bi, bl, bd) = labelBuckets[ix];
                bi.Add(tpl.ids[s]);
                bl.Add(tpl.labels[s]);
                for (int f = 0; f < tpl.data.Length; f++)
                    bd[f].Add(datat[s][f]);
            }
            for (int b = 0; b < nLabelBuckets; b++)
            {
                var (bi, bl, bd) = labelBuckets[b];
                labelTTVBuckets[b] = (
                    bi.ToArray(),
                    bl.ToArray(),
                    bd.Select(x => x.ToArray()).ToArray()
                ).SplitTTV(ratioTrain, ratioTest, ratioValidation, cartesianStatificationPerAxis, false);
            }
            return labelTTVBuckets.MergeTTVBuckets();
        }
        else
            return tpl.SplitTTVSimple(ratioTrain, ratioTest, ratioValidation);
    }
}