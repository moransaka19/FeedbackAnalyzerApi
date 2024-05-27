using FeedbackAnalyze.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedbackAnalyze.Data;

public class AppDataContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Sentence> Sentences { get; set; }
    

    public AppDataContext(DbContextOptions<AppDataContext> options)
        : base(options)
    { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .HasKey(x => x.Id);
        
        modelBuilder.Entity<Product>()
            .HasMany<Feedback>(x => x.Feedbacks)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId);
        
        modelBuilder.Entity<Feedback>()
            .HasKey(x => x.Id);
        
        modelBuilder.Entity<Feedback>()
            .HasMany<Sentence>(x => x.Sentences)
            .WithOne(x => x.Feedback)
            .HasForeignKey(x => x.FeedbackId);

        modelBuilder.Entity<Sentence>()
            .HasMany(x => x.Tags)
            .WithMany(x => x.Sentences);
        
        modelBuilder.Entity<Tag>()
            .HasKey(x => x.Id);
    }
}