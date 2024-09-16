document.addEventListener('DOMContentLoaded', function () {
    var dateInput = document.getElementById('incident-date');
    var now = new Date();

    var year = now.getFullYear();
    var month = String(now.getMonth() + 1).padStart(2, '0');
    var day = String(now.getDate()).padStart(2, '0');
    var hours = String(now.getHours()).padStart(2, '0');
    var minutes = String(now.getMinutes()).padStart(2, '0');

    var formattedDate = `${year}-${month}-${day}T${hours}:${minutes}`;

    if (dateInput) {
        dateInput.value = formattedDate;
    }

    var MAX_FILES = 10;

    function handlePaste(event) {
        event.preventDefault();
        const items = event.clipboardData.items;
        for (let i = 0; i < items.length; i++) {
            const item = items[i];
            if (item.kind === 'file' && item.type.startsWith('image/')) {
                const file = item.getAsFile();
                displayImage(file);
            }
        }
    }

    // Делаем handleFileSelect доступной глобально
    window.handleFileSelect = function handleFileSelect(event) {
        const files = event.target.files;
        if (files.length + document.querySelectorAll('.file-preview .file-item').length > MAX_FILES) {
            alert('Можно добавить не более 10 файлов.');
            return;
        }

        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            if (file.type.startsWith('image/')) {
                displayImage(file);
            } else {
                displayFile(file);
            }
        }

        // Автоматически нажать кнопку "Сохранить" после выбора файла
        setTimeout(function () {
            document.querySelector('button[type="submit"]').click();
        }, 100); // Задержка в 100 мс для уверенности, что файл выбран перед отправкой формы
    };

    function displayImage(file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const filePreviewDiv = document.getElementById('filePreview');
            if (!filePreviewDiv) {
                console.error('filePreview элемент не найден');
                return;
            }

            const fileItem = document.createElement('div');
            fileItem.className = 'file-item';

            const img = document.createElement('img');
            img.src = e.target.result;
            img.className = 'mini-image';
            img.onclick = function () {
                showFullImage(e.target.result);
            };

            const fileName = document.createElement('div');
            fileName.className = 'file-name';
            fileName.textContent = file.name;

            const deleteBtn = document.createElement('button');
            deleteBtn.className = 'delete-btn';
            deleteBtn.onclick = function (event) {
                event.stopPropagation();
                fileItem.remove();
            };

            const svgElement = createSVGIcon("M17 7L7 17M7 7L17 17");
            deleteBtn.appendChild(svgElement);

            fileItem.appendChild(img);
            fileItem.appendChild(fileName);
            fileItem.appendChild(deleteBtn);
            filePreviewDiv.appendChild(fileItem);
        };

        reader.readAsDataURL(file);
    }

    function displayFile(file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const filePreviewDiv = document.getElementById('filePreview');
            if (!filePreviewDiv) {
                console.error('filePreview элемент не найден');
                return;
            }

            const fileItem = document.createElement('div');
            fileItem.className = 'file-item';

            const fileIcon = document.createElement('div');
            fileIcon.className = 'file-icon';
            fileIcon.title = file.name;
            fileIcon.onclick = function () {
                openFile(e.target.result, file.type);
            };

            const fileName = document.createElement('div');
            fileName.className = 'file-name';
            fileName.style.wordBreak = 'break-all';
            fileName.textContent = file.name;

            const deleteBtn = document.createElement('button');
            deleteBtn.className = 'delete-btn';
            deleteBtn.onclick = function (event) {
                event.stopPropagation();
                fileItem.remove();
            };

            const svgElement = createSVGIcon("M17 7L7 17M7 7L17 17");
            deleteBtn.appendChild(svgElement);

            fileItem.appendChild(fileIcon);
            fileItem.appendChild(fileName);
            fileItem.appendChild(deleteBtn);
            filePreviewDiv.appendChild(fileItem);
        };

        reader.readAsDataURL(file);
    }

    function createSVGIcon(pathData) {
        const svgElement = document.createElementNS("http://www.w3.org/2000/svg", "svg");
        svgElement.setAttribute("viewBox", "0 0 24 24");
        svgElement.setAttribute("xmlns", "http://www.w3.org/2000/svg");
        svgElement.setAttribute("class", "icon_delete_item");

        const pathElement = document.createElementNS("http://www.w3.org/2000/svg", "path");
        pathElement.setAttribute("d", pathData);

        svgElement.appendChild(pathElement);

        return svgElement;
    }

    function getFileIcon(fileType) {
        if (fileType.includes('text/')) return "M14 2.26953V6.40007C14 6.96012 14 7.24015 14.109 7.45406C14.2049 7.64222 14.3578 7.7952 14.546 7.89108C14.7599 8.00007 15.0399 8.00007 15.6 8.00007H19.7305M14 17H8M16 13H8M20 9.98822V17.2C20 18.8802 20 19.7202 19.673 20.362C19.3854 20.9265 18.9265 21.3854 18.362 21.673C17.7202 22 16.8802 22 15.2 22H8.8C7.11984 22 6.27976 22 5.63803 21.673C5.07354 21.3854 4.6146 20.9265 4.32698 20.362C4 19.7202 4 18.8802 4 17.2V6.8C4 5.11984 4 4.27976 4.32698 3.63803C4.6146 3.07354 5.07354 2.6146 5.63803 2.32698C6.27976 2 7.11984 2 8.8 2H12.0118C12.7455 2 13.1124 2 13.4577 2.08289C13.7638 2.15638 14.0564 2.27759 14.3249 2.44208C14.6276 2.6276 14.887 2.88703 15.4059 3.40589L18.5941 6.59411C19.113 7.11297 19.3724 7.3724 19.5579 7.67515C19.7224 7.94356 19.8436 8.2362 19.9171 8.5423C20 8.88757 20 9.25445 20 9.98822Z";
        if (fileType.includes('application/vnd.openxmlformats-officedocument.spreadsheetml.sheet')) return "M14 2.26953V6.40007C14 6.96012 14 7.24015 14.109 7.45406C14.2049 7.64222 14.3578 7.7952 14.546 7.89108C14.7599 8.00007 15.0399 8.00007 15.6 8.00007H19.7305M14 17H8M16 13H8M20 9.98822V17.2C20 18.8802 20 19.7202 19.673 20.362C19.3854 20.9265 18.9265 21.3854 18.362 21.673C17.7202 22 16.8802 22 15.2 22H8.8C7.11984 22 6.27976 22 5.63803 21.673C5.07354 21.3854 4.6146 20.9265 4.32698 20.362C4 19.7202 4 18.8802 4 17.2V6.8C4 5.11984 4 4.27976 4.32698 3.63803C4.6146 3.07354 5.07354 2.6146 5.63803 2.32698C6.27976 2 7.11984 2 8.8 2H12.0118C12.7455 2 13.1124 2 13.4577 2.08289C13.7638 2.15638 14.0564 2.27759 14.3249 2.44208C14.6276 2.6276 14.887 2.88703 15.4059 3.40589L18.5941 6.59411C19.113 7.11297 19.3724 7.3724 19.5579 7.67515C19.7224 7.94356 19.8436 8.2362 19.9171 8.5423C20 8.88757 20 9.25445 20 9.98822Z";
        return "M14 2.26946V6.4C14 6.96005 14 7.24008 14.109 7.45399C14.2049 7.64215 14.3578 7.79513 14.546 7.89101C14.7599 8 15.0399 8 15.6 8H19.7305M20 9.98822V17.2C20 18.8802 20 19.7202 19.673 20.362C19.3854 20.9265 18.9265 21.3854 18.362 21.673C17.7202 22 16.8802 22 15.2 22H8.8C7.11984 22 6.27976 22 5.63803 21.673C5.07354 21.3854 4.6146 20.9265 4.32698 20.362C4 19.7202 4 18.8802 4 17.2V6.8C4 5.11984 4 4.27976 4.32698 3.63803C4.6146 3.07354 5.07354 2.6146 5.63803 2.32698C6.27976 2 7.11984 2 8.8 2H12.0118C12.7455 2 13.1124 2 13.4577 2.08289C13.7638 2.15638 14.0564 2.27759 14.3249 2.44208C14.6276 2.6276 14.887 2.88703 15.4059 3.40589L18.5941 6.59411C19.113 7.11297 19.3724 7.3724 19.5579 7.67515C19.7224 7.94356 19.8436 8.2362 19.9171 8.5423C20 8.88757 20 9.25445 20 9.98822Z";
    }

    function openFile(fileURL, fileType) {
        window.open(fileURL, '_blank');
    }

    function showFullImage(src) {
        const fullImage = document.getElementById('fullImage');
        const overlay = document.getElementById('overlay');

        fullImage.src = src;
        fullImage.style.display = 'block';
        overlay.style.display = 'block';

        requestAnimationFrame(() => {
            fullImage.style.opacity = 1;
            overlay.style.opacity = 1;
        });

        document.addEventListener('keydown', handleKeyDown);
    }

    function closeFullImage() {
        const fullImage = document.getElementById('fullImage');
        const overlay = document.getElementById('overlay');

        fullImage.style.opacity = 0;
        overlay.style.opacity = 0;

        setTimeout(() => {
            fullImage.style.display = 'none';
            overlay.style.display = 'none';
        }, 500);
    }

    function handleKeyDown(event) {
        if (event.key === 'Escape') {
            closeFullImage();
        }
    }

    const textarea = document.getElementById('exampleFormControlTextarea1');
    if (textarea) {
        textarea.addEventListener('paste', handlePaste);
    }

    // Автоматически нажимаем кнопку "Сохранить" после выбора файла
    function handleFileSelect(event) {
        console.log('handleFileSelect called');
        console.log('Selected file:', event.target.files[0]);

        // Используем setTimeout для отсрочки нажатия кнопки "Сохранить"
        setTimeout(function () {
            var submitButton = document.querySelector('button[type="submit"]');
            if (submitButton) {
                console.log('Submitting form...');
                submitButton.click();
            } else {
                console.error('Submit button not found');
            }
        }, 100); // Задержка в 100 мс для уверенности, что файл выбран
    }
});
