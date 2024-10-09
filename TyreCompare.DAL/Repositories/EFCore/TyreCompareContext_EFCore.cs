using Microsoft.EntityFrameworkCore;
using TyreCompare.Models;

namespace TyreCompare.DAL.EFCore;

public partial class TyreCompareContext_EFCore : DbContext
{
    private string TestConnectionString { get; set; }

    public TyreCompareContext_EFCore()
    {
    }

    public TyreCompareContext_EFCore(string testConnectionString)
    {
        TestConnectionString = testConnectionString;
    }

    public TyreCompareContext_EFCore(DbContextOptions<TyreCompareContext_EFCore> options)
        : base(options)
    {
    }

    public virtual DbSet<ITyre> ITyres { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<PatternSet> PatternSets { get; set; }

    public virtual DbSet<Summary> Summary { get; set; }

    public virtual DbSet<Models.Log> Logs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (string.IsNullOrWhiteSpace(TestConnectionString))
        { return; }
        optionsBuilder.UseSqlServer(TestConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ITyre>(entity =>
        {
            entity
                .ToTable("ITyres", "Data");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Brand).HasMaxLength(50);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.Image_Url_New)
                .HasMaxLength(150)
                .HasColumnName("Image_Url_New");
            entity.Property(e => e.Image_Url)
                .HasMaxLength(100);
            entity.Property(e => e.Pattern).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity
                .ToTable("Roles", "Lookup");

            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.RoleName).HasMaxLength(20);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity
                .ToTable("Users", "App");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(20);
        });

        modelBuilder.Entity<PatternSet>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PatternSets", "View");
        });

        modelBuilder.Entity<Summary>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Summary", "View");
        });

        modelBuilder.Entity<Models.Log>(entity =>
        {
            entity
                .ToTable("Logs", "App");

            entity.HasKey(e => e.ID);

        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
