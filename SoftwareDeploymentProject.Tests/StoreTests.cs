using Microsoft.Data.Sqlite;
using SoftwareDeploymentProject.Services;
namespace SoftwareDeploymentProject.Tests;

public class StoreTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "campus-tests-" + Guid.NewGuid().ToString("N"));
    private readonly string connection;
    private readonly ShopStore store;
    public StoreTests()
    {
        Directory.CreateDirectory(directory);
        connection = new SqliteConnectionStringBuilder { DataSource = Path.Combine(directory, "shop.db"), ForeignKeys = true }.ToString();
        store = new(connection); store.Initialize();
    }
    [Fact] public void InitializationIsRepeatable() { store.Initialize(); Assert.Equal(6, store.Products().Count); Assert.True(store.IsReady()); }
    [Fact] public void CartsAreIsolated() { store.ChangeQuantity("alice", 1, 2, true); Assert.Empty(store.GetCart("bob").Lines); Assert.Equal(13000, store.GetCart("alice").TotalCents); }
    [Fact]
    public void QuantityCanBeAddedUpdatedAndRemoved()
    {
        store.ChangeQuantity("a", 1, 2, true); store.ChangeQuantity("a", 1, 3, true);
        Assert.Equal(5, store.GetCart("a").ItemCount);
        store.ChangeQuantity("a", 1, 2, false); Assert.Equal(2, store.GetCart("a").ItemCount);
        store.Remove("a", 1); Assert.Empty(store.GetCart("a").Lines);
    }
    [Fact]
    public void CumulativeQuantityLimitLeavesCartUnchanged()
    {
        store.ChangeQuantity("a", 1, 20, true);
        Assert.Throws<ArgumentException>(() => store.ChangeQuantity("a", 1, 1, true));
        Assert.Equal(20, store.GetCart("a").ItemCount);
    }
    [Fact] public void UnknownProductIsRejected() => Assert.Throws<ArgumentException>(() => store.ChangeQuantity("a", 999, 1, true));
    [Fact] public void EmptyCheckoutIsRejected() => Assert.Throws<ArgumentException>(() => store.Checkout("a"));
    [Fact]
    public void CheckoutPersistsSnapshotAndClearsCart()
    {
        store.ChangeQuantity("a", 1, 2, true); store.ChangeQuantity("a", 3, 1, true);
        var reference = store.Checkout("a");
        var restarted = new ShopStore(connection);
        var order = restarted.GetOrder(reference, "a"); Assert.NotNull(order);
        Assert.Equal(14200, order.TotalCents); Assert.Equal(2, order.Lines.Count);
        Assert.Empty(restarted.GetCart("a").Lines); Assert.Null(restarted.GetOrder(reference, "b"));
    }
    [Fact]
    public void RepeatedCheckoutReturnsSameOrder()
    {
        store.ChangeQuantity("a", 1, 1, true); var reference = store.Checkout("a");
        Assert.Equal(reference, store.Checkout("a"));
    }
    [Fact]
    public async Task ConcurrentCheckoutsCreateOneOrder()
    {
        store.ChangeQuantity("a", 1, 1, true);
        var results = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => Task.Run(() => store.Checkout("a"))));
        Assert.Single(results.Distinct());
    }
    [Fact]
    public void CheckedOutCartCannotBeChangedByAStaleRequest()
    {
        store.ChangeQuantity("a", 1, 1, true); var reference = store.Checkout("a");
        Assert.Throws<ArgumentException>(() => store.ChangeQuantity("a", 1, 2, false));
        Assert.Empty(store.GetCart("a").Lines); Assert.Equal(6500, store.GetOrder(reference, "a")!.TotalCents);
    }
    public void Dispose() { SqliteConnection.ClearAllPools(); Directory.Delete(directory, true); }
}
