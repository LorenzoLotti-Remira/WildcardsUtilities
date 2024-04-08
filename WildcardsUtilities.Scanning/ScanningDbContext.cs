
namespace WildcardsUtilities.Scanning;

public class ScanningDbContext(DbContextOptions<ScanningDbContext> options) : DbContext(options)
{
    public DbSet<SnapshotDbItem> Snapshots { get; set; }
    public DbSet<FileDbItem> Files { get; set; }
    public DbSet<PathDbItem> Paths { get; set; }
    public DbSet<DriveDbItem> Drives { get; set; }
    public DbSet<VolumeDbItem> Volumes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primary keys.
        modelBuilder.Entity<SnapshotDbItem>().HasKey(s => s.SnapshotId);
        modelBuilder.Entity<PathDbItem>().HasKey(p => p.PathId);
        modelBuilder.Entity<FileDbItem>().HasKey(f => f.FileId);
        modelBuilder.Entity<DriveDbItem>().HasKey(d => d.DriveId);
        modelBuilder.Entity<VolumeDbItem>().HasKey(d => d.VolumeId);

        // Foreign keys.
        modelBuilder
            .Link<VolumeDbItem, DriveDbItem>(v => v.DriveId)
            .Link<FileDbItem, PathDbItem>(f => f.PathId)
            .Link<FileDbItem, VolumeDbItem>(f => f.VolumeId)
            .Link<FileDbItem, SnapshotDbItem>(f => f.SnapshotId);
    }
}
