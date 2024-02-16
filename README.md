# C# KMeans + SpringBoot3 API

This was a self-challenge during 2024's carnival holiday's weekend, and incremented to become an optional exercise for [an MBA in AI for Businesses](https://exame.com/faculdade/mba/mba-em-inteligencia-artificial-para-negocios) - proof-of-concept maturity.

This repository reimplements KMeans in plain C# in the form of a externally-schedulable job¹, and also a minimal effort to make it a Spring Boot 3 API². This also provides some form of outlier detection.

¹ Deployable via crontab, your internal corporate solution, AWS Batch, Azure Batch, GCP Preemptible VMs, Oracle Burstable Instances, or anything else that runs a commandline at a scheduled time. May require tweaking to fit your needs.<br>
² Deployable in your coporate TomCat, WildFly, Docker, Kubernetes, CloudFlare Workers, AWS Lambda, GCP Cloud Functions, or anything else you use to serve a Java Web API. May require tweaking to fit your needs.

## Example lifecycle

1.  Train:
    ```sh
    make
    ```
    It will produce an output like this:
    ```
    dotnet run --configuration Release
    Started         2/16/2024 1:13:00 AM (0.0136183s)
    Loaded          2/16/2024 1:13:10 AM (9.8201451s, DatasetLines=2671097)
    Shuffled        2/16/2024 1:13:10 AM (0.1334194s)
    Scaled          2/16/2024 1:13:10 AM (0.0696958s)
    > TrainCentroids=[0.32,0.49,0.52,0.5,0.56]
    SplitTTV        2/16/2024 1:13:13 AM (2.7777124s, Train=2569595, Test=48222, Validation=53280)
    10x Fits        2/16/2024 1:18:16 AM (302.8325479s)
    > WSSs=[0.23768,0.19774,0.17236,0.15302,0.13696,0.11922,0.11058,0.102,0.09685,0.09028]
    10x Tests       2/16/2024 1:18:16 AM (0.1831497s)
    > SILs=[0.2277,0.22823,0.21612,0.23022,0.23678,0.24369,0.24482,0.24524,0.2472,0.24839]
    > AGGs=[-0.00998,0.03049,0.04376,0.0772,0.09982,0.12447,0.13424,0.14324,0.15035,0.15811]
    10x Silhouettes 2/16/2024 1:39:22 AM (1266.4811917s)
    > OutlierScore(o=1.6, Min=0, Avg=0.4713820048875287, Max=0.8736922864125183)
    Scalers=[
        [32.5, 42.03755], [-124.49859, -114.1], [0, 6], [1, 366], [0, 86340]
    ]
    Centroids=[
        [0.16, 0.62, 0.77, 0.76, 0.72],
        [0.57, 0.28, 0.83, 0.76, 0.59],
        [0.57, 0.28, 0.83, 0.25, 0.57],
        [0.18, 0.61, 0.82, 0.6, 0.25],
        [0.17, 0.62, 0.27, 0.25, 0.29],
        [0.16, 0.63, 0.24, 0.79, 0.52],
        [0.16, 0.62, 0.76, 0.23, 0.65],
        [0.17, 0.62, 0.17, 0.3, 0.73],
        [0.57, 0.28, 0.27, 0.22, 0.58],
        [0.56, 0.29, 0.26, 0.74, 0.72],
        [0.56, 0.29, 0.28, 0.69, 0.27]
    ]
    DescaledCentroids=[
        [34.026008, -118.0514642, 4.62, 278.4, 62164.799999999996],
        [37.936403500000004, -121.5869848, 4.9799999999999995, 278.4, 50940.6],
        [37.936403500000004, -121.5869848, 4.9799999999999995, 92.25, 49213.799999999996],
        [34.216759, -118.1554501, 4.92, 220, 21585],
        [34.1213835, -118.0514642, 1.62, 92.25, 25038.6],
        [34.026008, -117.9474783, 1.44, 289.35, 44896.8],
        [34.026008, -118.0514642, 4.5600000000000005, 84.95, 56121],
        [34.1213835, -118.0514642, 1.02, 110.5, 63028.2],
        [37.936403500000004, -121.5869848, 1.62, 81.3, 50077.2],
        [37.841028, -121.4829989, 1.56, 271.1, 62164.799999999996],
        [37.841028, -121.4829989, 1.6800000000000002, 252.85, 23311.800000000003]
    ]
    Prevalence=[1,1,1,1,1,1,1,1,1,1,1]
    CompensatedPrevalence=[2,0,1,1,1,3,4,1,1,1,1]
    bestK=11 clusters
    WSS=0.09427714334438726
    Sil=0.24316338789249595
    Agg=0.14888624454810867
    Validation        2/16/2024 1:42:01 AM (158.4397816s)
    All Done!         2/16/2024 1:42:01 AM (0.000619s; Total=1740.7518809s)
    memusg: peak=3274104
    ```
2.  Run server:
    ```sh
    make runapi
    ```
3.  Visit with the browser: `http://localhost:8080`
4.  Submit the request:
    | Field | Value |
    | ----- | ----- |
    | latitude | 34.16449 |
    | longitude | -118.15798 |
    | date | 2009-01-14 |
    | time | 14:15:00 |
    ```sh
    curl -s 'http://localhost:8080/model?latitude=34.16449&longitude=-118.15798&date=2009-01-14&time=14%3A15%3A00' | jq
    ```
5.  See the response:
    ```json
    {
      "cluster": {
        "id": 6,
        "truePrevalence": {
          "id": 1,
          "label": "1-possible"
        },
        "compensatedPrevalence": {
          "id": 4,
          "label": "4-dead"
        }
      },
      "outlierScore": 0.5144809550353892
    }
    ```

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
|           | 3.2 GB | 1.1 GB⁴    | 3.2 GB       | 3.2 GB  |

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

The “youngest” item is 10 years old by the time this line got written. That's no innovation.

If you “innovate” using these technologies in your business, it's just a century worth of technical debts that you are removing from your outworn processes.

## License

The implementation is licensed under MIT-0, which basically means Public Domain.

## Links

- Canonical: https://git.adlerneves.com/adler/cskmeans
- Mirror: https://github.com/adlerosn/cskmeans
