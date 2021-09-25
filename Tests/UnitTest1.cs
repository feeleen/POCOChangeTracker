using Microsoft.VisualStudio.TestTools.UnitTesting;
using POCOChangeTracker;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestGetChangedProperties()
        {
            var customer = new CustomerTest { ID = 1, Name = "Test", Address = "London" };

            customer.AcceptChanges();
            customer.Name = "Modified";

            var changes = customer.GetChangedProperties();

            Assert.IsTrue(changes.Count == 1);
            Assert.IsTrue(changes[0] == nameof(customer.Name));
        }

        [TestMethod]
        public void TestGetOldValue()
        {
            var customer = new CustomerTest { ID = 1, Name = "Test", Address = "London" };

            customer.AcceptChanges();
            customer.Name = "Modified";

            var old = customer.GetOldValue(x => x.Name);

            Assert.IsTrue(old.ToString() == "Test");

            customer.AcceptChanges();
            old = customer.GetOldValue(x => x.Name);

            Assert.IsTrue(old.ToString() == "Modified");
        }

        [TestMethod]
        public void TestRollback()
        {
            var customer = new CustomerTest { ID = 1, Name = "Test", Address = "London" };

            customer.AcceptChanges();
            customer.Name = "Modified";

            customer.RejectChanges();

            Assert.IsTrue(customer.Name == "Test");
        }

        public class CustomerTest : ChangeTrackedPOCO<CustomerTest>
        {
            public virtual int ID { get; set; }
            public virtual string Name { get; set; }
            public virtual string Address { get; set; }
        }
    }
}
