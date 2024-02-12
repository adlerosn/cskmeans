package com.example.demo;

import com.example.demo.classification.*;

import java.nio.file.Files;
import java.nio.file.Paths;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class DemoController {
	@GetMapping("/")
	public String index() {
		return "<!doctype html>"
				+ "<html><body><h1>Model API form</h1><form action=\"/model\" method=\"GET\">"
				+ "<label for=\"latitude\">latitude</label><input id=\"latitude\" name=\"latitude\" value=\"34.16449\"><br>"
				+ "<label for=\"longitude\">longitude</label><input id=\"longitude\" name=\"longitude\" value=\"-118.15798\"><br>"
				+ "<label for=\"date\">date</label><input id=\"date\" name=\"date\" value=\"2009-01-14\"><br>"
				+ "<label for=\"time\">time</label><input id=\"time\" name=\"time\" value=\"14:15:00\"><br>"
				+ "<button>Classify</button>"
				+ "</form></body></html>";
	}

	@GetMapping("/model")
	public ClassificationPOJO index(
			@RequestParam double latitude,
			@RequestParam double longitude,
			@RequestParam(name = "date") String dateString,
			@RequestParam(name = "time") String timeString) throws Throwable {
		// Input format converting
		double timeN = Utils.secondsSinceMidnight(timeString);
		double dayN = Utils.daysSinceEpoch(dateString);
		double[] inputData = new double[] { latitude, longitude, dayN, timeN };
		// Load model from some source (replace with DB connection here)
		String modelSerialized = Files.readString(Paths.get("../model-as-a-db-row.txt"));
		String[] modelSerializedParts = modelSerialized.split("@");
		// Instance the pipeline
		KmeansPipeline pipeline = new KmeansPipeline(
				modelSerializedParts[modelSerializedParts.length - 2],
				modelSerializedParts[modelSerializedParts.length - 1],
				modelSerializedParts[modelSerializedParts.length - 3],
				modelSerializedParts[modelSerializedParts.length - 5],
				modelSerializedParts[modelSerializedParts.length - 6],
				modelSerializedParts[modelSerializedParts.length - 7]);
		// Return cluster id
		return pipeline.takeOne(inputData);
	}
}
