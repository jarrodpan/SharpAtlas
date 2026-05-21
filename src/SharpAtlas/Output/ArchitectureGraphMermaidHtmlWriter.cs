using System.Text;

namespace SharpAtlas.Output;

public static class ArchitectureGraphMermaidHtmlWriter
{
    public static string Write(string mermaidSource, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, "class-graph-mermaid.html");
        File.WriteAllText(path, Render(mermaidSource));
        return path;
    }

    public static string Render(string mermaidSource)
    {
        var sourceBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(mermaidSource));

        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>SharpAtlas Mermaid Viewer</title>
              <script src="https://cdn.jsdelivr.net/npm/mermaid/dist/mermaid.min.js"></script>
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
                  --accent: #7dd3fc;
                  --error: #f87171;
                }

                * { box-sizing: border-box; }
                html, body { height: 100%; margin: 0; }
                body {
                  display: grid;
                  grid-template-rows: auto auto 1fr;
                  overflow: hidden;
                  background: var(--bg);
                  color: var(--text);
                  font-family: ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
                }

                header {
                  display: flex;
                  align-items: center;
                  justify-content: space-between;
                  gap: 16px;
                  padding: 14px 18px;
                  border-bottom: 1px solid var(--line);
                  background: #0b1118;
                }

                h1 { margin: 0; font-size: 20px; letter-spacing: 0; }
                #summary {
                  display: flex;
                  flex-wrap: wrap;
                  gap: 12px;
                  color: var(--muted);
                  font-size: 13px;
                }

                #toolbar {
                  display: flex;
                  flex-wrap: wrap;
                  gap: 8px;
                  padding: 10px 14px;
                  border-bottom: 1px solid var(--line);
                  background: var(--panel);
                }

                button {
                  border: 1px solid var(--line);
                  border-radius: 6px;
                  background: var(--panel-2);
                  color: var(--text);
                  cursor: pointer;
                  font: inherit;
                  padding: 8px 10px;
                }

                button:hover { border-color: var(--accent); }

                main {
                  display: grid;
                  grid-template-columns: 1fr minmax(320px, 420px);
                  min-height: 0;
                }

                #viewport {
                  position: relative;
                  min-width: 0;
                  min-height: 0;
                  overflow: hidden;
                  background: var(--graph);
                  cursor: grab;
                }

                #viewport.dragging { cursor: grabbing; }

                #diagramHost {
                  position: absolute;
                  left: 0;
                  top: 0;
                  transform-origin: 0 0;
                  will-change: transform;
                }

                #diagramHost svg {
                  display: block;
                  max-width: none;
                }

                #side {
                  min-height: 0;
                  overflow: auto;
                  border-left: 1px solid var(--line);
                  background: var(--panel);
                }

                #sourcePanel, #errorPanel {
                  padding: 14px;
                  border-bottom: 1px solid var(--line);
                }

                #sourcePanel[hidden], #errorPanel[hidden] { display: none; }

                pre {
                  margin: 0;
                  overflow: auto;
                  white-space: pre-wrap;
                  word-break: break-word;
                  color: #d1d5db;
                  font-family: ui-monospace, SFMono-Regular, Consolas, "Liberation Mono", monospace;
                  font-size: 12px;
                  line-height: 1.45;
                }

                #errorPanel {
                  color: var(--error);
                }

                .hint {
                  padding: 14px;
                  color: var(--muted);
                  font-size: 13px;
                  line-height: 1.45;
                }

                @media (max-width: 1000px) {
                  main { grid-template-columns: 1fr; }
                  #side { display: none; }
                }
              </style>
            </head>
            <body>
              <header>
                <h1>SharpAtlas Mermaid Viewer</h1>
                <div id="summary"></div>
              </header>

              <div id="toolbar">
                <button id="renderButton">Render</button>
                <button id="fitButton">Fit</button>
                <button id="zoomInButton">Zoom In</button>
                <button id="zoomOutButton">Zoom Out</button>
                <button id="resetZoomButton">Reset Zoom</button>
                <button id="toggleSourceButton">Toggle Source</button>
                <button id="downloadSvgButton">Download SVG</button>
              </div>

              <main>
                <section id="viewport" aria-label="Rendered Mermaid diagram">
                  <div id="diagramHost"></div>
                </section>

                <aside id="side">
                  <section id="errorPanel" hidden>
                    <pre id="errorText"></pre>
                  </section>
                  <section id="sourcePanel" hidden>
                    <pre id="sourceText"></pre>
                  </section>
                  <div class="hint">
                    Large graphs may be too complex for Mermaid. Try using class-graph.html, the Cytoscape-based SharpAtlas visualiser, for large codebases.
                  </div>
                </aside>
              </main>

              <script>
                const mermaidSourceBase64 = "{{sourceBase64}}";
                const mermaidSource = decodeBase64Utf8(mermaidSourceBase64);
                const sourceFileName = 'class-graph.mmd';
                let scale = 1;
                let translateX = 30;
                let translateY = 30;
                let dragStart = null;

                const viewport = document.getElementById('viewport');
                const diagramHost = document.getElementById('diagramHost');
                const sourcePanel = document.getElementById('sourcePanel');
                const sourceText = document.getElementById('sourceText');
                const errorPanel = document.getElementById('errorPanel');
                const errorText = document.getElementById('errorText');

                mermaid.initialize({
                  startOnLoad: false,
                  securityLevel: 'loose',
                  theme: 'dark',
                  flowchart: {
                    useMaxWidth: false,
                    htmlLabels: true,
                    curve: 'basis'
                  }
                });

                sourceText.textContent = mermaidSource;
                document.getElementById('summary').innerHTML = [
                  `Source: ${sourceFileName}`,
                  `Lines: ${lineCount(mermaidSource)}`,
                  `Characters: ${mermaidSource.length.toLocaleString()}`
                ].map(value => `<span>${escapeHtml(value)}</span>`).join('');

                document.getElementById('renderButton').addEventListener('click', renderMermaid);
                document.getElementById('fitButton').addEventListener('click', fitDiagram);
                document.getElementById('zoomInButton').addEventListener('click', () => zoomBy(1.2));
                document.getElementById('zoomOutButton').addEventListener('click', () => zoomBy(1 / 1.2));
                document.getElementById('resetZoomButton').addEventListener('click', resetView);
                document.getElementById('toggleSourceButton').addEventListener('click', toggleSource);
                document.getElementById('downloadSvgButton').addEventListener('click', downloadSvg);

                viewport.addEventListener('wheel', event => {
                  event.preventDefault();
                  const factor = event.deltaY < 0 ? 1.12 : 1 / 1.12;
                  zoomAt(factor, event.clientX, event.clientY);
                }, { passive: false });

                viewport.addEventListener('pointerdown', event => {
                  viewport.setPointerCapture(event.pointerId);
                  dragStart = {
                    x: event.clientX,
                    y: event.clientY,
                    translateX,
                    translateY
                  };
                  viewport.classList.add('dragging');
                });

                viewport.addEventListener('pointermove', event => {
                  if (!dragStart) return;
                  translateX = dragStart.translateX + event.clientX - dragStart.x;
                  translateY = dragStart.translateY + event.clientY - dragStart.y;
                  applyTransform();
                });

                viewport.addEventListener('pointerup', event => {
                  if (dragStart) viewport.releasePointerCapture(event.pointerId);
                  dragStart = null;
                  viewport.classList.remove('dragging');
                });

                viewport.addEventListener('pointercancel', () => {
                  dragStart = null;
                  viewport.classList.remove('dragging');
                });

                renderMermaid();

                async function renderMermaid() {
                  try {
                    hideError();
                    diagramHost.innerHTML = '';
                    const rendered = await mermaid.render('sharpatlas-mermaid-diagram', mermaidSource);
                    diagramHost.innerHTML = typeof rendered === 'string' ? rendered : rendered.svg;
                    resetView();
                  } catch (error) {
                    showError(error);
                  }
                }

                function decodeBase64Utf8(base64) {
                  const binary = atob(base64);
                  const bytes = Uint8Array.from(binary, c => c.charCodeAt(0));
                  return new TextDecoder('utf-8').decode(bytes);
                }

                function zoomBy(factor) {
                  zoomAt(factor, viewport.clientWidth / 2, viewport.clientHeight / 2);
                }

                function zoomAt(factor, clientX, clientY) {
                  const previousScale = scale;
                  scale = Math.max(0.05, Math.min(5, scale * factor));
                  const rect = viewport.getBoundingClientRect();
                  const x = clientX - rect.left;
                  const y = clientY - rect.top;
                  translateX = x - (x - translateX) * (scale / previousScale);
                  translateY = y - (y - translateY) * (scale / previousScale);
                  applyTransform();
                }

                function fitDiagram() {
                  const svg = diagramHost.querySelector('svg');
                  if (!svg) return;

                  const width = getSvgWidth(svg);
                  const height = getSvgHeight(svg);
                  if (!width || !height) return;

                  scale = Math.min(viewport.clientWidth / width, viewport.clientHeight / height) * 0.92;
                  scale = Math.max(0.05, Math.min(5, scale));
                  translateX = Math.max(20, (viewport.clientWidth - width * scale) / 2);
                  translateY = Math.max(20, (viewport.clientHeight - height * scale) / 2);
                  applyTransform();
                }

                function resetView() {
                  scale = 1;
                  translateX = 30;
                  translateY = 30;
                  applyTransform();
                  setTimeout(fitDiagram, 0);
                }

                function applyTransform() {
                  diagramHost.style.transform = `translate(${translateX}px, ${translateY}px) scale(${scale})`;
                }

                function toggleSource() {
                  sourcePanel.hidden = !sourcePanel.hidden;
                }

                function downloadSvg() {
                  const svg = diagramHost.querySelector('svg');
                  if (!svg) {
                    showError('No SVG has been rendered yet. Click Render first.');
                    return;
                  }

                  const blob = new Blob([svg.outerHTML], { type: 'image/svg+xml;charset=utf-8' });
                  const url = URL.createObjectURL(blob);
                  const link = document.createElement('a');
                  link.href = url;
                  link.download = 'class-graph.svg';
                  document.body.appendChild(link);
                  link.click();
                  link.remove();
                  URL.revokeObjectURL(url);
                }

                function showError(error) {
                  errorPanel.hidden = false;
                  errorText.textContent = [
                    'Mermaid failed to render this diagram.',
                    '',
                    String(error?.stack || error?.message || error),
                    '',
                    'Large graphs may be too complex for Mermaid. Try using class-graph.html, the Cytoscape-based SharpAtlas visualiser, for large codebases.'
                  ].join('\n');
                }

                function hideError() {
                  errorPanel.hidden = true;
                  errorText.textContent = '';
                }

                function getSvgWidth(svg) {
                  const viewBox = svg.viewBox?.baseVal;
                  if (viewBox?.width) return viewBox.width;
                  return svg.getBoundingClientRect().width || Number.parseFloat(svg.getAttribute('width')) || 0;
                }

                function getSvgHeight(svg) {
                  const viewBox = svg.viewBox?.baseVal;
                  if (viewBox?.height) return viewBox.height;
                  return svg.getBoundingClientRect().height || Number.parseFloat(svg.getAttribute('height')) || 0;
                }

                function lineCount(value) {
                  return value.length === 0 ? 0 : value.split(/\r\n|\r|\n/).length;
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
}
