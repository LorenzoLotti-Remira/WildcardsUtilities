namespace WildcardsUtilities.Async;

public static class FilesFiltering
{
    private static readonly IActorRefFactory _system =
        ActorSystem.Create("files-filtering-system");

    public static async ValueTask<IEnumerable<Common.ChecksummedFileInfo>> GetFilesAsync
    (
        Common.FilesFilteringOptions options,
        CancellationToken cancellationToken = default
    )
    =>
    (
        await _system
            .ActorOf<FileFilteringActor>($"root-files-filtering-actor-{Guid.NewGuid()}")
            .Ask<SearchResponseMessage>(new SearchRequestMessage(options), cancellationToken)
            .ConfigureAwait(false)
    )
    .Files;
}
