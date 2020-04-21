using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LinqToDB;

namespace MyTest
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			using (var db = new DbContext())
			{
				var list = db.Customers.Take(100).ToList();

				var customer = list[0];

				customer.StartChangeTracking();
				customer.CompanyName = DateTime.Now.Ticks.ToString();

				var changes = customer.GetChangedProperties();

				var old = customer.GetOldValue(x => x.CompanyName);

				if (changes.Count > 0)
				{
					//db.Update(customer, (a, b) => changes.Contains(b.ColumnName)); // partial update API will be available since Linq2DB v.3.0.0
				}
			}
		}
	}
}
