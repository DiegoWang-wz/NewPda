// wwwroot/js/mermaidFold.js
(function () {
    let dotnetRef = null;
    let idMap = {};

    // Mermaid flowchart click callback (called by: click X call mermaidNodeClick("X"))
    window.mermaidNodeClick = function (mid) {
        try {
            if (!dotnetRef) return;
            dotnetRef.invokeMethodAsync("ToggleNodeByMermaidId", mid);
        } catch (e) {
            console.error("mermaidNodeClick invoke failed:", e);
        }
    };

    async function safeRender(elementId, mermaidText, mapJson) {
        const el = document.getElementById(elementId);
        if (!el) return;

        el.innerHTML = "";

        if (!window.mermaid) {
            el.innerHTML = `<div style="color:#d32f2f;">Mermaid 未加载，请引入 mermaid.min.js</div>`;
            return;
        }

        try {
            idMap = mapJson ? JSON.parse(mapJson) : {};
        } catch {
            idMap = {};
        }

        try {
            window.mermaid.initialize({
                startOnLoad: false,
                securityLevel: "loose", // 允许 click 回调
                flowchart: { useMaxWidth: false }
            });
        } catch {
            // ignore
        }

        const timeoutMs = 5000;

        const renderPromise = (async () => {
            const uniqueId = "mmd_" + Math.random().toString(36).slice(2);
            const out = await window.mermaid.render(uniqueId, mermaidText);

            const svg = (out && out.svg) ? out.svg : out;
            el.innerHTML = svg || "";

            if (out && typeof out.bindFunctions === "function") {
                try { out.bindFunctions(el); } catch { }
            }

            const svgEl = el.querySelector("svg");
            if (svgEl) {
                svgEl.style.maxWidth = "none";
                svgEl.style.height = "auto";
            }
        })();

        const timeoutPromise = new Promise((resolve) => setTimeout(resolve, timeoutMs));

        await Promise.race([renderPromise, timeoutPromise]).catch((e) => {
            console.error("Mermaid render failed:", e);
            el.innerHTML = `<div style="color:#d32f2f;">Mermaid 渲染失败：${(e && e.message) ? e.message : e}</div>`;
        });
    }

    window.mermaidFold = {
        bind: function (ref) { dotnetRef = ref; },
        clear: function (elementId) {
            const el = document.getElementById(elementId);
            if (el) el.innerHTML = "";
        },
        render: async function (elementId, mermaidText, mapJson) {
            try {
                await safeRender(elementId, mermaidText, mapJson);
            } catch (e) {
                console.error("mermaidFold.render error:", e);
                const el = document.getElementById(elementId);
                if (el) el.innerHTML = `<div style="color:#d32f2f;">渲染异常：${(e && e.message) ? e.message : e}</div>`;
            }
        }
    };
})();
