namespace UnitOfWorkSharp.Host.Models;

using Microsoft.EntityFrameworkCore;

public class BloggingContext : DbContext
{
    public BloggingContext(DbContextOptions<BloggingContext> options) : base(options)
    {
    }

    public DbSet<Blog> Blogs => Set<Blog>();

    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.EnableAutoHistory();
}

public class Blog
{
    public Blog()
    {
        Title = string.Empty;
        Id = -1;
        Url = string.Empty;
        Url = string.Empty;
        Posts = new List<Post>();
    }

    public int Id { get; init; }

    public string Url { get; init; }

    public string Title { get; init; }

    public List<Post> Posts { get; init; }
}

public class Post
{
    public Post()
    {
        Title = string.Empty;
        Id = -1;

        Content = string.Empty;
        Comments = new List<Comment>();
    }

    public int Id { get; init; }

    public string Title { get; init; }

    public string Content { get; init; }

    public List<Comment> Comments { get; init; }
}

public class Comment
{
    public Comment()
    {
        Title = string.Empty;
        Id = -1;

        Content = string.Empty;
    }

    public int Id { get; init; }

    public string Title { get; init; }

    public string Content { get; init; }
}