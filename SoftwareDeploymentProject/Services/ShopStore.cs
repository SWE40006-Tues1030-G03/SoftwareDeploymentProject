using Microsoft.Data.Sqlite;
using SoftwareDeploymentProject.Models;

namespace SoftwareDeploymentProject.Services;

// Connections are short lived. Each write transaction is atomic, including checkout.
public sealed class ShopStore(string connectionString)
{
    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }
    public void Initialize()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS products (
                id INTEGER PRIMARY KEY, name TEXT NOT NULL, category TEXT NOT NULL,
                description TEXT NOT NULL, price_cents INTEGER NOT NULL CHECK(price_cents > 0), icon TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS cart_items (
                cart_id TEXT NOT NULL, product_id INTEGER NOT NULL REFERENCES products(id),
                quantity INTEGER NOT NULL CHECK(quantity BETWEEN 1 AND 20), PRIMARY KEY(cart_id, product_id));
            CREATE TABLE IF NOT EXISTS orders (
                reference TEXT PRIMARY KEY, cart_id TEXT NOT NULL UNIQUE, created_utc TEXT NOT NULL,
                total_cents INTEGER NOT NULL CHECK(total_cents > 0));
            CREATE TABLE IF NOT EXISTS order_items (
                order_reference TEXT NOT NULL REFERENCES orders(reference), name TEXT NOT NULL,
                price_cents INTEGER NOT NULL, quantity INTEGER NOT NULL CHECK(quantity BETWEEN 1 AND 20));
            INSERT OR IGNORE INTO products VALUES
                (1,'Campus hoodie','Clothing','A comfortable everyday layer for early lectures and late study sessions.',6500,'hoodie'),
                (2,'Everyday tote','Accessories','A reusable cotton carry-all for books, groceries and everything between.',1800,'tote'),
                (3,'Study notebook','Stationery','A5 dotted pages, ready for your next big idea.',1200,'notebook'),
                (4,'Campus drink bottle','Accessories','A reusable 750 mL bottle to keep you refreshed on the go.',2500,'bottle'),
                (5,'Essential tee','Clothing','A relaxed cotton tee with a little campus spirit.',3000,'tee'),
                (6,'Desk pen set','Stationery','Three smooth-writing pens for notes, sketches and plans.',800,'pens');
            """;
        command.ExecuteNonQuery();
    }
    public bool IsReady()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM products";
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }
    public IReadOnlyList<Product> Products()
    {
        using var connection = Open();
        return ReadProducts(connection);
    }
    private static List<Product> ReadProducts(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id,name,category,description,price_cents,icon FROM products ORDER BY id";
        using var reader = command.ExecuteReader();
        var products = new List<Product>();
        while (reader.Read()) products.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetInt32(4), reader.GetString(5)));
        return products;
    }
    public Cart GetCart(string cartId)
    {
        using var connection = Open();
        return ReadCart(connection, cartId);
    }
    private static Cart ReadCart(SqliteConnection connection, string cartId, SqliteTransaction? transaction = null)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT p.id,p.name,p.category,p.description,p.price_cents,p.icon,c.quantity
            FROM cart_items c JOIN products p ON p.id=c.product_id WHERE c.cart_id=$cart ORDER BY p.id
            """;
        command.Parameters.AddWithValue("$cart", cartId);
        using var reader = command.ExecuteReader();
        var lines = new List<CartLine>();
        while (reader.Read()) lines.Add(new(new(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetInt32(4), reader.GetString(5)), reader.GetInt32(6)));
        return new(lines);
    }
    public void ChangeQuantity(string cartId, int productId, int quantity, bool add)
    {
        ShopRules.ValidateQuantity(quantity);
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(*) FROM products WHERE id=$product";
        command.Parameters.AddWithValue("$product", productId);
        if (Convert.ToInt32(command.ExecuteScalar()) != 1) throw new ArgumentException("That product is not available.");
        command.CommandText = "SELECT COUNT(*) FROM orders WHERE cart_id=$cart";
        command.Parameters.AddWithValue("$cart", cartId);
        if (Convert.ToInt32(command.ExecuteScalar()) != 0)
            throw new ArgumentException("This cart has already been checked out. Return to the shop to start a new cart.");
        var existing = ReadCart(connection, cartId, transaction).Lines.FirstOrDefault(l => l.Product.Id == productId)?.Quantity ?? 0;
        var next = add ? checked(existing + quantity) : quantity;
        ShopRules.ValidateQuantity(next);
        command.CommandText = """
            INSERT INTO cart_items(cart_id,product_id,quantity) VALUES($cart,$product,$quantity)
            ON CONFLICT(cart_id,product_id) DO UPDATE SET quantity=excluded.quantity
            """;
        command.Parameters.AddWithValue("$quantity", next);
        command.ExecuteNonQuery();
        transaction.Commit();
    }
    public void Remove(string cartId, int productId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM cart_items WHERE cart_id=$cart AND product_id=$product";
        command.Parameters.AddWithValue("$cart", cartId);
        command.Parameters.AddWithValue("$product", productId);
        command.ExecuteNonQuery();
    }
    public bool HasOrder(string cartId) => OrderForCart(cartId) is not null;
    private string? OrderForCart(string cartId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT reference FROM orders WHERE cart_id=$cart";
        command.Parameters.AddWithValue("$cart", cartId);
        return command.ExecuteScalar() as string;
    }
    public string Checkout(string cartId)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT reference FROM orders WHERE cart_id=$cart";
        command.Parameters.AddWithValue("$cart", cartId);
        // A repeated checkout returns the same receipt, never a duplicate order.
        if (command.ExecuteScalar() is string previous) return previous;
        var cart = ReadCart(connection, cartId, transaction);
        if (cart.ItemCount == 0) throw new ArgumentException("Add something to your cart before checking out.");
        var reference = Guid.NewGuid().ToString("N");
        command.CommandText = "INSERT INTO orders VALUES($reference,$cart,$created,$total)";
        command.Parameters.AddWithValue("$reference", reference);
        command.Parameters.AddWithValue("$created", DateTimeOffset.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$total", cart.TotalCents);
        command.ExecuteNonQuery();
        foreach (var line in cart.Lines)
        {
            using var item = connection.CreateCommand();
            item.Transaction = transaction;
            item.CommandText = "INSERT INTO order_items VALUES($reference,$name,$price,$quantity)";
            item.Parameters.AddWithValue("$reference", reference);
            item.Parameters.AddWithValue("$name", line.Product.Name);
            item.Parameters.AddWithValue("$price", line.Product.PriceCents);
            item.Parameters.AddWithValue("$quantity", line.Quantity);
            item.ExecuteNonQuery();
        }
        command.CommandText = "DELETE FROM cart_items WHERE cart_id=$cart";
        command.ExecuteNonQuery();
        transaction.Commit();
        return reference;
    }
    public Order? GetOrder(string reference, string cartId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT created_utc,total_cents FROM orders WHERE reference=$reference AND cart_id=$cart";
        command.Parameters.AddWithValue("$reference", reference);
        command.Parameters.AddWithValue("$cart", cartId);
        string created;
        int total;
        using (var reader = command.ExecuteReader())
        {
            if (!reader.Read()) return null;
            created = reader.GetString(0); total = reader.GetInt32(1);
        }
        command.CommandText = "SELECT name,price_cents,quantity FROM order_items WHERE order_reference=$reference ORDER BY rowid";
        using var items = command.ExecuteReader();
        var lines = new List<OrderLine>();
        while (items.Read()) lines.Add(new(items.GetString(0), items.GetInt32(1), items.GetInt32(2)));
        return new(reference, created, total, lines);
    }
}
