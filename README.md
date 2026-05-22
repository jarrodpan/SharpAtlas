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

## HTML visualiser navigation

The generated HTML visualiser supports:

- drag empty space to pan
- mouse wheel to zoom
- middle-click drag to pan
- zoom in/out/reset buttons
- fit graph
- fit selected node neighbourhood
- lock/unlock node positions
- keyboard shortcuts:
  - `+` / `=` zoom in
  - `-` zoom out
  - `0` reset zoom
  - `f` fit graph
  - arrow keys or `WASD` pan diagram
  - `Escape` clear selection

The default EntryPoint Flow layout places detected Program/Main-style entrypoints on the left and their dependencies to the right.

## Local Mermaid viewer

SharpAtlas generates a local Mermaid viewer when Mermaid output is requested:

```bash
dotnet run --project src/SharpAtlas -- --solution path/to/App.sln --output artifacts/architecture --format mermaid
```

This creates:

```text
class-graph.mmd
class-graph-mermaid.html
```

Open:

```text
class-graph-mermaid.html
```

This avoids online Mermaid viewer text limits because the Mermaid source is embedded directly into the local HTML file.

For very large codebases, the interactive `class-graph.html` visualiser is usually more practical than Mermaid.

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

`--group-by` supports `namespace`, `assembly`, `project`, or `namespace-hierarchy`. Use `csproj` as an alias for `project`, and `namespace-tree` as an alias for `namespace-hierarchy`. Default: `namespace`.

`--help` prints CLI help.

## Output files

SharpAtlas writes:

```text
class-graph.json
class-graph.mmd
class-graph.html
class-graph-mermaid.html
```

## JSON schema

`class-graph.json` is canonical output. It contains:

`source`: scanned solution or project path.

`options`: scan options used to generate graph.

`entrypoints`: best-effort entrypoint type IDs such as `Program`, `Startup`, `App`, `Application`, `HostBuilder`, static `Main` hosts, and top-level statement hosts.

`nodes`: C# types with `id`, `label`, `namespace`, `assembly`, `project`, `kind`, `file`, and `isExternal`.

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
