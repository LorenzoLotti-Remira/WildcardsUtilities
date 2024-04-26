namespace WildcardsUtilities.Async.Actors;

internal sealed class FileFilteringActor : ReceiveActor
{
    private IActorRef _requestSender = ActorRefs.Nobody;
    private uint _pendingRequests = 0;

    private IEnumerable<Common.ChecksummedFileInfo> _files = [];

    public FileFilteringActor()
    {
        Receive<SearchResponseMessage>(m =>
        {
            _pendingRequests--;
            Collect(m.Files);
        });

        Receive<SearchRequestMessage>(m =>
        {
            _requestSender = Sender;
            Search(m.Options);
        });
    }

    private void Collect(IEnumerable<Common.ChecksummedFileInfo> files)
    {
        _files = _files.Concat(files);

        if (_pendingRequests == 0)
        {
            _requestSender.Tell(new SearchResponseMessage(_files.DistinctBy(f => f.Path)));
            Context.Stop(Self);
        }
    }

    private void Search(Common.FilesFilteringOptions options) => Collect
    (
        Common.FilesFilteringCore.GetFiles
        (
            options,
            Common.FlatMap.ForEach,
            Common.FlatMap.ForEach,
            newOptions =>
            {
                _pendingRequests++;

                Context
                    .ActorOf<FileFilteringActor>($"file-filtering-actor-{_pendingRequests}")
                    .Tell(new SearchRequestMessage(newOptions));

                return [];
            },
            fileInfo =>
            {
                try
                {
                    using var file = File.OpenRead(fileInfo.Path);
                    return fileInfo with { Checksum = [.. SHA1.HashData(file)] };
                }
                catch
                {
                    return fileInfo;
                }
            },
            fileInfo => options.ComputeChecksum
        )
    );
}