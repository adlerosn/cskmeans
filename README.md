# C# KMeans + SpringBoot3 API

This was a self-challenge during 2024's carnival holiday's weekend - proof-of-concept maturity.

This repository reimplements KMeans in plain C# in the form of a externally-schedulable job¹, and also a minimal effort to make it a Spring Boot 3 API². This also provides some form of outlier detection.

¹ Deployable via crontab, your internal corporate solution, AWS Batch, Azure Batch, GCP Preemptible VMs, Oracle Burstable Instances, or anything else that runs a commandline at a scheduled time. May require tweaking to fit your needs.<br>
² Deployable in your coporate TomCat, WildFly, Docker, Kubernetes, CloudFlare Workers, AWS Lambda, GCP Cloud Functions, or anything else you use to serve a Java Web API. May require tweaking to fit your needs.

## Performance

The dataset contains 2,671,097 lines by 4 columns stored as double (8 bytes), which is at least 85,475,104 bytes (81.5 MiB).

The single-threaded³ C# code performance was evaluated on these systems:

| Hardware      | 7900X              | MacMini M2      | Dell 3511 i5      | i7-4790            |
| ------------- | ------------------ | --------------- | ----------------- | ------------------ |
| Form factor   | Desktop            | Desktop         | Laptop            | Desktop            |
| Processor     | Ryzen 9 7900X      | Apple M2        | Intel i5-1135G7   | Intel i7-4790      |
| Cache L3      | 64MB               | 8MB             | 8MB               | 8MB                |
| Max Frequency | 4.70 GHz           | 3.48 GHz        | 2.40 GHz          | 3.60 GHz           |
| Max Turbo     | 5.70 GHz           | 3.48 GHz        | 4.20 GHz          | 4.00 GHz           |
| Storage Type  | SSD                | SSD             | SSD               | HDD                |
| RAM           | 4×32GB @ DDR5-4000 | 16GB @ 6400MT/s | 2×8GB @ DDR4-2666 | 2×8 GB @ DDR3-1366 |
| Kernel        | Linux 6.7.4-zen1   | Darwin 23.0.0   | Linux 6.7.0-zen3  | Linux 6.6.8-arch-1 |

Therefore, we should should see some memory busses saturated.

³ There are parallelization paths, and they are explicit by their prefix “10x”, but I believe that in a corporate environment there would be many jobs running in parallel and the predictability of a stable resource allocation would have a greater importance.

### RAM resource

Memory was measured by watching the numbers on the resource monitor on each system. Under Linux, that means `htop` and on Mac that means `Activity Monitor`.

| RAM usage | 7900X  | MacMini M2 | Dell 3511 i5 | i7-4790 |
| --------- | ------ | ---------- | ------------ | ------- |
| Debug     | 2.8 GB | ?          | ?            | ?       |
| Release   | 1.9 GB | 1.1 GB⁴    | 1.9 GB       | 1.9 GB  |

⁴ Apple tries cheating by compressing processes' memory, but it backfires when it needs decompressing data in order to use it.

### Processor resource

These timings are measured by the own program.

| Stage           | 7900X        | MacMini M2  | Dell 3511 i5 | i7-4790      |
| --------------- | ------------ | ----------- | ------------ | ------------ |
| Started         | 0.0145117    | 0.030513    | 0.019999     | 0.0237424    |
| Loaded          | 3.5465618    | 6.516409    | 7.7220067    | 8.1680365    |
| Shuffled        | 0.0226329    | 0.02862     | 0.0641004    | 0.048993     |
| Scaled          | 0.0583177    | 0.070994    | 0.0663303    | 0.0903944    |
| SplitTTV        | 0.7055008    | 1.027031    | 1.2220999    | 1.3453164    |
| 10x Fits        | 194.6585428  | 237.454974  | 309.4348727  | 405.7263568  |
| 10x Tests       | 0.1762073    | 0.203529    | 0.275389     | 0.3948622    |
| 10x Silhouettes | 1395.8565106 | 1499.822873 | 2118.9745899 | 2955.7305509 |
| Validation      | 141.7715882  | 151.862509  | 217.2271935  | 299.9685991  |
| All Done!       | 0.0005635    | 0.001758    | 0.0006435    | 0.0006692    |
| Total           | 1736.8109373 | 1897.01921  | 2655.0072249 | 3671.4975209 |

Therefore, we can confirm that L3 cache size and memory bandwidth are more important than CPU “speed”.

## Innovations

None. This is inherently no innovation, as:

- Math-wise:

  - KMeans is an old algorithm, known [since at least 1956](https://stats.stackexchange.com/a/82740);
  - WSS (Within-Cluster Sum of Squares) is just a fancy name for a specific kind of variance, which the latter exists [since at least 1923](https://link.springer.com/chapter/10.1007/978-1-4612-6079-0_4);
  - Silhouette, the newest of it all, was [proposed in 1987](<https://en.wikipedia.org/wiki/Silhouette_(clustering)>).

- Programming-wise:
  - C# is an old programming language, available [since 2002](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-10-1);
  - Spring Boot is an old web framework, available [since 2014](https://spring.io/blog/2014/04/01/spring-boot-1-0-ga-released);
  - Java is an old programming language, available [since 1996](https://en.wikipedia.org/wiki/Java_version_history#Release_table).

The “youngest” item is 10 years old by the time this line is written. That's no innovation.

If you “innovate” using these technologies in your business, it's just a century of technical debts that you are removing from your outworn processes.

## License

The implementation is licensed under MIT-0, which basically means Public Domain.

## Links

- Canonical: https://git.adlerneves.com/adler/cskmeans
- Mirror: https://github.com/adlerosn/cskmeans
