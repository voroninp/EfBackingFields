using Microsoft.EntityFrameworkCore;

var cb = new Testcontainers.PostgreSql.PostgreSqlBuilder();
await using var c = cb.Build();
await c.StartAsync();

var ctx = new Ctx(c.GetConnectionString());
await ctx.Database.MigrateAsync();

var owner = new Owner();
owner.AddItem(new Owned());

await ctx.AddAsync(owner);
await ctx.SaveChangesAsync();
await ctx.DisposeAsync();

ctx = new Ctx(c.GetConnectionString());
var owners = await ctx.Set<Owner>().ToListAsync();
owners.ForEach(o => Console.WriteLine(o.Items.Count));
Console.WriteLine("Ok");

public sealed class Ctx(string connectionString) : DbContext
{
    public Ctx() : this(string.Empty)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Owner>(b =>
        {
            b.Navigation(e => e.Items).HasField("_ownedItems");
            b.OwnsMany(e => e.Items, b =>
            {
                b.WithOwner().HasForeignKey(e => e.OwnerId);
                b.HasKey(e => e.Id);
                // If you comment this out, migrations work.
                b.Navigation(e => e.Items).HasField("_ownedItems");
                b.OwnsMany(e => e.Items, b =>
                {
                    b.WithOwner().HasForeignKey(e => e.OwnerId);
                    b.HasKey(e => e.Id);
                });
            });
        });
    }
}

public sealed class Owner
{
    public int Id { get; private set; }

    private readonly List<Owned> _ownedItems = new List<Owned>();

    public IReadOnlyList<Owned> Items { get; }

    public void AddItem(Owned item)
    {
        _ownedItems.Add(item);
    }

    public Owner()
    {
        Items = _ownedItems.AsReadOnly();
    }
}

public sealed class Owned
{
    public int Id { get; private set; }

    public int OwnerId { get; private set; }

    private readonly List<Owned2> _ownedItems = new List<Owned2>();

    public IReadOnlyList<Owned2> Items { get; }

    public Owned()
    {
        Items = _ownedItems.AsReadOnly();
    }

    public void AddItem(Owned2 item)
    {
        _ownedItems.Add(item);
    }
}

public sealed class Owned2
{
    public int Id { get; private set; }

    public int OwnerId { get; private set; }
}