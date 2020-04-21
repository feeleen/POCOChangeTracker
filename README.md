# POCOChangeTracker
Simple Change Tracker for POCO objects

Usage example:

POCO entity

```cs

[Table(Name = "Customers")]
public class Customer : ChangeTrackedPOCO<Customer>
{
  [PrimaryKey]
  public virtual string CustomerID { get; set; }
  [Column]
  public virtual string CompanyName { get; set; }
  [Column]
  public virtual string ContactName { get; set; }
  [Column]
  public virtual string ContactTitle { get; set; }
  [Column]
  public virtual string Address { get; set; }
  [Column]
  public virtual string City { get; set; }
  [Column]
  public virtual string Region { get; set; }
  [Column]
  public virtual string PostalCode { get; set; }
}

```

Use with Linq2db

```cs
using (var db = new DbContext())
{
  var customer = db.Customers.Take(1).First();
  
  // here we start change tracking of the object
  customer.StartChangeTracking();
  
  customer.CompanyName = DateTime.Now.Ticks.ToString();

  // check if something changed
  var changes = customer.GetChangedProperties();

  // get old value
  var old = customer.GetOldValue(x => x.CompanyName);

  if (changes.Count > 0)
  {
    // update database record only with changed properties
    db.Update(customer, (a, b) => changes.Contains(b.ColumnName)); 
  }
}

```
