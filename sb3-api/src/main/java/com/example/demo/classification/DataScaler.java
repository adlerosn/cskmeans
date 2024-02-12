package com.example.demo.classification;

public class DataScaler {
    public double[][] scales;

    public DataScaler(String scales) {
        this.scales = Utils.parseFloat2D(scales, ";", ":");
    }

    public DataScaler(double[][] scales) {
        this.scales = scales;
    }

    public double[] scaleOne(double[] data) {
        return Utils.transpose(scaleMany(Utils.transpose(new double[][] { data })))[0];
    }

    public double[][] scaleMany(double[][] data) { // data[col][lin]
        double[][] scaledData = new double[scales.length][];
        for (int i = 0; i < scales.length; i++)
            scaledData[i] = new double[data[0].length];
        for (int i = 0; i < scales.length; i++)
            for (int j = 0; j < data[0].length; j++)
                scaledData[i][j] = (data[i][j] - scales[i][0]) / (scales[i][1] - scales[i][0]);
        return scaledData;
    }
}
