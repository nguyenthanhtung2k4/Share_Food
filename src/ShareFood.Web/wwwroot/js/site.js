document.addEventListener("DOMContentLoaded", () => {
    initializeToasts();
    initializeRecipeForm();
});

function initializeToasts() {
    const toastElements = document.querySelectorAll(".toast");

    toastElements.forEach((element) => {
        const toast = bootstrap.Toast.getOrCreateInstance(element, {
            delay: 3500
        });

        toast.show();
    });
}

function initializeRecipeForm() {
    const form = document.querySelector("[data-recipe-form]");
    if (!form) {
        return;
    }

    const ingredientList = form.querySelector("[data-ingredient-list]");
    const stepList = form.querySelector("[data-step-list]");
    const ingredientTemplate = document.getElementById("ingredient-row-template");
    const stepTemplate = document.getElementById("step-row-template");
    const addIngredientButton = form.querySelector("[data-add-ingredient]");
    const addStepButton = form.querySelector("[data-add-step]");

    addIngredientButton?.addEventListener("click", () => {
        const index = ingredientList.querySelectorAll("[data-ingredient-row]").length;
        ingredientList.insertAdjacentHTML("beforeend", ingredientTemplate.innerHTML.replaceAll("__index__", index.toString()));
    });

    addStepButton?.addEventListener("click", () => {
        const index = stepList.querySelectorAll("[data-step-row]").length;
        stepList.insertAdjacentHTML("beforeend", stepTemplate.innerHTML.replaceAll("__index__", index.toString()));
        reindexStepRows(stepList);
    });

    form.addEventListener("click", (event) => {
        const target = event.target;
        if (!(target instanceof HTMLElement)) {
            return;
        }

        if (!target.matches("[data-remove-row]")) {
            return;
        }

        const row = target.closest("[data-ingredient-row], [data-step-row]");
        row?.remove();
        reindexIngredientRows(ingredientList);
        reindexStepRows(stepList);
    });

    form.addEventListener("submit", () => {
        reindexIngredientRows(ingredientList);
        reindexStepRows(stepList);
    });

    reindexIngredientRows(ingredientList);
    reindexStepRows(stepList);
}

function reindexIngredientRows(container) {
    if (!container) {
        return;
    }

    const rows = container.querySelectorAll("[data-ingredient-row]");
    rows.forEach((row, index) => {
        renameIndexedFields(row, "Ingredients", index);
    });
}

function reindexStepRows(container) {
    if (!container) {
        return;
    }

    const rows = container.querySelectorAll("[data-step-row]");
    rows.forEach((row, index) => {
        renameIndexedFields(row, "Steps", index);

        const title = row.querySelector(".step-card__index");
        if (title) {
            title.textContent = `Bước ${index + 1}`;
        }

        const stepNumber = row.querySelector('input[name$=".StepNumber"]');
        if (stepNumber) {
            stepNumber.value = index + 1;
        }
    });
}

function renameIndexedFields(row, prefix, index) {
    const fields = row.querySelectorAll("input, select, textarea, span, label");
    fields.forEach((field) => {
        ["name", "id", "for"].forEach((attribute) => {
            const value = field.getAttribute(attribute);
            if (!value) {
                return;
            }

            const updated = value.replace(new RegExp(`${prefix}\\[(\\d+)\\]`, "g"), `${prefix}[${index}]`);
            field.setAttribute(attribute, updated);
        });

        const validation = field.getAttribute("data-valmsg-for");
        if (validation) {
            field.setAttribute("data-valmsg-for", validation.replace(new RegExp(`${prefix}\\[(\\d+)\\]`, "g"), `${prefix}[${index}]`));
        }
    });
}
