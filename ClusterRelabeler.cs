using System;
using System.Collections;
using System.Linq;

public static class ClusterRelabeler
{
    public static int[] PrevalenceByCluster(this (int[] clusters, string?[] label, string[] labels) tpl, bool compensate = false)
    {
        var (clusters, label, labels) = tpl;
        var nClusters = clusters.Max() + 1;
        var nLabels = labels.Length;
        int[][] clusterLabelFrequency = (nClusters, nLabels).Allocate2D(0);
        for (int s = 0; s < clusters.Length; s++)
            if (label[s] != null)
                clusterLabelFrequency[clusters[s]][labels.IndexOf(label[s])]++;
        int[] clusterLabel = nClusters.Allocate1D(0);
        for (int k = 0; k < nClusters; k++)
            clusterLabel[k] = clusterLabelFrequency[k].IndexOfMax();
        if (!compensate)
            return clusterLabel;
        int[] clusterFrequency = clusterLabelFrequency.Select(x => x.Sum()).ToArray();
        int[] labelFrequency = clusterLabelFrequency.Transpose().Select(x => x.Sum()).ToArray();
        bool[] clusterHeld = nClusters.Allocate1D(false);
        var clusterLabelDistinct = clusterLabel.Distinct().ToArray();
        if (clusters.Distinct().Count() > labels.Length) // don't violate the pidgeonhole principle
            while (clusterLabelDistinct.Length < labels.Length)
            {
                int[] absents = Enumerable.Range(0, nLabels).Where(x => !clusterLabel.Contains(x)).ToArray();
                int leastFrequentAbscense = absents.MinBy(x => labelFrequency[x]);
                double[] maxFreqClusters = clusterLabelFrequency.Select(x => ((double)x[leastFrequentAbscense]) / labelFrequency[leastFrequentAbscense]).ToArray();
                for (int i = 0; i < nClusters; i++)
                    if (clusterHeld[i])
                        maxFreqClusters[i] = double.NegativeInfinity;
                int maxFreqCluster = maxFreqClusters.IndexOfMax();
                clusterHeld[maxFreqCluster] = true;
                clusterLabel[maxFreqCluster] = leastFrequentAbscense;
                clusterLabelDistinct = clusterLabel.Distinct().ToArray();
            }
        return clusterLabel;
    }
}