(() => {
    const form = document.getElementById("consulta-tipo");
    const typeSelect = document.getElementById("tipo-automovil");
    const message = document.getElementById("mensaje-consulta");
    const list = document.getElementById("lista-automoviles");
    const panel = document.getElementById("panel-ingresos");
    const panelTitle = document.getElementById("titulo-ingresos");
    const entries = document.getElementById("tabla-ingresos");

    const showError = async (response) => {
        try {
            const problem = await response.json();
            return problem.detail || problem.title || `Error HTTP ${response.status}.`;
        } catch {
            return `Error HTTP ${response.status}.`;
        }
    };

    const addCell = (row, value) => {
        const cell = document.createElement("td");
        cell.textContent = value;
        row.appendChild(cell);
    };

    const formatDate = (value) => value
        ? new Date(value).toLocaleString()
        : "En curso";

    const showEntries = async (automovil) => {
        panel.hidden = false;
        panelTitle.textContent = `Ingresos del automóvil ${automovil.fabricante} ${automovil.anio} (ID ${automovil.id})`;
        entries.replaceChildren();
        entries.textContent = "Consultando ingresos...";
        message.textContent = "";

        try {
            const url = new URL(form.dataset.ingresosUrl, window.location.origin);
            url.searchParams.set("id", automovil.id);
            const response = await fetch(url);
            if (!response.ok) {
                throw new Error(await showError(response));
            }

            const records = await response.json();
            if (records.length === 0) {
                entries.textContent = "Este automóvil no tiene ingresos registrados.";
                return;
            }

            const table = document.createElement("table");
            const header = document.createElement("thead");
            const headerRow = document.createElement("tr");
            [
                "ID del parqueo",
                "Fecha y hora de entrada",
                "Fecha y hora de salida",
                "Duración (minutos)",
                "Duración (horas)",
                "Monto a cancelar"
            ].forEach((label) => {
                const cell = document.createElement("th");
                cell.scope = "col";
                cell.textContent = label;
                headerRow.appendChild(cell);
            });
            header.appendChild(headerRow);
            table.appendChild(header);

            const body = document.createElement("tbody");
            records.forEach((record) => {
                const row = document.createElement("tr");
                addCell(row, record.id_parqueo);
                addCell(row, formatDate(record.fecha_hora_entrada));
                addCell(row, formatDate(record.fecha_hora_salida));
                addCell(row, record.duracion_estadia_minutos ?? "Pendiente");
                addCell(row, record.duracion_estadia_horas ?? "Pendiente");
                addCell(row, record.monto_total_pagar == null
                    ? "Pendiente"
                    : Number(record.monto_total_pagar).toLocaleString(undefined, {
                        style: "currency",
                        currency: "CRC"
                    }));
                body.appendChild(row);
            });
            table.appendChild(body);
            entries.replaceChildren(table);
        } catch (error) {
            entries.replaceChildren();
            message.textContent = error.message;
        }
    };

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        list.replaceChildren();
        entries.replaceChildren();
        panel.hidden = true;
        message.textContent = "";

        if (!typeSelect.value) {
            message.textContent = "Selecciona un tipo de automóvil.";
            return;
        }

        const url = new URL(form.dataset.url, window.location.origin);
        url.searchParams.set("tipo", typeSelect.value);
        message.textContent = "Consultando automóviles...";

        try {
            const response = await fetch(url);
            if (!response.ok) {
                throw new Error(await showError(response));
            }

            const automoviles = await response.json();
            if (automoviles.length === 0) {
                message.textContent = "No hay automóviles registrados de ese tipo.";
                return;
            }

            message.textContent = `Se encontraron ${automoviles.length} automóvil(es). Selecciona uno para consultar sus ingresos.`;
            const table = document.createElement("table");
            const header = document.createElement("thead");
            const headerRow = document.createElement("tr");
            ["ID", "Color", "Año", "Fabricante", "Tipo", "Ingresos"].forEach((label) => {
                const cell = document.createElement("th");
                cell.scope = "col";
                cell.textContent = label;
                headerRow.appendChild(cell);
            });
            header.appendChild(headerRow);
            table.appendChild(header);

            const body = document.createElement("tbody");
            automoviles.forEach((automovil) => {
                const row = document.createElement("tr");
                addCell(row, automovil.id);
                addCell(row, automovil.color);
                addCell(row, automovil.anio);
                addCell(row, automovil.fabricante);
                addCell(row, automovil.tipo);
                const actionCell = document.createElement("td");
                const selectButton = document.createElement("button");
                selectButton.type = "button";
                selectButton.textContent = "Ver ingresos";
                selectButton.addEventListener("click", () => showEntries(automovil));
                actionCell.appendChild(selectButton);
                row.appendChild(actionCell);
                body.appendChild(row);
            });
            table.appendChild(body);
            list.replaceChildren(table);
        } catch (error) {
            message.textContent = error.message;
        }
    });
})();
