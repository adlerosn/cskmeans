package com.example.demo.classification;

public class ClusterPOJO {
    public int id;
    public PrevalencePOJO truePrevalence;
    public PrevalencePOJO compensatedPrevalence;

    public ClusterPOJO(int id, PrevalencePOJO truePrevalence, PrevalencePOJO compensatedPrevalence) {
        this.id = id;
        this.truePrevalence = truePrevalence;
        this.compensatedPrevalence = compensatedPrevalence;
    }
}
