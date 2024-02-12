package com.example.demo.classification;

public class KMeansOutlierDetector {
    public KMeansApplier kma2;
    public KMeansApplier kma;
    public double[] centroid;
    public double[][] centroids;
    public double sil;
    public double wss;
    public double o;

    public KMeansOutlierDetector(KMeansApplier kma, double[] centroid, double sil, double wss, double o) {
        this.kma = kma;
        this.centroid = centroid;
        this.sil = sil;
        this.wss = wss;
        this.o = o;
        _postInit();
    }

    public KMeansOutlierDetector(KMeansApplier kma, String centroid, String sil, String wss, String o) {
        this.kma = kma;
        this.centroid = Utils.parseFloat1D(centroid, ":");
        this.sil = Double.parseDouble(sil);
        this.wss = Double.parseDouble(wss);
        this.o = Double.parseDouble(o);
        _postInit();
    }

    public void _postInit() {
        centroids = new double[centroid.length][];
        for (int k = 0; k < centroids.length; k++)
            centroids[k] = new double[kma.centroids[0].length + 1];
        for (int k = 0; k < centroids.length; k++)
            centroids[k][kma.centroids[0].length] = centroid[k];
        for (int k = 0; k < centroids.length; k++)
            for (int f = 0; f < kma.centroids[0].length; f++)
                centroids[k][f] = kma.centroids[k][f];
        kma2 = new KMeansApplier(centroids);
    }

    public double judgeOne(double[] data) {
        return judgeMany(Utils.transpose(new double[][] { data }))[0];
    }

    public double[] judgeMany(double[][] data) {
        double[] distances = kma2.predictManyDebug(data).second;
        double adjWss = Math.max(0, Math.min(1, wss));
        double adjSil = Math.max(0, Math.min(1, sil));
        double ratSil = Math.max(0, Math.min(1, adjWss / Math.max(0.000000001, Math.pow(adjSil, o))));
        for (int s = 0; s < distances.length; s++)
            distances[s] = Math.sqrt(Math.max(0, distances[s] - adjWss));
        for (int s = 0; s < distances.length; s++)
            distances[s] /= Math.max(0.000000001, ratSil);
        return distances;
    }
}
