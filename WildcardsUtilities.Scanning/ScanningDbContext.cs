
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
        modelBuilder.Entity<GroupSnapshotRelationDbItem>().HasKey(r => new { r.SnapshotGroupId, r.SnapshotId });

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
