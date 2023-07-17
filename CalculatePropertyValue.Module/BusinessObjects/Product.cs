using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CalculatePropertyValue.Module.BusinessObjects
{
    [DefaultClassOptions]
    public class Product : BaseObject
    {
        public Product(Session session) : base(session) { }
        private string fName;
        public string Name
        {
            get { return fName; }
            set { SetPropertyValue(nameof(Name), ref fName, value); }
        }
        [Association("Product-Orders")]
        public XPCollection<Order> Orders
        {
            get { return GetCollection<Order>(nameof(Orders)); }
        }

        [Persistent("OrdersCount")]
        private int? fOrdersCount = null;
        [PersistentAlias(nameof(fOrdersCount))]
        public int? OrdersCount
        {
            get
            {
                if (!IsLoading && !IsSaving && fOrdersCount == null)
                    UpdateOrdersCount(false);
                return fOrdersCount;
            }
        }


        [Persistent("OrdersTotal")]
        private decimal? fOrdersTotal = null;
        [PersistentAlias(nameof(fOrdersTotal))]
        public decimal? OrdersTotal
        {
            get
            {
                if (!IsLoading && !IsSaving && fOrdersTotal == null)
                    UpdateOrdersTotal(false);
                return fOrdersTotal;
            }
        }

        [Persistent("MaximumOrder")]
        private decimal? fMaximumOrder = null;
        [PersistentAlias(nameof(fMaximumOrder))]
        public decimal? MaximumOrder
        {
            get
            {
                if (!IsLoading && !IsSaving && fMaximumOrder == null)
                    UpdateMaximumOrder(false);
                return fMaximumOrder;
            }
        }

        public void UpdateOrdersCount(bool forceChangeEvents)
        {
            int? oldOrdersCount = fOrdersCount;
            fOrdersCount = Convert.ToInt32(Session.Evaluate<Product>(CriteriaOperator.Parse("Orders.Count"),
    CriteriaOperator.Parse("Oid=?", Oid)));
            if (forceChangeEvents)
                OnChanged(nameof(OrdersCount), oldOrdersCount, fOrdersCount);
        }
        public void UpdateOrdersTotal(bool forceChangeEvents)
        {
            decimal? oldOrdersTotal = fOrdersTotal;
            decimal tempTotal = 0m;
            foreach (Order detail in Orders)
                tempTotal += detail.Total;
            fOrdersTotal = tempTotal;
            if (forceChangeEvents)
                OnChanged(nameof(OrdersTotal), oldOrdersTotal, fOrdersTotal);
        }
        public void UpdateMaximumOrder(bool forceChangeEvents)
        {
            decimal? oldMaximumOrder = fMaximumOrder;
            decimal tempMaximum = 0m;
            foreach (Order detail in Orders)
                if (detail.Total > tempMaximum)
                    tempMaximum = detail.Total;
            fMaximumOrder = tempMaximum;
            if (forceChangeEvents)
                OnChanged(nameof(MaximumOrder), oldMaximumOrder, fMaximumOrder);
        }

        protected override void OnLoaded()
        {
            Reset();
            base.OnLoaded();
        }
        private void Reset()
        {
            fOrdersCount = null;
            fOrdersTotal = null;
            fMaximumOrder = null;
        }
    }
}