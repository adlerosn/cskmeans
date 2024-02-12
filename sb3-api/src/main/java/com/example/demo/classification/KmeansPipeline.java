package com.example.demo.classification;

public class KmeansPipeline {
    public double[][] scales;
    public double[][] centroids;
    public double[] centroid;
    public double sil;
    public double wss;
    public double o;
    public DataScaler ds;
    public KMeansApplier kma;
    public KMeansOutlierDetector kmod;

    public KmeansPipeline(String scales, String centroids, String centroid, String sil, String wss, String o) {
        this.scales = Utils.parseFloat2D(scales, ";", ":");
        this.centroids = Utils.parseFloat2D(centroids, ";", ":");
        this.centroid = Utils.parseFloat1D(centroid, ":");
        this.sil = Double.parseDouble(sil);
        this.wss = Double.parseDouble(wss);
        this.o = Double.parseDouble(o);
        _postInit();
    }

    public KmeansPipeline(double[][] scales, double[][] centroids, double[] centroid, double sil, double wss,
            double o) {
        this.scales = scales;
        this.centroids = centroids;
        this.centroid = centroid;
        this.sil = sil;
        this.wss = wss;
        this.o = o;
        _postInit();
    }

    public void _postInit() {
        ds = new DataScaler(scales);
        kma = new KMeansApplier(centroids);
        kmod = new KMeansOutlierDetector(kma, centroid, sil, wss, o);
    }

    public ClassificationPOJO takeOne(double[] data) {
        return takeMany(Utils.transpose(new double[][] { data }))[0];
    }

    public ClassificationPOJO[] takeMany(double[][] data) {
        double[][] scaled = ds.scaleMany(data);
        int[] classes = kma.predictMany(scaled);
        double[] unfitness = kmod.judgeMany(scaled);
        ClassificationPOJO[] clfs = new ClassificationPOJO[classes.length];
        for (int s = 0; s < classes.length; s++)
            clfs[s] = new ClassificationPOJO(classes[s], unfitness[s]);
        return clfs;
    }
}
