package com.example.demo.classification;

public class ClassificationPOJO {
    public int clusterId;
    public double outlierScore;

    public ClassificationPOJO(int clusterId, double outlierScore) {
        this.clusterId = clusterId;
        this.outlierScore = outlierScore;
    }
}
