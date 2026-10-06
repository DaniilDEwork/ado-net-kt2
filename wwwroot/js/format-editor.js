const textArea = document.getElementById("formatText");
const preview = document.getElementById("preview");
const formatStatus = document.getElementById("formatStatus");

function updatePreview() {
    let text = textArea.value
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;");

    text = text.replace(
        /&lt;(\/?)(b|i|u)&gt;/g,
        "<$1$2>"
    );

    preview.innerHTML = text;
}

document.querySelectorAll("[data-tag]").forEach(button => {
    button.addEventListener("click", () => {
        const start = textArea.selectionStart;
        const end = textArea.selectionEnd;

        if (start === end) {
            formatStatus.textContent = "Сначала выделите текст";
            return;
        }

        const tag = button.dataset.tag;
        const selectedText = textArea.value.slice(start, end);
        const replacement = `<${tag}>${selectedText}</${tag}>`;

        textArea.setRangeText(replacement, start, end, "select");
        textArea.focus();

        formatStatus.textContent = "";
        updatePreview();
    });
});

textArea.addEventListener("input", updatePreview);

updatePreview();