package com.example.demo.classification;

import java.text.SimpleDateFormat;
import java.util.GregorianCalendar;

public class Utils {
    public static GregorianCalendar gregorianCalendarFromString(String isoDateWithoutTime) {
        String[] dateParts = isoDateWithoutTime.split("-");
        GregorianCalendar dateCal = new GregorianCalendar(
                Integer.parseInt(dateParts[0]),
                Integer.parseInt(dateParts[1]),
                Integer.parseInt(dateParts[2]));
        return dateCal;
    }

    public static long daysSinceEpoch(String isoDateWithoutTime) {
        return gregorianCalendarFromString(isoDateWithoutTime).getTimeInMillis() / (24 * 3600 * 1000);
    }

    public static int daysSinceStartOfYear(String isoDateWithoutTime) {
        return Integer.parseInt(new SimpleDateFormat("D").format(gregorianCalendarFromString(isoDateWithoutTime)
                .getTime()));
    }

    public static int daysSinceStartOfWeek(String isoDateWithoutTime) {
        return Integer.parseInt(new SimpleDateFormat("u").format(gregorianCalendarFromString(isoDateWithoutTime)
                .getTime())) % 7;
    }

    public static double secondsSinceMidnight(String isoTimeWithoutDateNorTimezone) {
        double timeN = 0;
        String[] timeParts = isoTimeWithoutDateNorTimezone.split(":");
        timeN += Integer.parseInt(timeParts[0]) * 3600;
        if (timeParts.length > 1)
            timeN += Integer.parseInt(timeParts[1]) * 60;
        if (timeParts.length > 2)
            timeN += Double.parseDouble(timeParts[2]);
        return timeN;
    }

    public static double[] parseFloat1D(String data, String sep1) {
        String[] f = data.split(sep1);
        double[] v = new double[f.length];
        for (int i = 0; i < f.length; i++) {
            v[i] = Double.parseDouble(f[i]);
        }
        return v;
    }

    public static int[] parseInt1D(String data, String sep1) {
        String[] f = data.split(sep1);
        int[] v = new int[f.length];
        for (int i = 0; i < f.length; i++) {
            v[i] = Integer.parseInt(f[i]);
        }
        return v;
    }

    public static double[][] parseFloat2D(String data, String sep1, String sep2) {
        String[] f = data.split(sep1);
        double[][] v = new double[f.length][];
        for (int i = 0; i < f.length; i++) {
            v[i] = parseFloat1D(f[i], sep2);
        }
        return v;
    }

    public static double[][] transpose(double[][] in) {
        double[][] out = new double[in[0].length][];
        for (int j = 0; j < in[0].length; j++)
            out[j] = new double[in.length];
        for (int i = 0; i < in.length; i++)
            for (int j = 0; j < in[0].length; j++)
                out[j][i] = in[i][j];
        return out;
    }
}
