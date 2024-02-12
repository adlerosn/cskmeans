# C# KMeans + SpringBoot3 API

This was a self-challenge during 2024's carnival holiday's weekend.

This repository reimplements KMeans in plain C# in the form of a externally-schedulable job (crontab, or your internal corporate solution), and also an Spring Boot 3 API. This also provides some form of outlier detection.

## Performance

Single-threaded only, but there are places where optimization is possible.

| Stage | 7900X @ 5.70GHz | i5-11135G7 @ 2.40GHz |
| ----- | --------------- | -------------------- |

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

If you “innovate” using these technologies in your business, it's just a century of technical debts that you are removing from your outworn processes.

## License

The implementation is licensed under MIT-0, which basically means Public Domain.

## Links

- Canonical: https://git.adlerneves.com/adler/cksmeans
- Mirror: https://github.com/adlerosn/cksmeans
