# CalculatePropertyValue


# Calculate a Property Value Based on Values from a Detail Collection

This topic describes how to implement a business class, so that one of its properties is calculated based on a property(ies) of the objects contained in the child object collection.

![CalculatePropertyBasedOnDetailCollection ](https://docs.devexpress.com/eXpressAppFramework/images/calculatepropertybasedondetailcollection116394.png)

>TIP
A complete sample project is available in the DevExpress Code Examples database at [https://supportcenter.devexpress.com/ticket/details/e305/how-to-calculate-a-master-property-based-on-values-from-a-details-collection](https://supportcenter.devexpress.com/ticket/details/e305/how-to-calculate-a-master-property-based-on-values-from-a-details-collection).

## Initial Class Implementation

A  **Product**  class has a collection of  **Order**  objects. The  **Product**  and  **Order**  classes are associated by the  [One-to-Many](https://docs.devexpress.com/eXpressAppFramework/112654/business-model-design-orm/business-model-design-with-xpo/relationships-between-persistent-objects-in-code-and-ui)  relationship, which means that a  **Product**  object may be associated with several  **Order**  objects. The collection of  **Order**  objects is aggregated.  **Order**  objects are created, belonging to one of the  **Product**  objects. When the master object is removed, all the objects in its aggregated collection are removed as well.

The following snippet illustrates the  **Product**  class implementation.



```csharp
[DefaultClassOptions]
public class Product : BaseObject {
    public Product(Session session) : base(session) { }
    private string fName;
    public string Name {
        get { return fName; }
        set { SetPropertyValue(nameof(Name), ref fName, value); }
    }
    [Association("Product-Orders"), Aggregated]
    public XPCollection<Order> Orders {
        get { return GetCollection<Order>(nameof(Orders)); }
    }
}

```

The following snippet illustrates the  **Order**  class implementation.



```csharp
[DefaultClassOptions]
public class Order : BaseObject {
    public Order(Session session) : base(session) { }
    private string fDescription;
    public string Description {
         get { return fDescription; }
         set { SetPropertyValue(nameof(Description), ref fDescription, value); }
    }
    private decimal fTotal;
    public decimal Total {
        get { return fTotal; }
        set { SetPropertyValue(nameof(Total), ref fTotal, value); }
    }
    private Product fProduct;
    [Association("Product-Orders")]
    public Product Product {
        get { return fProduct; }
        set { SetPropertyValue(nameof(Product), ref fProduct, value); }
    }
}

```

In the code above, the  **Order**  class contains the  **Total**  property and the  **Product**  class has the  **MaximumOrder**  and  **OrdersTotal**  properties. These  **Product**‘s properties are calculated based on  **Total**  properties of the aggregated  **Orders**. The  **OrderCount**  property is also added to the  **Product**  class. This property exposes the number of aggregated  **Orders**.

>NOTE
You can modify an object from the child collection in a separate Detail View and save it. In this scenario, the parent object may also be marked as modified in a separate object space. If the collection property is not decorated with the [AggregatedAttribute](https://docs.devexpress.com/XPO/DevExpress.Xpo.AggregatedAttribute), you need to refresh the parent object before saving changes. To avoid this, disable the [XpoDefault.IsObjectModifiedOnNonPersistentPropertyChange](https://docs.devexpress.com/XPO/DevExpress.Xpo.XpoDefault.IsObjectModifiedOnNonPersistentPropertyChange) option before starting the application.

## Implement Non-Persistent Calculated Properties

An implementation of “lazy” calculated (calculated on demand) properties is described in this section.

Omit the property setter to implement a non-persistent property. The following code snippet demonstrates the implementation of three calculated properties - the  **OrdersCount**,  **OrdersTotal**  and  **MaximumOrder**.



```csharp
[DefaultClassOptions]
public class Product : BaseObject {
    // ...
    private int? fOrdersCount = null;
    public int? OrdersCount {
        get {
            if(!IsLoading && !IsSaving && fOrdersCount == null)
                UpdateOrdersCount(false);
            return fOrdersCount;
        }
    }
    private decimal? fOrdersTotal = null;
    public decimal? OrdersTotal {
        get {
           if(!IsLoading && !IsSaving && fOrdersTotal == null)
                UpdateOrdersTotal(false);
            return fOrdersTotal;
        }
    }
    private decimal? fMaximumOrder = null;
    public decimal? MaximumOrder {
        get {
            if(!IsLoading && !IsSaving && fMaximumOrder == null)
                UpdateMaximumOrder(false);
            return fMaximumOrder;
        }
    }
}

```

The properties’ business logic is contained into three separate methods -  **UpdateOrdersCount**,  **UpdateOrdersTotal**  and  **UpdateMaximumOrder**. These methods are invoked in the property getters. Having the business logic in separate methods allows you to update a property’s value by calling the corresponding method, when required. The  **OrdersCount**  is a simple calculated non-persistent property. This property is calculated using  **XPO**  criteria language. The  **OrdersTotal**  and  **MaximumOrder**  are complex calculated non-persistent properties, not expressed using the criteria language. So, traverse the  **Orders**  collection to calculate these properties.

>NOTE
In this topic, the **OrdersTotal** and **MaximumOrder** properties are considered to be complex to illustrate how such properties are calculated. Actually, their values can be easily calculated using **XPO** criteria language. For instance, you can use the **Avg**, **Count**, **Exists**, **Max** and **Min** functions to perform aggregate operations on collections. Refer to the [Criteria Language Syntax](https://docs.devexpress.com/CoreLibraries/4928/devexpress-data-library/criteria-language-syntax) topic for details.

The following snippet illustrates the  **UpdateOrdersCount**,  **UpdateOrdersTotal**  and  **UpdateMaximumOrder**  methods definitions.



```csharp
[DefaultClassOptions]
public class Product : BaseObject {
    // ...
    public void UpdateOrdersCount(bool forceChangeEvents) {
        int? oldOrdersCount = fOrdersCount;
        fOrdersCount = Convert.ToInt32(Evaluate(CriteriaOperator.Parse("Orders.Count")));
        if (forceChangeEvents)
          OnChanged(nameof(OrdersCount), oldOrdersCount, fOrdersCount);
    }
    public void UpdateOrdersTotal(bool forceChangeEvents) {
        decimal? oldOrdersTotal = fOrdersTotal;
        decimal tempTotal = 0m;
        foreach (Order detail in Orders)
            tempTotal += detail.Total;
        fOrdersTotal = tempTotal;
        if (forceChangeEvents)
            OnChanged(nameof(OrdersTotal), oldOrdersTotal, fOrdersTotal);
    }
    public void UpdateMaximumOrder(bool forceChangeEvents) {
        decimal? oldMaximumOrder = fMaximumOrder;
        decimal tempMaximum = 0m;
        foreach (Order detail in Orders)
            if (detail.Total > tempMaximum)
                tempMaximum = detail.Total;
        fMaximumOrder = tempMaximum;
        if (forceChangeEvents)
            OnChanged(nameof(MaximumOrder), oldMaximumOrder, fMaximumOrder);
    }
}

```

Note that the  **fOrdersCount**  is evaluated on the client side using the objects loaded from an internal  **XPO**  cache in the  **UpdateOrdersCount**  method. You can use the following code to evaluate the  **fOrdersCount**  on the server side, so the uncommitted objects are not taken into account.



```csharp
fOrdersCount = Convert.ToInt32(Session.Evaluate<Product>(CriteriaOperator.Parse("Orders.Count"), 
    CriteriaOperator.Parse("Oid=?", Oid)));

```

In the  **Order**  class’  **Total**  and  **Product**  property setters, a UI is updated when an  **Order**  object’s property values change and an object is not currently being initialized:



```csharp
[DefaultClassOptions]
public class Order : BaseObject {
    // ...
    private decimal fTotal;
    public decimal Total {
        get { return fTotal; }
        set {
            bool modified = SetPropertyValue(nameof(Total), ref fTotal, value);
            if(!IsLoading && !IsSaving && Product != null && modified) {
                Product.UpdateOrdersTotal(true);
                Product.UpdateMaximumOrder(true);
            }
        }
    }
    private Product fProduct;
    [Association("Product-Orders")]
    public Product Product {
        get { return fProduct; }
        set {
            Product oldProduct = fProduct;
            bool modified = SetPropertyValue(nameof(Product), ref fProduct, value);
            if(!IsLoading && !IsSaving && oldProduct != fProduct && modified) {
                oldProduct = oldProduct ?? fProduct;
                oldProduct.UpdateOrdersCount(true);
                oldProduct.UpdateOrdersTotal(true);
                oldProduct.UpdateMaximumOrder(true);
            }
        }
    }
}

```

In the  **Product**  class, the  **OnLoaded**  method is overridden, as it is necessary to reset cached values when using “lazy” calculations.



```csharp
[DefaultClassOptions]
public class Product : BaseObject {
    // ...
    protected override void OnLoaded() {
        Reset();
        base.OnLoaded();
    }
    private void Reset() {
        fOrdersCount = null;
        fOrdersTotal = null;
        fMaximumOrder = null;
    }
    // ...

```

## Store Calculated Property Values in the Database

The non-persistent calculated properties can be inappropriate in certain scenarios, especially when a large number of objects should be manipulated. Each time such a property is accessed, a query to the database is generated to evaluate the property for each master object. For instance, suppose you have the  **Order**  business class which has the  **Total**  non-persistent property. This property is calculated from the properties of the objects contained in the  **Order**‘s child object collection. To display an  **Order**  object in a List View, the  **Total**  property’s value should be determined. To determine that value, a database query is generated. If the List View should display a thousand objects, a thousand queries will be generated. Obviously, this can have a negative impact on the performance of the application.

To avoid the performance issues, the calculated property values can be stored in the database. You can apply the  [PersistentAttribute](https://docs.devexpress.com/XPO/DevExpress.Xpo.PersistentAttribute)  to save values to the database (see  [How to: Use Read-Only Persistent Properties](https://docs.devexpress.com/XPO/2875/examples/how-to-use-read-only-persistent-properties)). Additionally, if it is assumed that the calculated property is to be used in a filter criterion or while sorting, the  [PersistentAliasAttribute](https://docs.devexpress.com/XPO/DevExpress.Xpo.PersistentAliasAttribute)  can be applied.



```csharp
[DefaultClassOptions]
public class Product : BaseObject {
    // ...
    [Persistent("OrdersCount")]
    private int? fOrdersCount = null;
    [PersistentAlias(nameof(fOrdersCount))]
    public int? OrdersCount {
        // ...
    }
    [Persistent("OrdersTotal")]
    private decimal? fOrdersTotal = null;
    [PersistentAlias(nameof(fOrdersTotal))]
    public decimal? OrdersTotal {
        // ...
    }
    [Persistent("MaximumOrder")]
    private decimal? fMaximumOrder = null;
    [PersistentAlias(nameof(fMaximumOrder))]
    public decimal? MaximumOrder {
        // ...
    }
    // ...

```
Remove the **OnLoaded** method overload from the master **Order** class.
