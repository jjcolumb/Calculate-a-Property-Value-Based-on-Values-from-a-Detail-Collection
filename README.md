[![en](https://img.shields.io/badge/lang-en-red.svg)](https://github.com/jjcolumb/Calculate-a-Property-Value-Based-on-Values-from-a-Detail-Collection/blob/master/README.en.md)

# Calcular un valor de propiedad basado en valores de una colección de detalles


En este tema se describe cómo implementar una clase de negocio, de modo que una de sus propiedades se calcule en función de una propiedad de los objetos contenidos en la colección de objetos secundarios.

![image](https://github.com/jjcolumb/Calculate-a-Property-Value-Based-on-Values-from-a-Detail-Collection/assets/126447472/0924be67-8dd4-4b96-a603-90ee70cafe4a)

>PROPINA
Un proyecto de ejemplo completo está disponible en la base de datos de ejemplos de código de DevExpress en [https://supportcenter.Devexpress.  com/ticket/details/e305/how-to-calculate-a-master-property-based-on-values-from-a-details-collection](https://supportcenter.devexpress.com/ticket/details/e305/how-to-calculate-a-master-property-based-on-values-from-a-details-collection) .

## Implementación de la clase inicial

Una clase  **Product**  tiene una colección de objetos  **Order**. Las clases Product y Order están asociadas por la relación  [One-to-Many](https://docs.devexpress.com/eXpressAppFramework/112654/business-model-design-orm/business-model-design-with-xpo/relationships-between-persistent-objects-in-code-and-ui), lo que significa que  un objeto  **Product**  puede estar asociado a varios objetos  **Order**.  Se agrega la colección de objetos  **Order**.  **Se**  crean objetos de pedido, pertenecientes a uno de los objetos  **Product**. Cuando se quita el objeto maestro, también se quitan todos los objetos de su colección agregada.

El siguiente fragmento de código ilustra la implementación de la clase  **Product**.


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

El siguiente fragmento de código ilustra la implementación de la clase  **Order**.


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

En el código anterior, la clase  **Order**  contiene la propiedad Total y la clase  **Product**  tiene las propiedades  **MaximumOrder**  y  **OrdersTotal**.  Las propiedades de estos  **Productos**  se calculan en función de las propiedades  **totales**  de los  **Pedidos agregados**. La propiedad  **OrderCount**  también se agrega a la clase  **Product**. Esta propiedad expone el número de  **pedidos agregados**.

>NOTA
Puede modificar un objeto de la colección secundaria en una vista de detalles independiente y guardarlo. En este escenario, el objeto primario también puede marcarse como modificado en un espacio de objetos independiente. Si la propiedad de colección no está decorada con el  [atributo agregado](https://docs.devexpress.com/XPO/DevExpress.Xpo.AggregatedAttribute), debe actualizar el objeto primario antes de guardar los cambios. Para evitar esto, deshabilite el [valor predeterminado de Xpo.Esobjetomodificadoenla opción Cambio de propiedadnopersistente](https://docs.devexpress.com/XPO/DevExpress.Xpo.XpoDefault.IsObjectModifiedOnNonPersistentPropertyChange) antes de iniciar la aplicación.

## Implementar propiedades calculadas no persistentes

En esta sección se describe una implementación de propiedades calculadas "perezosas" (calculadas a petición).

Omita el configurador de propiedades para implementar una propiedad no persistente. El siguiente fragmento de código muestra la implementación de tres propiedades calculadas:  **OrdersCount**,  **OrdersTotal**  y  **MaximumOrder**.



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

La lógica empresarial de las propiedades se divide en tres métodos independientes:  **UpdateOrdersCount**,  **UpdateOrdersTotal**  y  **UpdateMaximumOrder**. Estos métodos se invocan en los getters de propiedades. Tener la lógica de negocios en métodos separados permite actualizar el valor de una propiedad llamando al método correspondiente, cuando sea necesario.  **OrdersCount**  es una propiedad no persistente calculada simple. Esta propiedad se calcula utilizando  **el lenguaje de criterios XPO**.  **OrdersTotal**  y  **MaximumOrder**  son propiedades no persistentes calculadas complejas, que no se expresan mediante el lenguaje de criterios. Por lo tanto, recorra la colección  **Orders**  para calcular estas propiedades.

NOTA

En este tema, las propiedades  **Total de pedidos** y **Orden máxima**  se  consideran complejas para ilustrar cómo se calculan dichas propiedades. En realidad, sus valores se pueden calcular fácilmente utilizando el lenguaje de criterios  **XPO**. Por ejemplo, puede utilizar las funciones  **Avg**,  **Count**,  **Exists**, **Max** y **Min**  para realizar operaciones agregadas en colecciones. Consulte el tema  [Sintaxis del lenguaje de criterios](https://docs.devexpress.com/CoreLibraries/4928/devexpress-data-library/criteria-language-syntax) para obtener más información.

El siguiente fragmento de código ilustra las definiciones de los métodos  **UpdateOrdersCount**,  **UpdateOrdersTotal**  y  **UpdateMaximumOrder**.


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

Tenga en cuenta que fOrdersCount se evalúa en el lado del cliente utilizando los objetos cargados desde una caché  **XPO**  interna en el método  **UpdateOrdersCount**.  Puede utilizar el código siguiente para evaluar  **fOrdersCount**  en el servidor, de modo que no se tengan en cuenta los objetos no confirmados.


```csharp
fOrdersCount = Convert.ToInt32(Session.Evaluate<Product>(CriteriaOperator.Parse("Orders.Count"), 
    CriteriaOperator.Parse("Oid=?", Oid)));

```

En los establecedores de propiedades  **Total**  y  **Product**  de la clase Order, una interfaz de usuario se actualiza cuando cambian los valores de propiedad de un objeto  **Order**  y no se está inicializando un objeto:



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

En la clase  **Product**, se reemplaza el método  **OnLoaded**, ya que es necesario restablecer los valores almacenados en caché cuando se utilizan cálculos "diferidos".



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

## Almacenar valores de propiedad calculados en la base de datos

Las propiedades calculadas no persistentes pueden ser inapropiadas en ciertos escenarios, especialmente cuando se debe manipular un gran número de objetos. Cada vez que se accede a una propiedad de este tipo, se genera una consulta a la base de datos para evaluar la propiedad de cada objeto maestro. Por ejemplo, supongamos que tiene la clase de negocio  **Order**  que tiene la propiedad  **Total**  no persistente. Esta propiedad se calcula a partir de las propiedades de los objetos contenidos en la colección de objetos secundarios de  **Order**. Para mostrar un objeto  **Order**  en una vista de lista, se debe determinar el valor de la propiedad  **Total**. Para determinar ese valor, se genera una consulta de base de datos. Si la vista de lista muestra mil objetos, se generarán mil consultas. Obviamente, esto puede tener un impacto negativo en el rendimiento de la aplicación.

Para evitar los problemas de rendimiento, los valores de propiedad calculados se pueden almacenar en la base de datos. Puede aplicar  [PersistentAttribute](https://docs.devexpress.com/XPO/DevExpress.Xpo.PersistentAttribute)  para guardar valores en la base de datos (vea  [Cómo: Usar propiedades persistentes de solo lectura](https://docs.devexpress.com/XPO/2875/examples/how-to-use-read-only-persistent-properties)). Además, si se supone que la propiedad calculada se va a utilizar en un criterio de filtro o durante la ordenación, se puede aplicar  [PersistentAliasAttribute](https://docs.devexpress.com/XPO/DevExpress.Xpo.PersistentAliasAttribute).


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

Quite la sobrecarga del método  **OnLoaded**  de la clase master  **Order**.
