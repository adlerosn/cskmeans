package com.example.demo.classification;

public class ClassificationPOJO {
    public ClusterPOJO cluster;
    public double outlierScore;

    public ClassificationPOJO(ClusterPOJO cluster, double outlierScore) {
        this.cluster = cluster;
        this.outlierScore = outlierScore;
    }
}
