# Kinetix pour .NET 10

Kinetix est un ensemble de modules destiné à enrichir une application .NET de fonctionnalités transverses pour simplifier certaines opérations courantes. Ces modules sont relativement indépendants et peuvent donc être intégrés unitairement.

Ci-dessous, la liste des fonctionnalités disponibles par module :

## `Kinetix.Services`

Divisé en trois modules :

- [`Kinetix.Services`](https://www.nuget.org/packages/Kinetix.Services) [![NuGet](https://badgen.net/nuget/v/Kinetix.Services)](https://www.nuget.org/packages/Kinetix.Services) —Gestion de contextes de transaction, gestion des listes de référence avec cache et enregistrement des services par annotations.
- [`Kinetix.Services.Annotations`](https://www.nuget.org/packages/Kinetix.Services.Annotations) [![NuGet](https://badgen.net/nuget/v/Kinetix.Services.Annotations)](https://www.nuget.org/packages/Kinetix.Services.Annotations) — annotations `[RegisterContract]`, `[RegisterImpl]` et `[ReferenceAccessor]` utilisées par le module principal.
- [`Kinetix.Services.DependencyInjection`](https://www.nuget.org/packages/Kinetix.Services.DependencyInjection) [![NuGet](https://badgen.net/nuget/v/Kinetix.Services.DependencyInjection)](https://www.nuget.org/packages/Kinetix.Services.DependencyInjection) — Intercepteurs pour les transactions et les traces de service (optionnel, nécessite [AspectCore](https://github.com/dotnetcore/AspectCore-Framework))

## `Kinetix.Modeling` [![NuGet](https://badgen.net/nuget/v/Kinetix.Modeling)](https://www.nuget.org/packages/Kinetix.Modeling)

- Utilitaires pour explorer les définitions d'objets métier
- Définitions d'erreurs métier

## `Kinetix.Search`

Divisé en trois modules :

- [`Kinetix.Search.Models`](https://www.nuget.org/packages/Kinetix.Search.Models) [![NuGet](https://badgen.net/nuget/v/Kinetix.Search.Models)](https://www.nuget.org/packages/Kinetix.Search.Models) — modèles d’entrée et de sortie, critères, tri, facettes et résultats de recherche.
- [`Kinetix.Search.Core`](https://www.nuget.org/packages/Kinetix.Search.Core) [![NuGet](https://badgen.net/nuget/v/Kinetix.Search.Core)](https://www.nuget.org/packages/Kinetix.Search.Core) — contrats et services pour la recherche avancée à facettes, définition des documents et gestion de l’indexation. La mise à jour des indexes est liée aux transactions Kinetix.
- [`Kinetix.Search.Elastic`](https://www.nuget.org/packages/Kinetix.Search.Elastic) [![NuGet](https://badgen.net/nuget/v/Kinetix.Search.Elastic)](https://www.nuget.org/packages/Kinetix.Search.Elastic) — implémentation du moteur de recherche basée sur la pile Elasticsearch/NEST 7.

## `Kinetix.Reporting`

Divisé en trois modules :

- [`Kinetix.Reporting.Annotations`](https://www.nuget.org/packages/Kinetix.Reporting.Annotations) [![NuGet](https://badgen.net/nuget/v/Kinetix.Reporting.Annotations)](https://www.nuget.org/packages/Kinetix.Reporting.Annotations) — annotations de formatage pour les valeurs booléennes, dates et nombres.
- [`Kinetix.Reporting.Core`](https://www.nuget.org/packages/Kinetix.Reporting.Core) [![NuGet](https://badgen.net/nuget/v/Kinetix.Reporting.Core)](https://www.nuget.org/packages/Kinetix.Reporting.Core) — construction de rapports et de classeurs Excel à partir de modèles.
- [`Kinetix.Reporting.Web`](https://www.nuget.org/packages/Kinetix.Reporting.Web) [![NuGet](https://badgen.net/nuget/v/Kinetix.Reporting.Web)](https://www.nuget.org/packages/Kinetix.Reporting.Web) — intégration ASP.NET Core pour télécharger les rapports

## `Kinetix.User` [![NuGet](https://badgen.net/nuget/v/Kinetix.User)](https://www.nuget.org/packages/Kinetix.User)

- Abstraction pour accéder à l'utilisateur connecté (pour ne pas toujours devoir utilisé celui de `HttpContext.User`)

## `Kinetix.Web` [![NuGet](https://badgen.net/nuget/v/Kinetix.Web)](https://www.nuget.org/packages/Kinetix.Web)

- Filtres d'endpoint (culture, transaction, vérification des références, dates UTC)
- Gestion centralisée des exceptions,
- Configuration JSON
- Utilitaires OpenTelemetry.

## `Kinetix.EFCore`

Divisé en 2 modules :

- [`Kinetix.EFCore`](https://www.nuget.org/packages/Kinetix.EFCore) [![NuGet](https://badgen.net/nuget/v/Kinetix.EFCore)](https://www.nuget.org/packages/Kinetix.EFCore) — participation d’Entity Framework Core aux transactions Kinetix.
- [`Kinetix.EFCore.Npgsql`](https://www.nuget.org/packages/Kinetix.EFCore.Npgsql) [![NuGet](https://badgen.net/nuget/v/Kinetix.EFCore.Npgsql)](https://www.nuget.org/packages/Kinetix.EFCore.Npgsql) — extensions spécifiques à Npgsql et configuration de ses traces OpenTelemetry.

## `Kinetix.DataAccess.Sql`

Divisé en trois modules :

- `Kinetix.DataAccess.Sql` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.DataAccess.Sql)](https://www.nuget.org/packages/Kinetix.DataAccess.Sql)
- `Kinetix.DataAccess.Sql.SqlServer` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.DataAccess.Sql.SqlServer)](https://www.nuget.org/packages/Kinetix.DataAccess.Sql.SqlServer)
- `Kinetix.DataAccess.Sql.Postgres` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.DataAccess.Sql.Postgres)](https://www.nuget.org/packages/Kinetix.DataAccess.Sql.Postgres)

Il s'agit d'un ORM "legacy" (utilisez plutôt [EF Core](https://docs.microsoft.com/en-us/ef/core/) sur vos nouveaux projets), avec une implémentation pour SQL Server et PostgreSQL.

Fonctionnalités :

- Requêtes SQL dynamiques (via une syntaxe spéciale)
- "Broker" pour gérer du CRUD simple sur des modèles persistés.
