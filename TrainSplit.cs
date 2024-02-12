using System;
using System.Linq;

public static class TrainSplit
{
    public static double[][] Shuffle(this double[][] data, int? randomStateSeed = null)
    {
        var randomizer = new Random(randomStateSeed ?? (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalNanoseconds);
        int[] scrambleTemplate = Enumerable.Range(0, data[0].Length).ToArray();
        randomizer.Shuffle(scrambleTemplate);
        var shuffled = data.Shape().Allocate2D<double>(0);
        for (int i = 0; i < data.Length; i++)
            for (int j = 0; j < data.Length; j++)
                shuffled[i][scrambleTemplate[j]] = data[i][j];
        return shuffled;
    }
    public static (double[][], double[][], double[][]) SplitTTV(this double[][] data, double ratioTrain, double ratioTest, double ratioValidation)
    {
        var features = data.Length;
        var nSamples = data[0].Length;
        int nTrain = (int)Math.Round(nSamples * ratioTrain / (ratioTrain + ratioTest + ratioValidation));
        int nTest = (int)Math.Round(nSamples * ratioTest / (ratioTrain + ratioTest + ratioValidation));
        int nVal = nSamples - (nTrain + nTest);
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
        return (dTrain, dTest, dVal);
    }
}