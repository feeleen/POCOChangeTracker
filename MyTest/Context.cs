using LinqToDB;
using LinqToDB.Mapping;
using POCOChangeTracker;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyTest
{
	public class DbContext : LinqToDB.Data.DataConnection
	{
		public DbContext() : base("Northwind") { }
		public ITable<Customer> Customers => GetTable<Customer>();
	}

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
}
