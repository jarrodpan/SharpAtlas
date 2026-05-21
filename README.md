# SharpAtlas

SharpAtlas generates class/type dependency maps for C# codebases.

It scans a C# solution or project using Roslyn and outputs architecture graph files that can be used for documentation, review, and future interactive visualization.

## Usage

```bash
dotnet run --project src/SharpAtlas -- --solution path/to/App.sln --output artifacts/architecture --format all

dotnet run --project src/SharpAtlas -- --project path/to/App.csproj --output artifacts/architecture --format json
```

## HTML visualiser

SharpAtlas can generate a local interactive HTML visualiser:

```bash
dotnet run --project src/SharpAtlas -- --solution path/to/App.sln --output artifacts/architecture --format html
```

Then open:

```text
artifacts/architecture/class-graph.html
```

The visualiser supports:

- zoom/pan
- node search
- node selection
- incoming/outgoing dependency highlighting
- path-to-entrypoint highlighting
- relationship filtering
- layout switching

## Future global tool usage

```bash
sharp-atlas --solution path/to/App.sln --output artifacts/architecture --format all
```

## Options

`--solution` scans a C# solution.

`--project` scans a C# project.

`--output` sets the output directory. Default: `artifacts/architecture`.

`--format` supports `json`, `mermaid`, `html`, or `all`. Default: `all`.

`--include-tests` includes test projects and test files.

`--include-external` includes external/library/framework type nodes.

`--relationship` filters edges by comma-separated values: `constructor`, `field`, `property`, `method-parameter`, `method-return`, `inherits`, `implements`, `generic`, `record-parameter`.

`--group-by` supports `namespace` or `assembly`. Default: `namespace`.

`--help` prints CLI help.

## Output files

SharpAtlas writes:

```text
class-graph.json
class-graph.mmd
class-graph.html
```

## JSON schema

`class-graph.json` is canonical output. It contains:

`source`: scanned solution or project path.

`options`: scan options used to generate graph.

`entrypoints`: best-effort entrypoint type IDs such as `Program`, `Startup`, `App`, `Application`, `HostBuilder`, static `Main` hosts, and top-level statement hosts.

`nodes`: C# types with `id`, `label`, `namespace`, `assembly`, `kind`, `file`, and `isExternal`.

`edges`: directed dependencies from requiring type to required type with a `relationship` value.

Relationship values: `constructor`, `field`, `property`, `method-parameter`, `method-return`, `inherits`, `implements`, `generic`, `record-parameter`.

## Roadmap

- interactive web viewer
- zoom/pan
- click node
- highlight incoming/outgoing dependencies
- highlight path from entrypoint
- collapse namespaces
- filter relationship types
- dependency cycle detection
- forbidden dependency rules
- compare graphs before/after refactor
