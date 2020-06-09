# Dazor

## Example usage
```shell
cd MyProject
# Initialize a new Dazor project
dazor init

# CODE GENERATION COMMANDS:
# Generate .dazor.cs files.
dazor gen
# Watch for .dazor file creation/changes and SQL schema changes, and generate .dazor.cs files.
dazor watch

# VERSIONING COMMANDS:
# Note ALL commands run new/modified repeatable scripts after performing their versioning operation(s).
# Keep in mind, your newly-versioned database may not be compatible with your repeatable scripts -- usually
# this is more likely if you're downgrading.

# Apply all version scripts.
# Dazor will error if no change is necessary.
dazor upgrade
# Apply all version scripts up to 18.
# Dazor will error if there aren't at least 18 version scripts.
# Dazor will error if the current version is >= 18.
dazor upgrade to 18
# Inspect the database's current version and then either upgrade or downgrade to the given version.
# Dazor will error if there aren't at least 18 version scripts.
# Dazor will error if no change is necessary.
dazor migrate to 18
# Undo the last script, rerun it.
# Dazor will error if there is no version to rerun.
dazor rerun last
# Undo everything down to 18 and including 18, then upgrade.
# Dazor will error if there aren't at least 18 version scripts.
# Dazor will error if the current version is < 18.
dazor rerun 18+
# Undo everything down to 21 and including 21, then only run to 23 even if there are later version scripts.
# Dazor will error if there aren't at least 23 version scripts.
# Dazor will error if the current version is < 23.
dazor rerun 21-23
# Undo everything down to and including 18.
# Dazor will error if there aren't at least 18 version scripts.
# Dazor will error if current version is < 18.
dazor undo 18+
# Synonym (with same error conditions):
dazor downgrade to 17

# SEED COMMANDS:
# Generate a data seed (SQL file containing insert statements reflecting the current database).
dazor new-seed <name>
# Clean data out of the database, then insert records from the given seed file.
# Dazor will error if there is no seed with the given name.
dazor apply-seed <seed-name>
# Fix a database seed for a specific version; useful if the schema has changed since seeds were generated.
# Dazor will error if there are no saved seeds.
dazor fix-seeds

# SCRUBBING COMMANDS:
# Remove all objects from the database.
dazor clean-schema
# Truncate/delete all data out of the database.
dazor clean-data

# HELP COMMANDS:
dazor help

dazor help gen
dazor help watch

dazor help upgrade
dazor help migrate
dazor help rerun
dazor help undo
dazor help downgrade

dazor help new-seed
dazor help apply-seed
dazor help fix-seeds

dazor help clean-schema
dazor help clean-data
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
    JOIN dbo.Foo AS F ON F.FooIDs @= @#ids
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

## `dazor.json` support (no `bool`s; use `enum`s for everything):
```json5
{
    "connectionString": "Data Source=localhost;Initial Catalog=YourAppDatabase;Integrated Security=True",
    "rootDirectory": "./SQL", // (default); where migrations and seeds will be put, relative to this dazor.json file
    "autoFromClause": "off", // off, on (default)
    "autoJoinClause": "off", // off, fk or foreignKey (default), convention
    "autoParameterNameSuffix": "QueryParameters", // (default); allows for @: MyProject.CommonNamespace.CommonQueryParameters to be automatically named Common
    "gitHook": "on", // off, on (default); if enabled, switching branches backs up DB to {old-branch}.sql and then cleans the schema, fully upgrades, then restores {new-branch}.sql if it exists, else uses {default-seed}.sql.
    "defaultSeed": "default", // The name of the default seed to use when switching branches, among other operations.
}
```

## Warm-up all execution plans
More thought needed to be put into this, but the idea is look at all parameter types,
and execute the query with reasonable parameter value permutations. For a query to be
considered for warm-up, an `@WarmUp` directive should exist after `@:`/`@VarName:` directives.
Example:
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

JOIN dbo.UserOrder         AS UO   ON UO.UserID = @Common.CurrentUserId
-- etc.
```
#### TODO: Discuss how to add Dazor to project and startup.