using FeedbackAnalyze.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedbackAnalyze.Data;

public class AppDataContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<ProductFeedback> ProductFeedbacks { get; set; }
    public DbSet<FeedbackSentence> FeedbackSentences { get; set; }
    

    public AppDataContext(DbContextOptions<AppDataContext> options)
        : base(options)
    { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .HasKey(x => x.Id);
        
        modelBuilder.Entity<Product>()
            .HasMany<ProductFeedback>(x => x.ProductFeedbacks)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId);
        
        modelBuilder.Entity<ProductFeedback>()
            .HasKey(x => x.Id);
        
        modelBuilder.Entity<ProductFeedback>()
            .HasMany<FeedbackSentence>(x => x.FeedbackSentences)
            .WithOne(x => x.ProductFeedback)
            .HasForeignKey(x => x.ProductFeedbackId);

        modelBuilder.Entity<FeedbackSentence>()
            .HasMany(x => x.Feedbacks)
            .WithMany(x => x.FeedbackSentences);
        
        modelBuilder.Entity<Feedback>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Feedback>()
            .HasIndex(x => x.CommonTag);
    }
}