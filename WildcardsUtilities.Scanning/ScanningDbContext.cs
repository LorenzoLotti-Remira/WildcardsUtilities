namespace WildcardsUtilities.Scanning;

public class ScanningDbContext(DbContextOptions<ScanningDbContext> options) : DbContext(options)
{
    public DbSet<SnapshotDbItem> Snapshots { get; set; }
    public DbSet<FileDbItem> Files { get; set; }
    public DbSet<PathDbItem> Paths { get; set; }
    public DbSet<DriveDbItem> Drives { get; set; }
    public DbSet<VolumeDbItem> Volumes { get; set; }
    public DbSet<SnapshotGroupDbItem> SnapshotGroups { get; set; }
    public DbSet<GroupSnapshotRelationDbItem> GroupSnapshotRelations { get; set; }

    public IQueryable<GroupLogicalFileDbItem> GroupLogicalFiles =>
        from snapshotGroup in SnapshotGroups
        join groupSnapshotRelation in GroupSnapshotRelations
            on snapshotGroup.SnapshotGroupId equals groupSnapshotRelation.SnapshotGroupId
            into groupSnapshotRelations
        from groupSnapshotRelation in groupSnapshotRelations.DefaultIfEmpty()
        join snapshot in Snapshots
            on groupSnapshotRelation.SnapshotId equals snapshot.SnapshotId
        join file in Files
            on snapshot.SnapshotId equals file.SnapshotId
        group new { file.FileId, GroupName = snapshotGroup.Name, DateTime = snapshot.StartDateTime }
            by new { file.VolumeId, file.PathId, file.Name }
            into redundancies
        select
        (
            from file in redundancies
            orderby file.DateTime descending
            select new GroupLogicalFileDbItem
            {
                FileId = file.FileId,
                SnapshotGroupName = file.GroupName
            }
        )
        .First();

    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseStronglyTypeConverters();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primary keys.

        modelBuilder.Entity<SnapshotDbItem>().HasKey(s => s.SnapshotId);
        modelBuilder.Entity<PathDbItem>().HasKey(p => p.PathId);
        modelBuilder.Entity<FileDbItem>().HasKey(f => f.FileId);
        modelBuilder.Entity<DriveDbItem>().HasKey(d => d.DriveId);
        modelBuilder.Entity<VolumeDbItem>().HasKey(d => d.VolumeId);
        modelBuilder.Entity<SnapshotGroupDbItem>().HasKey(g => g.SnapshotGroupId);

        modelBuilder
            .Entity<GroupSnapshotRelationDbItem>()
            .HasKey(r => new { r.SnapshotGroupId, r.SnapshotId });



        // Unique constraints.

        modelBuilder.Entity<SnapshotGroupDbItem>().HasIndex(g => g.Name).IsUnique();



        // Foreign keys.

        modelBuilder
            .Link<VolumeDbItem, DriveDbItem>(v => v.DriveId)
            .Link<FileDbItem, PathDbItem>(f => f.PathId)
            .Link<FileDbItem, VolumeDbItem>(f => f.VolumeId)
            .Link<FileDbItem, SnapshotDbItem>(f => f.SnapshotId)
            .Link<GroupSnapshotRelationDbItem, SnapshotDbItem>(r => r.SnapshotId)
            .Link<GroupSnapshotRelationDbItem, SnapshotGroupDbItem>(r => r.SnapshotGroupId);
    }
}
