using Microsoft.EntityFrameworkCore;
using Timelines.Data.Entities;

namespace Timelines.Data;

public class TimelinesDbContext : DbContext
{
    public TimelinesDbContext(DbContextOptions<TimelinesDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserProfileEntity> UserProfiles => Set<UserProfileEntity>();
    public DbSet<TimelineEntity> Timelines => Set<TimelineEntity>();
    public DbSet<LaneEntity> Lanes => Set<LaneEntity>();
    public DbSet<TagEntity> Tags => Set<TagEntity>();
    public DbSet<TimelineItemEntity> TimelineItems => Set<TimelineItemEntity>();
    public DbSet<ItemTagEntity> ItemTags => Set<ItemTagEntity>();
    public DbSet<AttachmentEntity> Attachments => Set<AttachmentEntity>();
    public DbSet<ShareLinkEntity> ShareLinks => Set<ShareLinkEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUserProfile(modelBuilder);
        ConfigureTimeline(modelBuilder);
        ConfigureLane(modelBuilder);
        ConfigureTag(modelBuilder);
        ConfigureTimelineItem(modelBuilder);
        ConfigureItemTag(modelBuilder);
        ConfigureAttachment(modelBuilder);
        ConfigureShareLink(modelBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        OnBeforeSaving();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void OnBeforeSaving()
    {
        var now = DateTimeOffset.UtcNow;
        var entries = ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is UserProfileEntity userProfile)
                {
                    userProfile.CreatedUtc = now;
                    userProfile.UpdatedUtc = now;
                }
                else if (entry.Entity is TimelineEntity timeline)
                {
                    timeline.CreatedUtc = now;
                    timeline.UpdatedUtc = now;
                }
                else if (entry.Entity is LaneEntity lane)
                {
                    lane.CreatedUtc = now;
                    lane.UpdatedUtc = now;
                }
                else if (entry.Entity is TagEntity tag)
                {
                    tag.CreatedUtc = now;
                    tag.UpdatedUtc = now;
                }
                else if (entry.Entity is TimelineItemEntity item)
                {
                    item.CreatedUtc = now;
                    item.UpdatedUtc = now;
                    ComputeSortKeys(item);
                }
                else if (entry.Entity is AttachmentEntity attachment)
                {
                    attachment.CreatedUtc = now;
                }
                else if (entry.Entity is ShareLinkEntity shareLink)
                {
                    shareLink.CreatedUtc = now;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is UserProfileEntity userProfile)
                {
                    userProfile.UpdatedUtc = now;
                }
                else if (entry.Entity is TimelineEntity timeline)
                {
                    timeline.UpdatedUtc = now;
                }
                else if (entry.Entity is LaneEntity lane)
                {
                    lane.UpdatedUtc = now;
                }
                else if (entry.Entity is TagEntity tag)
                {
                    tag.UpdatedUtc = now;
                }
                else if (entry.Entity is TimelineItemEntity item)
                {
                    item.UpdatedUtc = now;
                    ComputeSortKeys(item);
                }
            }
        }
    }

    private static void ComputeSortKeys(TimelineItemEntity item)
    {
        item.StartSortKey = DateComponentSortKeyHelper.ComputeSortKey(item.StartDate);
        item.EndSortKey = item.EndDate != null 
            ? DateComponentSortKeyHelper.ComputeSortKey(item.EndDate) 
            : null;
    }

    private static void ConfigureUserProfile(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserProfileEntity>();
        entity.ToTable("UserProfiles");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.PreferredLanguage).IsRequired().HasMaxLength(10);
        entity.Property(e => e.Email).HasMaxLength(256);
        entity.Property(e => e.Phone).HasMaxLength(50);
    }

    private static void ConfigureTimeline(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TimelineEntity>();
        entity.ToTable("Timelines");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
        entity.Property(e => e.Description).HasMaxLength(5000);
        
        entity.HasIndex(e => e.OwnerUserId);

        entity.HasMany(e => e.Lanes)
            .WithOne(l => l.Timeline)
            .HasForeignKey(l => l.TimelineId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.Tags)
            .WithOne(t => t.Timeline)
            .HasForeignKey(t => t.TimelineId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.Items)
            .WithOne(i => i.Timeline)
            .HasForeignKey(i => i.TimelineId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(e => e.ShareLinks)
            .WithOne(s => s.Timeline)
            .HasForeignKey(s => s.TimelineId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureLane(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<LaneEntity>();
        entity.ToTable("Lanes");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        
        entity.HasIndex(e => e.TimelineId);
        entity.HasIndex(e => new { e.TimelineId, e.Name }).IsUnique();

        entity.HasMany(e => e.Items)
            .WithOne(i => i.Lane)
            .HasForeignKey(i => i.LaneId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureTag(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TagEntity>();
        entity.ToTable("Tags");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        
        entity.HasIndex(e => e.TimelineId);
        entity.HasIndex(e => new { e.TimelineId, e.Name }).IsUnique();
    }

    private static void ConfigureTimelineItem(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TimelineItemEntity>();
        entity.ToTable("TimelineItems");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
        entity.Property(e => e.Description).HasMaxLength(5000);
        
        entity.HasIndex(e => e.TimelineId);
        entity.HasIndex(e => e.LaneId);
        entity.HasIndex(e => e.StartSortKey);

        entity.OwnsOne(e => e.StartDate, sd =>
        {
            sd.Property(d => d.Era).HasColumnName("StartEra").IsRequired();
            sd.Property(d => d.Year).HasColumnName("StartYear").IsRequired();
            sd.Property(d => d.Month).HasColumnName("StartMonth");
            sd.Property(d => d.Day).HasColumnName("StartDay");
            sd.Property(d => d.Precision).HasColumnName("StartPrecision").IsRequired();
            sd.Property(d => d.IsApprox).HasColumnName("StartIsApprox").IsRequired();
        });

        entity.OwnsOne(e => e.EndDate, ed =>
        {
            ed.Property(d => d.Era).HasColumnName("EndEra").IsRequired();
            ed.Property(d => d.Year).HasColumnName("EndYear").IsRequired();
            ed.Property(d => d.Month).HasColumnName("EndMonth");
            ed.Property(d => d.Day).HasColumnName("EndDay");
            ed.Property(d => d.Precision).HasColumnName("EndPrecision").IsRequired();
            ed.Property(d => d.IsApprox).HasColumnName("EndIsApprox").IsRequired();
        });

        entity.HasMany(e => e.Attachments)
            .WithOne(a => a.Item)
            .HasForeignKey(a => a.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureItemTag(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ItemTagEntity>();
        entity.ToTable("ItemTags");
        entity.HasKey(e => new { e.ItemId, e.TagId });

        entity.HasOne(e => e.Item)
            .WithMany(i => i.ItemTags)
            .HasForeignKey(e => e.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Tag)
            .WithMany(t => t.ItemTags)
            .HasForeignKey(e => e.TagId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAttachment(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AttachmentEntity>();
        entity.ToTable("Attachments");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.MediaType).IsRequired().HasMaxLength(50);
        entity.Property(e => e.BlobKey).IsRequired().HasMaxLength(500);
        entity.Property(e => e.ThumbnailBlobKey).HasMaxLength(500);
        entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
        entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(500);
    }

    private static void ConfigureShareLink(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ShareLinkEntity>();
        entity.ToTable("ShareLinks");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.LinkToken).IsRequired().HasMaxLength(100);
        
        entity.HasIndex(e => e.LinkToken).IsUnique();
    }
}
