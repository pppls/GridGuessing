using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class GridDbContext : DbContext
{
    public GridDbContext(DbContextOptions<GridDbContext> options) : base(options)
    {
    }

    public DbSet<GridElement> GridElements { get; set; }
    
    public DbSet<GridPromotionalGame> PromotionalGames { get; set; }
    public DbSet<Prize> Prizes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<GridElement>()
            .HasOne(g => g.Prize)
            .WithMany() // 
            .HasForeignKey(g => g.PrizeId);
        
        modelBuilder.Entity<MonetaryPrize>().ToTable("MonetaryPrizes");
        modelBuilder.Entity<NonMonetaryPrize>().ToTable("NonMonetaryPrizes");
        
        modelBuilder.Entity<GridPromotionalGame>()
            .HasMany(g => g.GridElements)
            .WithOne(e => e.GridPromotionalGame)
            .HasForeignKey(e => e.GridPromotionalGameId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<GridElement>()
            .Property(b => b.Timestamp)
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();
    }
}

public class GridElement
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }
    public int Index { get; set; }
    public bool HasBeenFlipped { get; set; }
    public string? Flipper { get; set; }
    
    [Timestamp]
    public byte[] Timestamp { get; set; }
    
    public string? PrizeId { get; set; }
    public Prize? Prize { get; set; }
    
    public string GridPromotionalGameId { get; set; }
    public GridPromotionalGame GridPromotionalGame { get; set; }
}

public abstract class Prize
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }
}

public class MonetaryPrize : Prize
{
    public decimal MonetaryValue { get; set; } 
}

public class NonMonetaryPrize : Prize
{
    public string Description { get; set; } 
}

public class GridPromotionalGame
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }
    public string Name { get; set; }
    
    public List<GridElement> GridElements { get; set; }
}