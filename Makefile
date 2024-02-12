.PHONY: run
run: bin/Release/net8.0/cskmeans
	dotnet run --configuration Release

bin/Release/net8.0/cskmeans: *.cs
	dotnet build --configuration Release

.PHONY: clean
clean:
	dotnet clean

.PHONY: obliterate
obliterate:
	rm -rf bin obj

.PHONY: run
runapi:
	cd sb3-api; ./gradlew bootRun

.PHONY: zip
zip: cskmeans.zip
	@echo -ne ''

cskmeans.zip: *.csproj *.sln *.cs *.sql *.txt Makefile sb3-api/gradlew.bat sb3-api/build.gradle sb3-api/settings.gradle sb3-api/HELP.md sb3-api/gradlew sb3-api/.gitignore sb3-api/gradle/wrapper/* sb3-api/src/main/java/com/example/demo/* sb3-api/src/test/java/com/example/demo/* sb3-api/src/main/java/com/example/demo/classification/* sb3-api/src/main/resources/*
	@rm -f cskmeans.zip
	zip cskmeans.zip *.csproj *.sln *.cs *.sql *.txt Makefile  sb3-api/gradlew.bat sb3-api/build.gradle sb3-api/settings.gradle sb3-api/HELP.md sb3-api/gradlew sb3-api/.gitignore sb3-api/gradle/wrapper/* sb3-api/src/main/java/com/example/demo/* sb3-api/src/test/java/com/example/demo/* sb3-api/src/main/java/com/example/demo/classification/* sb3-api/src/main/resources/*
