package com.example.demo.classification;

public class KMeansApplier {
    public double[][] centroids;

    public KMeansApplier(String centroids) {
        this.centroids = Utils.parseFloat2D(centroids, ";", ":");
    }

    public KMeansApplier(double[][] centroids) {
        this.centroids = centroids;
    }

    public int predictOne(double[] data) {
        return predictMany(Utils.transpose(new double[][] { data }))[0];
    }

    public int[] predictMany(double[][] data) {
        return predictManyDebug(data).first;
    }

    public Pair<int[], double[]> predictManyDebug(double[][] data) { // data[col][lin]
        double[][] dist2 = new double[centroids.length][];
        for (int k = 0; k < centroids.length; k++)
            dist2[k] = new double[data[0].length];
        for (int k = 0; k < centroids.length; k++)
            for (int f = 0; f < data.length; f++)
                for (int s = 0; s < data[0].length; s++)
                    dist2[k][s] += Math.pow(data[f][s] - centroids[k][f], 2);
        int[] c = new int[data[0].length];
        for (int k = 1; k < centroids.length; k++)
            for (int s = 0; s < data[0].length; s++)
                if (dist2[c[s]][s] > dist2[k][s])
                    c[s] = k;
        double[] dist2f = new double[data[0].length];
        for (int s = 0; s < data[0].length; s++)
            dist2f[s] = dist2[c[s]][s];
        return new Pair<int[], double[]>(c, dist2f);
    }
}
