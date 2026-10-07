using System;
using System.Threading;
using System.Threading.Tasks;

namespace LastEpoch_Hud.Scripts.Core.BuildImport;

// Loading runs off the game thread. Only Poll publishes a result to the UI.
// An abandoned request can never replace a newer build or reopen a closed window.
public sealed class MaxrollPreviewSession
{
    readonly Func<string, CancellationToken, Task<MaxrollBuild>> retrieve;
    Request pending;

    public MaxrollPreviewSession()
        : this(Retrieve) { }

    public MaxrollPreviewSession(Func<string, CancellationToken, Task<MaxrollBuild>> retrieve)
    {
        this.retrieve = retrieve ?? throw new ArgumentNullException(nameof(retrieve));
    }

    public bool IsLoading => pending != null;
    public MaxrollBuild Build { get; private set; }
    public string Error { get; private set; }

    public void Load(string input)
    {
        Cancel();
        Build = null;
        Error = null;
        // Reject invalid links immediately, without issuing any network request.
        MaxrollLink.Parse(input);
        var request = new Request();
        pending = request;
        request.Task = Task.Run(async () =>
        {
            try
            {
                return new Result
                {
                    Build = await retrieve(input, request.Token).ConfigureAwait(false),
                };
            }
            catch (OperationCanceledException) when (request.Token.IsCancellationRequested)
            {
                return new Result();
            }
            catch (Exception ex)
            {
                return new Result { Error = ex.Message };
            }
            finally
            {
                request.Dispose();
            }
        });
    }

    public void Cancel()
    {
        var previous = pending;
        pending = null;
        previous?.Cancel();
    }

    public bool Poll()
    {
        var request = pending;
        if (request == null || !request.Task.IsCompleted)
            return false;
        pending = null;
        var result = request.Task.GetAwaiter().GetResult();
        Build = result.Build;
        Error = result.Error;
        return true;
    }

    static async Task<MaxrollBuild> Retrieve(string input, CancellationToken token)
    {
        using var client = new MaxrollBuildClient();
        return await client.RetrieveAsync(input, token).ConfigureAwait(false);
    }

    sealed class Result
    {
        public MaxrollBuild Build;
        public string Error;
    }

    sealed class Request
    {
        readonly object gate = new();
        readonly CancellationTokenSource source = new();
        bool disposed;

        public Request() => Token = source.Token;

        public CancellationToken Token { get; }
        public Task<Result> Task;

        public void Cancel()
        {
            lock (gate)
                if (!disposed)
                    source.Cancel();
        }

        public void Dispose()
        {
            lock (gate)
            {
                disposed = true;
                source.Dispose();
            }
        }
    }
}
