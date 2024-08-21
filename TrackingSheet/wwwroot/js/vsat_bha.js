// Добавляем номера всатов в комбобокс

var select = document.getElementById('ipPart');
for (var i = 1; i <= 200; i++) {
    var option = document.createElement('option');
    option.value = i;
    option.text = i;
    select.appendChild(option);
}

// Добавляем обработчик события для получения значения выбранного элемента
select.addEventListener('change', function() {
    const selectedValue = select.value;
    console.log('Selected value:', selectedValue);
    // Устанавливаем в лейбл всата
    const label = document.getElementById('vsat_label');
    label.textContent = "Информация по VSAT " + selectedValue;
});
