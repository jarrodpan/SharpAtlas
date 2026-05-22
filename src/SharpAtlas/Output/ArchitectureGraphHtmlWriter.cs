using System.Text.Encodings.Web;
using System.Text.Json;
using SharpAtlas.Graph;

namespace SharpAtlas.Output;

public static class ArchitectureGraphHtmlWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.Default
    };

    public static string Write(ArchitectureGraph graph, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, "class-graph.html");
        File.WriteAllText(path, Render(graph));
        return path;
    }

    public static string Render(ArchitectureGraph graph)
    {
        var graphJson = EscapeJsonForScriptTag(JsonSerializer.Serialize(graph, JsonOptions));

        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>SharpAtlas</title>
              <script src="https://unpkg.com/cytoscape/dist/cytoscape.min.js"></script>
              <style>
                :root {
                  color-scheme: dark;
                  --bg: #090d12;
                  --panel: #111821;
                  --panel-2: #17212d;
                  --graph: #0d141d;
                  --text: #e5e7eb;
                  --muted: #93a4b7;
                  --line: #253244;
                  --accent: #f8d66d;
                  --incoming: #60a5fa;
                  --outgoing: #fb923c;
                  --path: #f472b6;
                  --entry: #34d399;
                }

                * { box-sizing: border-box; }
                html, body { height: 100%; margin: 0; }
                body {
                  font-family: ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
                  background: var(--bg);
                  color: var(--text);
                  overflow: hidden;
                }

                .shell {
                  display: grid;
                  grid-template-columns: minmax(280px, 360px) 1fr minmax(300px, 380px);
                  grid-template-rows: auto 1fr;
                  height: 100vh;
                }

                header {
                  grid-column: 1 / -1;
                  display: flex;
                  align-items: center;
                  justify-content: space-between;
                  gap: 16px;
                  padding: 14px 18px;
                  border-bottom: 1px solid var(--line);
                  background: #0b1118;
                }

                h1 { margin: 0; font-size: 20px; letter-spacing: 0; }
                .summary {
                  display: flex;
                  flex-wrap: wrap;
                  gap: 12px;
                  color: var(--muted);
                  font-size: 13px;
                }

                aside, .details {
                  min-height: 0;
                  overflow: auto;
                  background: var(--panel);
                }

                aside {
                  border-right: 1px solid var(--line);
                  padding: 16px;
                }

                .details {
                  border-left: 1px solid var(--line);
                  padding: 16px;
                }

                #cy {
                  width: 100%;
                  height: 100%;
                  background: var(--graph);
                  cursor: grab;
                }

                #cy.grabbing { cursor: grabbing; }

                label {
                  display: block;
                  margin: 14px 0 6px;
                  color: var(--muted);
                  font-size: 12px;
                  font-weight: 700;
                  text-transform: uppercase;
                }

                input, select, button {
                  width: 100%;
                  border: 1px solid var(--line);
                  background: var(--panel-2);
                  color: var(--text);
                  border-radius: 6px;
                  padding: 9px 10px;
                  font: inherit;
                }

                button {
                  cursor: pointer;
                  margin-top: 10px;
                }

                button:hover { border-color: #3b82f6; }
                .row { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }
                .row.three { grid-template-columns: 1fr 1fr 1fr; }
                .checks { display: grid; gap: 6px; margin-top: 8px; }
                .check {
                  display: grid;
                  grid-template-columns: 18px 1fr;
                  align-items: center;
                  gap: 8px;
                  color: #cbd5e1;
                  font-size: 13px;
                }
                .check input { width: auto; padding: 0; }
                .hint {
                  color: var(--muted);
                  font-size: 12px;
                  line-height: 1.45;
                  margin-top: 8px;
                }
                .message {
                  min-height: 20px;
                  margin-top: 12px;
                  color: var(--muted);
                  font-size: 13px;
                }
                .details h2 {
                  margin: 0 0 4px;
                  font-size: 18px;
                  overflow-wrap: anywhere;
                }
                .details .kind {
                  color: var(--muted);
                  font-size: 13px;
                  margin-bottom: 16px;
                }
                .field {
                  border-top: 1px solid var(--line);
                  padding: 10px 0;
                }
                .field .name {
                  color: var(--muted);
                  font-size: 12px;
                  font-weight: 700;
                  text-transform: uppercase;
                }
                .field .value {
                  margin-top: 4px;
                  overflow-wrap: anywhere;
                }
                .shortcut {
                  display: grid;
                  grid-template-columns: 54px 1fr;
                  gap: 8px;
                  margin-top: 6px;
                  color: #cbd5e1;
                  font-size: 12px;
                }
                kbd {
                  border: 1px solid var(--line);
                  border-radius: 4px;
                  background: #0b1118;
                  color: var(--text);
                  padding: 1px 5px;
                  text-align: center;
                }

                @media (max-width: 1100px) {
                  .shell { grid-template-columns: 300px 1fr; }
                  .details { display: none; }
                }
              </style>
            </head>
            <body>
              <div class="shell">
                <header>
                  <h1>SharpAtlas</h1>
                  <div class="summary">
                    <span id="node-count">Nodes: 0</span>
                    <span id="edge-count">Edges: 0</span>
                    <span id="entrypoint-count">Entrypoints: 0</span>
                  </div>
                </header>

                <aside>
                  <label for="search">Search</label>
                  <input id="search" type="search" placeholder="Type, namespace, project, assembly, file">
                  <div class="message" id="match-message"></div>

                  <label for="layout">Layout</label>
                  <select id="layout">
                    <option value="entrypoint-flow">entrypoint-flow</option>
                    <option value="breadthfirst-lr">breadthfirst-lr</option>
                    <option value="cose">cose</option>
                    <option value="concentric">concentric</option>
                    <option value="circle">circle</option>
                    <option value="grid">grid</option>
                  </select>
                  <div class="hint" id="layout-note">Entrypoint Flow places Program/Main-style entrypoints on the left and dependencies to the right.</div>

                  <label for="group-mode">Color by</label>
                  <select id="group-mode">
                    <option value="namespace">namespace</option>
                    <option value="assembly">assembly</option>
                    <option value="project">project</option>
                    <option value="namespace-root">namespace-root</option>
                  </select>

                  <label>Navigate</label>
                  <div class="row">
                    <button id="fit">Fit graph</button>
                    <button id="fit-selected">Fit Selected</button>
                  </div>
                  <button id="center-selected">Center Selected</button>
                  <div class="row three">
                    <button id="zoom-out">Zoom Out</button>
                    <button id="zoom-in">Zoom In</button>
                    <button id="reset-zoom">Reset Zoom</button>
                  </div>
                  <label class="check" for="lock-nodes">
                    <input id="lock-nodes" type="checkbox" checked>
                    <span>Lock node positions</span>
                  </label>
                  <button id="reset">Reset selection</button>
                  <button id="path">Highlight path to entrypoint</button>
                  <div class="message" id="status"></div>

                  <label>Relationships</label>
                  <div class="checks" id="relationship-filters"></div>

                  <label>Shortcuts</label>
                  <div class="shortcut"><kbd>+</kbd><span>Zoom in</span></div>
                  <div class="shortcut"><kbd>-</kbd><span>Zoom out</span></div>
                  <div class="shortcut"><kbd>0</kbd><span>Reset zoom</span></div>
                  <div class="shortcut"><kbd>f</kbd><span>Fit graph</span></div>
                  <div class="shortcut"><kbd>arrows</kbd><span>Pan diagram</span></div>
                  <div class="shortcut"><kbd>WASD</kbd><span>Pan diagram</span></div>
                  <div class="shortcut"><kbd>Esc</kbd><span>Clear selection/search</span></div>
                </aside>

                <main id="cy"></main>

                <section class="details" id="details">
                  <h2>No node selected</h2>
                  <div class="kind">Click a node to inspect dependencies.</div>
                </section>
              </div>

              <script id="graph-data" type="application/json">
            {{graphJson}}
              </script>
              <script>
                const graph = JSON.parse(document.getElementById('graph-data').textContent);
                const relationships = [...new Set(graph.edges.map(edge => edge.relationship))].sort();
                const visibleRelationships = new Set(relationships);
                let selectedNodeId = null;
                let currentSearch = '';
                let middlePan = null;
                const defaultLayout = defaultLayoutName();

                document.getElementById('node-count').textContent = `Nodes: ${graph.nodes.length}`;
                document.getElementById('edge-count').textContent = `Edges: ${graph.edges.length}`;
                document.getElementById('entrypoint-count').textContent = `Entrypoints: ${graph.entrypoints.length}`;

                const elements = [
                  ...graph.nodes.map(node => ({
                    group: 'nodes',
                    data: {
                      id: node.id,
                      label: node.label,
                      namespace: node.namespace || '',
                      assembly: node.assembly || '',
                      project: node.project || '',
                      kind: node.kind || '',
                      file: node.file || '',
                      isExternal: Boolean(node.isExternal),
                      isEntrypoint: graph.entrypoints.includes(node.id)
                    }
                  })),
                  ...graph.edges.map((edge, index) => ({
                    group: 'edges',
                    data: {
                      id: `edge-${index}`,
                      source: edge.from,
                      target: edge.to,
                      relationship: edge.relationship
                    }
                  }))
                ];

                const cy = cytoscape({
                  container: document.getElementById('cy'),
                  elements,
                  wheelSensitivity: 0.25,
                  minZoom: 0.05,
                  maxZoom: 4,
                  userZoomingEnabled: true,
                  userPanningEnabled: true,
                  boxSelectionEnabled: false,
                  autoungrabify: false,
                  autounselectify: false,
                  style: [
                    {
                      selector: 'node',
                      style: {
                        'background-color': ele => colorFor(groupValue(ele)),
                        'border-color': '#314158',
                        'border-width': 2,
                        'color': '#e5e7eb',
                        'font-size': 11,
                        'height': 34,
                        'label': 'data(label)',
                        'overlay-opacity': 0,
                        'shape': 'round-rectangle',
                        'text-background-color': '#0d141d',
                        'text-background-opacity': 0.75,
                        'text-background-padding': 2,
                        'text-valign': 'center',
                        'width': labelWidth,
                        'cursor': 'pointer'
                      }
                    },
                    {
                      selector: 'edge',
                      style: {
                        'curve-style': 'bezier',
                        'line-color': '#5f7187',
                        'opacity': 0.72,
                        'target-arrow-color': '#5f7187',
                        'target-arrow-shape': 'triangle',
                        'width': 1.4,
                        'label': 'data(relationship)',
                        'font-size': 8,
                        'color': '#9ca3af',
                        'text-background-color': '#0d141d',
                        'text-background-opacity': 0.8,
                        'text-background-padding': 1
                      }
                    },
                    { selector: 'node.entrypoint', style: { 'border-color': '#34d399', 'border-width': 4 } },
                    { selector: 'node.external', style: { 'border-style': 'dashed', 'opacity': 0.72 } },
                    { selector: '.selected', style: { 'background-color': '#f8d66d', 'border-color': '#fef3c7', 'color': '#111827', 'z-index': 90 } },
                    { selector: '.outgoing', style: { 'background-color': '#fb923c', 'line-color': '#fb923c', 'target-arrow-color': '#fb923c', 'opacity': 1, 'z-index': 80 } },
                    { selector: '.incoming', style: { 'background-color': '#60a5fa', 'line-color': '#60a5fa', 'target-arrow-color': '#60a5fa', 'opacity': 1, 'z-index': 80 } },
                    { selector: '.path', style: { 'background-color': '#f472b6', 'line-color': '#f472b6', 'target-arrow-color': '#f472b6', 'opacity': 1, 'z-index': 100, 'width': 3 } },
                    { selector: '.searchMatch', style: { 'border-color': '#a78bfa', 'border-width': 4 } },
                    { selector: '.dimmed', style: { 'opacity': 0.14 } },
                    { selector: '.hiddenByRelationship', style: { 'display': 'none' } }
                  ],
                  layout: { name: 'preset' }
                });

                cy.nodes().forEach(node => {
                  if (node.data('isEntrypoint')) node.addClass('entrypoint');
                  if (node.data('isExternal')) node.addClass('external');
                });

                renderRelationshipFilters();
                updateDetails(null);
                document.getElementById('layout').value = defaultLayout;
                runLayout(defaultLayout);

                cy.on('tap', 'node', event => selectNode(event.target.id()));
                cy.on('tap', event => {
                  if (event.target === cy) resetSelection();
                });
                cy.on('mousedown', event => {
                  if (event.target === cy) document.getElementById('cy').classList.add('grabbing');
                });
                cy.on('mouseup mouseout', () => document.getElementById('cy').classList.remove('grabbing'));
                const cyHost = document.getElementById('cy');
                cyHost.addEventListener('mousedown', event => {
                  if (event.button !== 1) return;
                  event.preventDefault();
                  middlePan = {
                    startX: event.clientX,
                    startY: event.clientY,
                    pan: { ...cy.pan() }
                  };
                  cyHost.classList.add('grabbing');
                });
                cyHost.addEventListener('auxclick', event => {
                  if (event.button === 1) event.preventDefault();
                });
                window.addEventListener('mousemove', event => {
                  if (!middlePan) return;
                  event.preventDefault();
                  cy.pan({
                    x: middlePan.pan.x + event.clientX - middlePan.startX,
                    y: middlePan.pan.y + event.clientY - middlePan.startY
                  });
                });
                window.addEventListener('mouseup', event => {
                  if (event.button !== 1 || !middlePan) return;
                  middlePan = null;
                  cyHost.classList.remove('grabbing');
                });

                document.getElementById('fit').addEventListener('click', fitGraph);
                document.getElementById('fit-selected').addEventListener('click', fitSelected);
                document.getElementById('center-selected').addEventListener('click', centerSelected);
                document.getElementById('zoom-in').addEventListener('click', zoomIn);
                document.getElementById('zoom-out').addEventListener('click', zoomOut);
                document.getElementById('reset-zoom').addEventListener('click', resetZoom);
                document.getElementById('reset').addEventListener('click', resetSelection);
                document.getElementById('path').addEventListener('click', highlightPathToEntrypoint);
                document.getElementById('layout').addEventListener('change', event => runLayout(event.target.value));
                document.getElementById('group-mode').addEventListener('change', () => cy.style().update());
                document.getElementById('lock-nodes').addEventListener('change', event => setNodeLock(event.target.checked));
                document.getElementById('search').addEventListener('input', event => {
                  currentSearch = event.target.value.trim().toLowerCase();
                  applySearch();
                });
                document.getElementById('search').addEventListener('keydown', event => {
                  if (event.key !== 'Enter') return;
                  const first = cy.nodes('.searchMatch').first();
                  if (first.nonempty()) {
                    selectNode(first.id());
                    cy.animate({ center: { eles: first }, zoom: Math.max(cy.zoom(), 1.1) }, { duration: 250 });
                  }
                });
                document.addEventListener('keydown', handleKeyboardShortcut);

                function renderRelationshipFilters() {
                  const host = document.getElementById('relationship-filters');
                  host.innerHTML = '';
                  for (const relationship of relationships) {
                    const id = `rel-${relationship}`;
                    const label = document.createElement('label');
                    label.className = 'check';
                    label.htmlFor = id;
                    label.innerHTML = `<input id="${id}" type="checkbox" checked> <span>${escapeHtml(relationship)}</span>`;
                    label.querySelector('input').addEventListener('change', event => {
                      if (event.target.checked) visibleRelationships.add(relationship);
                      else visibleRelationships.delete(relationship);
                      applyRelationshipFilters();
                      if (selectedNodeId) selectNode(selectedNodeId, false);
                    });
                    host.appendChild(label);
                  }
                }

                function applyRelationshipFilters() {
                  cy.edges().forEach(edge => edge.toggleClass('hiddenByRelationship', !visibleRelationships.has(edge.data('relationship'))));
                  document.getElementById('edge-count').textContent = `Edges: ${cy.edges().not('.hiddenByRelationship').length}`;
                }

                function selectNode(id, clearExistingPath = true) {
                  selectedNodeId = id;
                  if (clearExistingPath) clearPath();
                  const node = cy.getElementById(id);
                  clearHighlights();
                  node.addClass('selected');

                  const outgoingEdges = node.outgoers('edge').not('.hiddenByRelationship');
                  const incomingEdges = node.incomers('edge').not('.hiddenByRelationship');
                  const outgoingNodes = outgoingEdges.targets();
                  const incomingNodes = incomingEdges.sources();
                  outgoingEdges.addClass('outgoing');
                  outgoingNodes.addClass('outgoing');
                  incomingEdges.addClass('incoming');
                  incomingNodes.addClass('incoming');

                  cy.elements().not(node.union(outgoingEdges).union(outgoingNodes).union(incomingEdges).union(incomingNodes)).addClass('dimmed');
                  applySearch();
                  updateDetails(node);
                  setStatus('');
                }

                function clearHighlights() {
                  cy.elements().removeClass('selected outgoing incoming dimmed');
                }

                function clearPath() {
                  cy.elements().removeClass('path');
                }

                function resetSelection() {
                  selectedNodeId = null;
                  clearHighlights();
                  clearPath();
                  applySearch();
                  updateDetails(null);
                  setStatus('');
                }

                function clearSearch() {
                  currentSearch = '';
                  const search = document.getElementById('search');
                  search.value = '';
                  cy.elements().removeClass('searchMatch dimmed');
                  document.getElementById('match-message').textContent = '';
                }

                function updateDetails(node) {
                  const details = document.getElementById('details');
                  if (!node) {
                    details.innerHTML = '<h2>No node selected</h2><div class="kind">Click a node to inspect dependencies.</div>';
                    return;
                  }

                  const incoming = node.incomers('edge').not('.hiddenByRelationship').length;
                  const outgoing = node.outgoers('edge').not('.hiddenByRelationship').length;
                  const rows = [
                    ['label', node.data('label')],
                    ['id', node.id()],
                    ['namespace', node.data('namespace')],
                    ['assembly', node.data('assembly')],
                    ['project', node.data('project')],
                    ['kind', node.data('kind')],
                    ['file', node.data('file') || ''],
                    ['isExternal', String(node.data('isExternal'))],
                    ['incoming count', String(incoming)],
                    ['outgoing count', String(outgoing)]
                  ];

                  details.innerHTML = `<h2>${escapeHtml(node.data('label'))}</h2><div class="kind">${escapeHtml(node.data('kind'))}</div>` +
                    rows.map(([name, value]) => `<div class="field"><div class="name">${escapeHtml(name)}</div><div class="value">${escapeHtml(value || '')}</div></div>`).join('');
                }

                function highlightPathToEntrypoint() {
                  clearPath();
                  if (!selectedNodeId) {
                    setStatus('Select a node first.');
                    return;
                  }
                  if (!graph.entrypoints.length) {
                    setStatus('No entrypoints detected.');
                    return;
                  }

                  const path = shortestPath(graph.entrypoints, selectedNodeId);
                  if (!path) {
                    setStatus('No path found from an entrypoint.');
                    return;
                  }

                  for (const nodeId of path.nodes) cy.getElementById(nodeId).addClass('path');
                  for (const edgeId of path.edges) cy.getElementById(edgeId).addClass('path');
                  setStatus(`Path length: ${path.edges.length}`);
                }

                function shortestPath(starts, targetId) {
                  const existingStarts = starts.filter(id => cy.getElementById(id).nonempty());
                  const queue = existingStarts.map(id => ({ id, nodes: [id], edges: [] }));
                  const seen = new Set(existingStarts);

                  while (queue.length) {
                    const current = queue.shift();
                    if (current.id === targetId) return current;

                    const node = cy.getElementById(current.id);
                    const edges = node.outgoers('edge').not('.hiddenByRelationship');
                    for (const edge of edges) {
                      const next = edge.target().id();
                      if (seen.has(next)) continue;
                      seen.add(next);
                      queue.push({
                        id: next,
                        nodes: [...current.nodes, next],
                        edges: [...current.edges, edge.id()]
                      });
                    }
                  }

                  return null;
                }

                function applySearch() {
                  cy.elements().removeClass('searchMatch');
                  if (!currentSearch) {
                    document.getElementById('match-message').textContent = '';
                    return;
                  }

                  const matches = cy.nodes().filter(node => searchableText(node).includes(currentSearch));
                  matches.addClass('searchMatch');
                  document.getElementById('match-message').textContent = `${matches.length} match${matches.length === 1 ? '' : 'es'}`;

                  if (!selectedNodeId) {
                    cy.elements().addClass('dimmed');
                    matches.removeClass('dimmed');
                  }
                }

                function searchableText(node) {
                  return [
                    node.data('label'),
                    node.id(),
                    node.data('namespace'),
                    node.data('assembly'),
                    node.data('project'),
                    node.data('file')
                  ].join(' ').toLowerCase();
                }

                function runLayout(name) {
                  setLayoutNote(name);
                  if (name === 'entrypoint-flow') {
                    runEntrypointFlowLayout();
                    return;
                  }

                  const layout = cy.layout(layoutOptions(name));
                  layout.on('layoutstop', () => {
                    if (name === 'breadthfirst-lr') rotateLayoutLeftToRight();
                    finishLayout();
                  });
                  layout.run();
                }

                function layoutOptions(name) {
                  const common = { name, animate: true, fit: true, padding: 60 };
                  if (name === 'breadthfirst-lr') return {
                    ...common,
                    name: 'breadthfirst',
                    directed: true,
                    roots: flowRoots(),
                    spacingFactor: 1.5,
                    padding: 100,
                    circle: false,
                    grid: false,
                    avoidOverlap: true,
                    nodeDimensionsIncludeLabels: true
                  };
                  if (name === 'cose') return { ...common, idealEdgeLength: 110, nodeOverlap: 18, randomize: false };
                  if (name === 'concentric') return { ...common, minNodeSpacing: 40 };
                  return common;
                }

                function defaultLayoutName() {
                  const rootIds = new Set(graph.entrypoints || []);
                  const hasEntrypointNode = graph.nodes.some(node => rootIds.has(node.id));
                  return hasEntrypointNode ? 'entrypoint-flow' : 'breadthfirst-lr';
                }

                function flowRoots() {
                  const rootIds = new Set(graph.entrypoints || []);
                  let roots = cy.nodes().filter(node => rootIds.has(node.id()));
                  if (roots.nonempty()) return roots;

                  roots = cy.nodes().filter(node => node.indegree(false) === 0);
                  if (roots.length > 0 && roots.length <= 40) return roots;
                  return cy.nodes().slice(0, Math.min(20, cy.nodes().length));
                }

                function runEntrypointFlowLayout() {
                  const roots = flowRoots();
                  const rootIds = new Set(roots.map(node => node.id()));
                  const depths = computeDepths(rootIds);
                  const reachedNodes = cy.nodes().filter(node => depths.has(node.id()));
                  const unreachedNodes = cy.nodes().difference(reachedNodes);
                  const horizontalSpacing = 280;
                  const verticalSpacing = 110;
                  const buckets = new Map();

                  for (const node of reachedNodes) {
                    const depth = depths.get(node.id()) ?? 0;
                    if (!buckets.has(depth)) buckets.set(depth, []);
                    buckets.get(depth).push(node);
                  }

                  let maxY = 0;
                  const sortedDepths = [...buckets.keys()].sort((a, b) => a - b);
                  for (const depth of sortedDepths) {
                    const bucket = buckets.get(depth).sort(compareNodes);
                    const startY = -(bucket.length - 1) * verticalSpacing / 2;
                    bucket.forEach((node, index) => {
                      const y = startY + index * verticalSpacing;
                      node.position({ x: depth * horizontalSpacing, y });
                      maxY = Math.max(maxY, Math.abs(y));
                    });
                  }

                  const unreachedStartY = maxY + 250;
                  const unreached = unreachedNodes.toArray().sort(compareNodes);
                  unreached.forEach((node, index) => {
                    const column = index % 5;
                    const row = Math.floor(index / 5);
                    node.position({
                      x: column * horizontalSpacing,
                      y: unreachedStartY + row * verticalSpacing
                    });
                  });

                  finishLayout();
                }

                function computeDepths(rootIds) {
                  const depths = new Map();
                  const queue = [];
                  for (const rootId of rootIds) {
                    if (cy.getElementById(rootId).empty()) continue;
                    depths.set(rootId, 0);
                    queue.push(rootId);
                  }

                  while (queue.length) {
                    const currentId = queue.shift();
                    const currentDepth = depths.get(currentId) ?? 0;
                    const outgoingEdges = cy.getElementById(currentId).outgoers('edge').not('.hiddenByRelationship');
                    for (const edge of outgoingEdges) {
                      const nextId = edge.target().id();
                      const nextDepth = currentDepth + 1;
                      if (depths.has(nextId) && depths.get(nextId) <= nextDepth) continue;
                      depths.set(nextId, nextDepth);
                      queue.push(nextId);
                    }
                  }

                  return depths;
                }

                function rotateLayoutLeftToRight() {
                  cy.nodes().forEach(node => {
                    const oldX = node.position('x');
                    const oldY = node.position('y');
                    node.position({ x: oldY, y: oldX });
                  });
                }

                function finishLayout() {
                  setNodeLock(true);
                  cy.fit(undefined, 60);
                }

                function setNodeLock(locked) {
                  document.getElementById('lock-nodes').checked = locked;
                  if (locked) cy.nodes().ungrabify();
                  else cy.nodes().grabify();
                }

                function compareNodes(a, b) {
                  return `${a.data('namespace')} ${a.data('label')} ${a.id()}`
                    .localeCompare(`${b.data('namespace')} ${b.data('label')} ${b.id()}`);
                }

                function setLayoutNote(name) {
                  const note = document.getElementById('layout-note');
                  if (name === 'entrypoint-flow') {
                    const roots = flowRoots();
                    const hasEntrypoint = roots.some(node => node.data('isEntrypoint'));
                    note.textContent = hasEntrypoint
                      ? 'Entrypoint Flow places Program/Main-style entrypoints on the left and dependencies to the right.'
                      : 'No entrypoints detected; using dependency roots instead.';
                    return;
                  }

                  note.textContent = name === 'breadthfirst-lr'
                    ? 'Breadthfirst LR uses dependency roots and places dependencies toward the right.'
                    : 'Layout changes keep current filters and selection.';
                }

                function zoomBy(factor) {
                  const nextZoom = Math.max(cy.minZoom(), Math.min(cy.maxZoom(), cy.zoom() * factor));
                  cy.animate({ zoom: nextZoom, center: { eles: selectedElementOrGraph() } }, { duration: 120 });
                }

                function zoomIn() {
                  zoomBy(1.2);
                }

                function zoomOut() {
                  zoomBy(1 / 1.2);
                }

                function resetZoom() {
                  cy.animate({ zoom: 1, center: { eles: selectedElementOrGraph() } }, { duration: 180 });
                }

                function fitGraph() {
                  cy.fit(undefined, 40);
                }

                function selectedElementOrGraph() {
                  return selectedNodeId ? cy.getElementById(selectedNodeId) : cy.elements();
                }

                function fitSelected() {
                  if (!selectedNodeId) {
                    setStatus('Select a node first.');
                    return;
                  }

                  const selected = cy.getElementById(selectedNodeId);
                  cy.fit(selected.closedNeighborhood().not('.hiddenByRelationship'), 60);
                }

                function centerSelected() {
                  if (!selectedNodeId) {
                    setStatus('Select a node first.');
                    return;
                  }

                  cy.center(cy.getElementById(selectedNodeId));
                }

                function handleKeyboardShortcut(event) {
                  const tag = document.activeElement?.tagName?.toLowerCase();
                  if (tag === 'input' || tag === 'select' || tag === 'textarea') return;

                  const panStep = event.shiftKey ? 180 : 80;
                  const key = event.key.toLowerCase();

                  if (event.key === '+' || event.key === '=') {
                    event.preventDefault();
                    zoomIn();
                  } else if (event.key === '-') {
                    event.preventDefault();
                    zoomOut();
                  } else if (event.key === '0') {
                    event.preventDefault();
                    resetZoom();
                  } else if (key === 'f') {
                    event.preventDefault();
                    fitGraph();
                  } else if (event.key === 'ArrowUp' || key === 'w') {
                    event.preventDefault();
                    panDiagram(0, panStep);
                  } else if (event.key === 'ArrowDown' || key === 's') {
                    event.preventDefault();
                    panDiagram(0, -panStep);
                  } else if (event.key === 'ArrowLeft' || key === 'a') {
                    event.preventDefault();
                    panDiagram(panStep, 0);
                  } else if (event.key === 'ArrowRight' || key === 'd') {
                    event.preventDefault();
                    panDiagram(-panStep, 0);
                  } else if (event.key === 'Escape') {
                    event.preventDefault();
                    resetSelection();
                    clearSearch();
                  }
                }

                function panDiagram(x, y) {
                  cy.panBy({ x, y });
                }

                function labelWidth(ele) {
                  return Math.max(58, Math.min(180, String(ele.data('label') || '').length * 8 + 24));
                }

                function groupValue(ele) {
                  const mode = document.getElementById('group-mode').value;
                  if (mode === 'namespace-root') {
                    const namespaceName = ele.data('namespace') || '';
                    const parts = namespaceName.split('.').filter(Boolean);
                    return parts.length >= 2 ? `${parts[0]}.${parts[1]}` : (parts[0] || 'Global');
                  }

                  return ele.data(mode) || 'Global';
                }

                function colorFor(value) {
                  let hash = 0;
                  for (let i = 0; i < value.length; i++) hash = ((hash << 5) - hash + value.charCodeAt(i)) | 0;
                  const hue = Math.abs(hash) % 360;
                  return `hsl(${hue}, 52%, 42%)`;
                }

                function setStatus(message) {
                  document.getElementById('status').textContent = message;
                }

                function escapeHtml(value) {
                  return String(value)
                    .replaceAll('&', '&amp;')
                    .replaceAll('<', '&lt;')
                    .replaceAll('>', '&gt;')
                    .replaceAll('"', '&quot;')
                    .replaceAll("'", '&#39;');
                }
              </script>
            </body>
            </html>
            """;
    }

    internal static string EscapeJsonForScriptTag(string json)
    {
        return json
            .Replace("<", "\\u003C", StringComparison.Ordinal)
            .Replace(">", "\\u003E", StringComparison.Ordinal)
            .Replace("&", "\\u0026", StringComparison.Ordinal)
            .Replace("\u2028", "\\u2028", StringComparison.Ordinal)
            .Replace("\u2029", "\\u2029", StringComparison.Ordinal);
    }
}
