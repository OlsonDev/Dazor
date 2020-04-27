# Dazor

## Example usage
```shell
cd MyProject
# Apply all Version scripts and then Repeatable scripts
dazor migrate
# Synonym:
dazor upgrade
# Apply all Version scripts up to 18
dazor migrate to 18
# Synonym:
dazor upgrade to 18
# Generate .dazor.cs files
dazor gen
# Watch for .dazor file creation/changes and SQL schema changes, and generate .dazor.cs files
dazor watch
# Undo the last script, rerun it
dazor rerun last
# Undo everything down to 18 and including 18, then rerun to current
dazor rerun 18+
# Undo everything down to 21 and including 21, then only run to 23 even if there are later Version scripts
dazor rerun 21-23
# Undo everything down to and including 18
dazor undo 18+
# Synonym:
dazor downgrade to 17
# Generate a data seed (SQL file containing insert statements)
dazor seed <name>
# Clean data out of the database, insert records from seed file
dazor apply <seed-name>
# Get help information
dazor help
dazor help migrate
dazor help upgrade
dazor help gen
dazor help watch
dazor help rerun
dazor help undo
dazor help downgrade
```

## Automatic joins via foreign key detection
```sql
JOIN dbo.User        AS U
JOIN dbo.UserToRole  AS U2R
JOIN dbo.Role        AS R
```

## Flow control
```sql
@if (Model.DoThing) {
    JOIN ...
}

WHERE 1 = 1
@foreach (var filter in Filters) {
    AND ( @filter.Fragment )
}
```

## Automatic = @ScalarParameter vs. IN (SELECT Value FROM @TableValueParameter), !=/NOT IN w/proper NULL handling
```sql
JOIN dbo.Category  AS C  ON C.CategoryID @= @Model.CategoryIds
JOIN dbo.Type      AS T  ON T.TypeID @!= @Model.ExcludedTypeIds
```

## Redundant clause removal
The below example would result in no `WHERE` clause whatsoever.
```sql
@{
    var filters = Enumerable.Empty<Dazor.SqlFragment>();
}
WHERE 1 = 1
@foreach (var filter in filters) {
    AND ( @filter )
}
```

## Reusable fragments
```sql
@fragment MustBeSuperUser {
    JOIN dbo.User AS U ON U.UserID = @Common.CurrentUserId AND U.IsSuperUser = 1
}

@fragment Foo(ids) {
    JOIN dbo.Foo AS F ON F.FooIDs #ids
}
```

## Escape Dazor cleverness
```sql
-- Multi-line:
@Raw {
    JOIN dbo.TableWithSpecialConditions  AS TWSC ON TWSC.Blah = @Common.Blah
}
-- Or single-line:
!@ JOIN dbo.TableWithSpecialConditions  AS TWSC ON TWSC.Blah = @Common.Blah
```


## Example project structure - organize into folders however you wish
### `Product.cs` (represents a row returned by a query)
```cs
public class Product {
    public Guid ProductId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; }
}
```
### `CommonParameters.cs` (represents common parameters required by a dynamic/static query)
```cs
public class CommonQueryParameters {
  public Guid CurrentUserId { get; set;}
}
```
### `ProductQueryParameters.cs` (represents specific parameters required by a dynamic/static query)
```cs
public class ProductQueryParameters {
  public IList<Guid> CategoryIds { get; set;}
}
```
### `ProductQuery.dazor` (a dynamic/static query)
```sql
@Common: MyProject.CommonNamespace.CommonQueryParameters
@Product: ProductQueryParameters

JOIN dbo.UserOrder         AS UO   ON UO.UserID = @Common.CurrentUserId
JOIN dbo.UserOrderProduct  AS UOP
JOIN dbo.Product           AS P    ON P.CategoryID @= @Product.CategoryIds
```
### `ProductQuery.dazor.cs` (automatically generated high performance API)
```cs
public class ProductQuery {
    public static Task<IEnumerable<T>> GetAsync<T>(CommonQueryParameters commonQueryParameters, ProductQueryParameters productQueryParameters) {
        // Implementation details
    }

    public static Task<IEnumerable<T>> GetSingleAsync<T>(CommonQueryParameters commonQueryParameters, ProductQueryParameters productQueryParameters) {
        // Implementation details
    }

    // etc.
}
```

## All `.dazor` syntaxes in one place:
| Syntax                            | What                     |
|-----------------------------------|--------------------------|
| @:                                | Automatic parameter name |
| @VarName:                         | Parameter named VarName  |
| @{}                               | Arbitrary C# code block  |
| @foreach (var e in enumerator) {} | For each loop            |

## `appsettings.json` support (no `bool`s; use `enum`s for everything):
```json5
{
    "Dazor": {
        "AutoFromClause": "Off", // Off, On (default)
        "AutoJoins": "Off", // Off, ForeignKey (default), Convention
        "AutoParameterNameSuffix": "QueryParameters", // (default); allows for @: MyProject.CommonNamespace.CommonQueryParameters to be automatically named Common
    }
}
```

## Warmup all execution plans
More thought needed to be put into this, but the idea is look at all parameter types,
and execute the query with reasonable parameter value permutations. For a query to be
considered for warmup, an `@WarmUp` directive should exist after `@:`/`@VarName:` directives.
Examples:
```cs
@Common: MyProject.CommonNamespace.CommonQueryParameters
@Product: ProductQueryParameters
@WarmUp {
  // We're not okay with parameter sniffing, don't even run this permutation.
  if (Common.SomethingSqlServerParameterSniffs.Count == 1) return false;
  // We're fine with parameter sniffing here, but just pick a better value.
  Product.SomethingDazorPicksASillyWarmUpValueFor = "somethingbetter";
  return true;
}
```