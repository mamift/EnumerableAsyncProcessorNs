# EnumerableAsyncProcessor for .NET Standard 2

This is a .NET Standard 2 backport of the very good https://github.com/thomhurst/EnumerableAsyncProcessor project. This project uses the same API, (so all public classes, types and method signatures are the same) but changes the root namespace to `EnumerableAsyncProcessor.Ns`, and should be usable on any .NET Standard 2 platform, such as .NET Framework 4.6.2+, Mono 5.4+ and .NET Core 2.1+. If you do need to switch out the original library with this one, ensure that the original namespaces `TomLonghurst.EnumerableAsyncProcessor.*` are replaced with `EnumerableAsyncProcessor.Ns.*` ones (the 'Ns' is meant to stand for .NET Standard).

## Installation
This library requires a .NET Standard 2.0 compatible-platform. See this [Microsoft documentation page for more information](https://docs.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-1-0) about which .NET implementations support .NET Standard.

Install via Nuget:

```
Install-Package EnumerableAsyncProcessor.Ns
```

## License
Like the original project, this is also licensed under the Apache License, Version 2.

## Development notes
As the [original project](https://github.com/thomhurst/EnumerableAsyncProcessor) is built using .NET 6, this backport to .NET Standard 2 uses the following libraries as polyfills to compensate for missing .NET 6 BCL APIs:
 - [Microsoft.Bcl.AsyncInterfaces](https://www.nuget.org/packages/Microsoft.Bcl.AsyncInterfaces)
 - [AsyncEnumerator](https://github.com/Dasync/AsyncEnumerable)
 - [morelinq](https://github.com/morelinq/MoreLINQ)
 - [System.Collections.Immutable](https://www.nuget.org/packages/System.Collections.Immutable/)
 - A custom built-in non-generic `TaskCompletionSource` class that inherits from `System.Threading.Tasks.TaskCompletionSource<T>` (where the generic type parameter is closed to resolve to `object`).

NOTE: If you use this same library in another library project that targets .NET Standard 2, please see the [example](https://github.com/Dasync/AsyncEnumerable#example-1-demonstrates-usage-only) in the [AsyncEnumerator project's](https://github.com/Dasync/AsyncEnumerable) README to properly consume `IAsyncEnumerable<T>` (as your C# language version will be set to 7.3 and the regular `await foreach` syntax won't be available).

### Unit tests

While this library targets .NET Standard 2, the unit tests have been copied over from the original project and it too targets .NET 6. Also as of v1.0.0, all the unit tests copied verbatim also pass in this project!

### Repo code is a copy, not a fork!

All the code in this repo is a copy, as of commit [7ca69d80864827a092c9e9ae0c1b8fa481a15f03](https://github.com/thomhurst/EnumerableAsyncProcessor/commit/7ca69d80864827a092c9e9ae0c1b8fa481a15f03) from 18 Mar 2022. 

The reason this is a copy is that any improvements to the original project code cannot be automatically merged without having to manually refactor for missing/different .NET 6 APIs. If there's any change or improvement from the original project you wish to see in this version, please raise an issue, or better yet, fork this repo and try raising a PR!

---

Beyond this point is the original README from the original project as of this [commit](https://github.com/thomhurst/EnumerableAsyncProcessor/commit/7ca69d80864827a092c9e9ae0c1b8fa481a15f03).

---

## Rate Limited Parallel Processor

**Types**  
| Type                                                        | Source Object | Return Object | Method 1            | Method 2           |
|--------------------------------------------------|---------------|---------------|--------------------| ------------------ |
| `RateLimitedParallelAsyncProcessor`                         | ❌             | ❌             | `.WithExecutionCount(int)` | `.ForEachAsync(delegate)` |
| `RateLimitedParallelAsyncProcessor<TInput>`                | ✔             | ❌             | `.WithItems(IEnumerable<TInput>)` | `.ForEachAsync(delegate)` |
| `ResultRateLimitedParallelAsyncProcessor<TOutput>`          | ❌             | ✔             | `.WithExecutionCount(int)` | `.SelectAsync(delegate)`  |
| `ResultRateLimitedParallelAsyncProcessor<TInput, TOutput>` | ✔             | ✔             | `.WithItems(IEnumerable<TInput>)` | `.SelectAsync(delegate)`  |

**How it works**  
Processes your Asynchronous Tasks in Parallel, but honouring the limit that you set. As one finishes, another will start. 

E.g. If you set a limit of 100, only 100 should ever run at any one time

This is a hybrid between Parallel Processor and Batch Processor (see below) - Trying to address the caveats of both. Increasing the speed of batching, but not overwhelming the system by using full parallelisation.

**Usage**  
```csharp
var ids = Enumerable.Range(0, 5000).ToList();

// SelectAsync for if you want to return something
var results = await AsyncProcessorBuilder.WithItems(ids) // Or Extension Method: await ids.ToAsyncProcessorBuilder()
        .SelectAsync(id => DoSomethingAndReturnSomethingAsync(id), CancellationToken.None)
        .ProcessInParallel(levelOfParallelism: 100);

// ForEachAsync for when you have nothing to return
await AsyncProcessorBuilder.WithItems(ids) // Or Extension Method: await ids.ToAsyncProcessorBuilder()
        .ForEachAsync(id => DoSomethingAsync(id), CancellationToken.None) 
        .ProcessInParallel(levelOfParallelism: 100);
```

## Timed Rate Limited Parallel Processor (e.g. Limit RPS)

**Types**  
| Type                                                        | Source Object | Return Object | Method 1            | Method 2           |
|--------------------------------------------------|---------------|---------------|--------------------| ------------------ |
| `TimedRateLimitedParallelAsyncProcessor`                         | ❌             | ❌             | `.WithExecutionCount(int)` | `.ForEachAsync(delegate)` |
| `TimedRateLimitedParallelAsyncProcessor<TInput>`                | ✔             | ❌             | `.WithItems(IEnumerable<TInput>)` | `.ForEachAsync(delegate)` |
| `ResultTimedRateLimitedParallelAsyncProcessor<TOutput>`          | ❌             | ✔             | `.WithExecutionCount(int)` | `.SelectAsync(delegate)`  |
| `ResultTimedRateLimitedParallelAsyncProcessor<TInput, TOutput>` | ✔             | ✔             | `.WithItems(IEnumerable<TInput>)` | `.SelectAsync(delegate)`  |

**How it works**  
Processes your Asynchronous Tasks in Parallel, but honouring the limit that you set over the timespan that you set. As one finishes, another will start, unless you've hit the maximum allowed for the current timespan duration. 

E.g. If you set a limit of 100, and a timespan of 1 second, only 100 operation should ever run at any one time over the course of a second. If the operation finishes sooner than a second (or your provided timespan), it'll wait and then start the next operation once that timespan has elapsed. 

This is useful in scenarios where, for example, you have an API but it has a request per second limit

**Usage**  
```csharp
var ids = Enumerable.Range(0, 5000).ToList();

// SelectAsync for if you want to return something
var results = await AsyncProcessorBuilder.WithItems(ids) // Or Extension Method: await ids.ToAsyncProcessorBuilder()
        .SelectAsync(id => DoSomethingAndReturnSomethingAsync(id), CancellationToken.None)
        .ProcessInParallel(levelOfParallelism: 100, TimeSpan.FromSeconds(1));

// ForEachAsync for when you have nothing to return
await AsyncProcessorBuilder.WithItems(ids) // Or Extension Method: await ids.ToAsyncProcessorBuilder()
        .ForEachAsync(id => DoSomethingAsync(id), CancellationToken.None) 
        .ProcessInParallel(levelOfParallelism: 100, TimeSpan.FromSeconds(1));
```

**Caveats**  
-   If your operations take longer than your provided TimeSpan, you probably won't get your desired throughput. This processor ensures you don't go over your rate limit, but will not increase parallel execution if you're below it.

## One At A Time

**Types**  

| Type                                               | Source Object | Return Object | Method 1            | Method 2           |
|--------------------------------------------------|---------------|---------------|--------------------| ------------------ |
| `OneAtATimeAsyncProcessor`                         | ❌             | ❌             | `.WithExecutionCount(int)` | `.ForEachAsync(delegate)` |
| `OneAtATimeAsyncProcessor<TInput>`                | ✔             | ❌             | `.WithItems(IEnumerable<TInput>)` | `.ForEachAsync(delegate)` |
| `ResultOneAtATimeAsyncProcessor<TOutput>`          | ❌             | ✔             | `.WithExecutionCount(int)` | `.SelectAsync(delegate)`  |
| `ResultOneAtATimeAsyncProcessor<TInput, TOutput>` | ✔             | ✔             | `.WithItems(IEnumerable<TInput>)` | `.SelectAsync(delegate)`  |

**How it works**  
Processes your Asynchronous Tasks One at a Time. Only one will ever progress at a time. As one finishes, another will start

**Usage**  
```csharp
var ids = Enumerable.Range(0, 5000).ToList();

// SelectAsync for if you want to return something
var results = await AsyncProcessorBuilder.WithItems(ids) // Or Extension Method: await ids.ToAsyncProcessorBuilder()
        .SelectAsync(id => DoSomethingAndReturnSomethingAsync(id), CancellationToken.None)
        .ProcessOneAtATime();

// ForEachAsync for when you have nothing to return
await AsyncProcessorBuilder.WithItems(ids) // Or Extension Method: await ids.ToAsyncProcessorBuilder()
        .ForEachAsync(id => DoSomethingAsync(id), CancellationToken.None) 
        .ProcessOneAtATime();
```

**Caveats**  
-   Slowest method

## Batch

**Types**  
| Type                                          | Source Object | Return Object | Method 1           | Method 2           |
|--------------------------------------------------|---------------|---------------|--------------------| ------------------ |
| `BatchAsyncProcessor`                         | ❌             | ❌             | `.WithExecutionCount(int)` | `.ForEachAsync(delegate)` |
| `BatchAsyncProcessor<TInput>`                | ✔             | ❌             | `.WithItems(IEnumerable<TInput>)` | `.ForEachAsync(delegate)` |
| `ResultBatchAsyncProcessor<TOutput>`          | ❌             | ✔             | `.WithExecutionCount(int)` | `.SelectAsync(delegate)`  |
| `ResultBatchAsyncProcessor<TInput, TOutput>` | ✔             | ✔             | `.WithItems(IEnumerable<TInput>)` | `.SelectAsync(delegate)`  |

**How it works**  
Processes your Asynchronous Tasks in Batches. The next batch will not start until every Task in previous batch has finished

**Usage**  
```csharp
var ids = Enumerable.Range(0, 5000).ToList();

// SelectAsync for if you want to return something
var results = await AsyncProcessorBuilder.WithItems(ids) // Or Extension Method: await ids.ToAsyncProcessorBuilder()
        .SelectAsync(id => DoSomethingAndReturnSomethingAsync(id), CancellationToken.None)
        .ProcessInBatches(batchSize: 100);

// ForEachAsync for when you have nothing to return
await AsyncProcessorBuilder.WithItems(ids) // Or Extension Method: await ids.ToAsyncProcessorBuilder()
        .ForEachAsync(id => DoSomethingAsync(id), CancellationToken.None) 
        .ProcessInBatches(batchSize: 100);
```

**Caveats**  
-   If even just 1 Task in a batch is slow or hangs, this will prevent the next batch from starting
-   If you set a batch of 100, and 70 have finished, you'll only have 30 left executing. This could slow things down

## Parallel

**Types**  
| Type                                             | Source Object | Return Object | Method 1           | Method 2           |
|--------------------------------------------------|---------------|---------------|--------------------| ------------------ |
| `ParallelAsyncProcessor`                         | ❌             | ❌             | `.WithExecutionCount(int)` | `.ForEachAsync(delegate)` |
| `ParallelAsyncProcessor<TInput>`                | ✔             | ❌             | `.WithItems(IEnumerable<TInput>)` | `.ForEachAsync(delegate)` |
| `ResultParallelAsyncProcessor<TOutput>`          | ❌             | ✔             | `.WithExecutionCount(int)` | `.SelectAsync(delegate)`  |
| `ResultParallelAsyncProcessor<TInput, TOutput>` | ✔             | ✔             | `.WithItems(IEnumerable<TInput>)` | `.SelectAsync(delegate)`  |

**How it works**  
Processes your Asynchronous Tasks as fast as it can. All at the same time if it can

**Usage**  
```csharp
var ids = Enumerable.Range(0, 5000).ToList();

// SelectAsync for if you want to return something
var results = await AsyncProcessorBuilder.WithItems(ids) // Or Extension Method: await ids.ToAsyncProcessorBuilder()
        .SelectAsync(id => DoSomethingAndReturnSomethingAsync(id), CancellationToken.None)
        .ProcessInParallel();

// ForEachAsync for when you have nothing to return
await AsyncProcessorBuilder.WithItems(ids) // Or Extension Method: await ids.ToAsyncProcessorBuilder()
        .ForEachAsync(id => DoSomethingAsync(id), CancellationToken.None) 
        .ProcessInParallel();
```

**Caveats**  
-   Depending on how many operations you have, you could overwhelm your system. Memory and CPU and Network usage could spike, and cause bottlenecks / crashes / exceptions

## Processor Methods

As above, you can see that you can just `await` on the processor to get the results.
Below shows examples of using the processor object and the various methods available.

This is for when you need to Enumerate through some objects and use them in your operations. E.g. Sending notifications to certain ids
```csharp
    var httpClient = new HttpClient();

    var ids = Enumerable.Range(0, 5000).ToList();

    // This is for when you need to Enumerate through some objects and use them in your operations
    
    var itemProcessor = Enumerable.Range(0, 5000).ToAsyncProcessorBuilder()
        .SelectAsync(NotifyAsync)
        .ProcessInParallel(100);

    // Or
    // var itemProcessor = AsyncProcessorBuilder.WithItems(ids)
    //     .SelectAsync(NotifyAsync, CancellationToken.None)
    //     .ProcessInParallel(100);

// GetEnumerableTasks() returns IEnumerable<Task<TOutput>> - These may have completed, or may still be waiting to finish.
    var tasks = itemProcessor.GetEnumerableTasks();

// Or call GetResultsAsyncEnumerable() to get an IAsyncEnumerable<TOutput> so you can process them in real-time as they finish.
    await foreach (var httpResponseMessage in itemProcessor.GetResultsAsyncEnumerable())
    {
        // Do something
    }

// Or call GetResultsAsync() to get a Task<TOutput[]> that contains all of the finished results 
    var results = await itemProcessor.GetResultsAsync();

// My dummy method
    Task<HttpResponseMessage> NotifyAsync(int id)
    {
        return httpClient.GetAsync($"https://localhost:8080/notify/{id}");
    }
```

This is for when you need to don't need any objects - But just want to do something a certain amount of times. E.g. Pinging a site to warm up multiple instances
```csharp
    var httpClient = new HttpClient();

    var itemProcessor = AsyncProcessorBuilder.WithExecutionCount(100)
        .SelectAsync(PingAsync, CancellationToken.None)
        .ProcessInParallel(10);

// GetEnumerableTasks() returns IEnumerable<Task<TOutput>> - These may have completed, or may still be waiting to finish.
    var tasks = itemProcessor.GetEnumerableTasks();

// Or call GetResultsAsyncEnumerable() to get an IAsyncEnumerable<TOutput> so you can process them in real-time as they finish.
    await foreach (var httpResponseMessage in itemProcessor.GetResultsAsyncEnumerable())
    {
        // Do something
    }

// Or call GetResultsAsync() to get a Task<TOutput[]> that contains all of the finished results 
    var results = await itemProcessor.GetResultsAsync();

// My dummy method
    Task<HttpResponseMessage> PingAsync()
    {
        return httpClient.GetAsync("https://localhost:8080/ping");
    }
```
