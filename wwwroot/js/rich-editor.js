const form = document.getElementById("richForm");
const savedContent = document.getElementById("savedContent");
const saveButton = document.getElementById("saveRich");
const richStatus = document.getElementById("richStatus");

const imageFile = document.getElementById("imageFile");
const imageWidth = document.getElementById("imageWidth");
const imageAlign = document.getElementById("imageAlign");

const applyImage = document.getElementById("applyImage");
const deleteImage = document.getElementById("deleteImage");

if (typeof Quill === "undefined") {
    richStatus.textContent =
        "Не загружена библиотека Quill. Проверьте файл quill.js";
} else {
    const BaseImage = Quill.import("formats/image");

    class SizedImage extends BaseImage {
        static formats(node) {
            const formats = super.formats(node);

            if (node.hasAttribute("width")) {
                formats.width = node.getAttribute("width");
            }

            return formats;
        }

        format(name, value) {
            if (name === "width") {
                if (value) {
                    this.domNode.setAttribute("width", value);
                } else {
                    this.domNode.removeAttribute("width");
                }
            } else {
                super.format(name, value);
            }
        }
    }

    Quill.register(SizedImage, true);

    const quill = new Quill("#richEditor", {
        theme: "snow",
        placeholder: "Введите текст...",
        modules: {
            toolbar: [
                ["bold", "italic", "underline"],
                [{ header: [1, 2, false] }],
                [{ list: "ordered" }, { list: "bullet" }]
            ]
        }
    });

    let selectedImage = null;

    function resetImageSelection() {
        selectedImage = null;
        applyImage.disabled = true;
        deleteImage.disabled = true;
    }

    function getSelectedImageIndex() {
        if (!selectedImage || !quill.root.contains(selectedImage)) {
            resetImageSelection();
            return null;
        }

        const blot = Quill.find(selectedImage);
        return quill.getIndex(blot);
    }

    let loaded = true;

    if (savedContent.value.trim() !== "") {
        try {
            quill.setContents(JSON.parse(savedContent.value));
        } catch {
            loaded = false;
            richStatus.textContent =
                "Не удалось загрузить сохранённый документ";
        }
    }

    saveButton.disabled = !loaded;

    form.addEventListener("submit", event => {
        const content = JSON.stringify(quill.getContents());

        if (content.length > 1_000_000) {
            event.preventDefault();

            richStatus.textContent =
                "Документ слишком большой. Уменьшите число изображений";

            return;
        }

        savedContent.value = content;
    });

    quill.root.addEventListener("click", event => {
        if (event.target instanceof HTMLImageElement) {
            selectedImage = event.target;

            applyImage.disabled = false;
            deleteImage.disabled = false;

            richStatus.textContent = "Изображение выбрано";
        } else {
            resetImageSelection();
        }
    });

    imageFile.addEventListener("change", () => {
        const file = imageFile.files[0];

        if (!file) {
            return;
        }

        const allowedTypes = ["image/png", "image/jpeg", "image/webp"];

        if (!allowedTypes.includes(file.type)) {
            richStatus.textContent = "Выберите PNG, JPEG или WebP";
            imageFile.value = "";
            return;
        }

        if (file.size > 300 * 1024) {
            richStatus.textContent = "Размер изображения больше 300 КБ";
            imageFile.value = "";
            return;
        }

        const range = quill.getSelection();
        const index = range ? range.index : quill.getLength() - 1;

        const reader = new FileReader();

        reader.addEventListener("load", () => {
            quill.insertText(index, "\n", "user");
            quill.insertEmbed(index + 1, "image", reader.result, "user");
            quill.insertText(index + 2, "\n", "user");

            quill.formatText(index + 1, 1, "width", "300", "user");
            quill.setSelection(index + 3, 0);

            richStatus.textContent =
                "Изображение добавлено. Не забудьте сохранить документ";

            imageFile.value = "";
        });

        reader.addEventListener("error", () => {
            richStatus.textContent = "Не удалось прочитать изображение";
            imageFile.value = "";
        });

        reader.readAsDataURL(file);
    });

    applyImage.addEventListener("click", () => {
        const index = getSelectedImageIndex();

        if (index === null) {
            return;
        }

        quill.formatText(
            index,
            1,
            "width",
            imageWidth.value,
            "user"
        );

        quill.formatLine(
            index,
            1,
            "align",
            imageAlign.value || false,
            "user"
        );

        richStatus.textContent =
            "Размер и положение изменены. Сохраните документ";
    });

    deleteImage.addEventListener("click", () => {
        const index = getSelectedImageIndex();

        if (index === null) {
            return;
        }

        quill.deleteText(index, 1, "user");
        resetImageSelection();

        richStatus.textContent =
            "Изображение удалено. Сохраните документ";
    });
}