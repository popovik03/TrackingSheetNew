<script>

    function deleteFile(fileName, incidentId) {
                if (confirm('�� �������, ��� ������ ������� ���� ����?')) {
        fetch(`/Incidents/DeleteFile?fileName=${encodeURIComponent(fileName)}&incidentId=${incidentId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        })

            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('���� ������� ������.');
                    location.reload(); // ������ ������������ �������� ����� �������� �����
                } else {
                    alert('������ ��� �������� �����: ' + (data.message || '����������� ������.'));
                }
            })
            .catch(error => {
                console.error('������:', error);
                alert('������ ��� �������� �����.');
            });
                }
            }

    function handleFileSelect(event) {
        console.log('handleFileSelect called');
    console.log('Selected file:', event.target.files[0]);

    // ���������� setTimeout ��� �������� ������� ������ "���������"
    setTimeout(function () {
                    var submitButton = document.querySelector('button[type="submit"]');
    if (submitButton) {
        console.log('Submitting form...');
    submitButton.click();
                    } else {
        console.error('Submit button not found');
                    }
                }, 100); // �������� � 100 �� ��� �����������, ��� ���� ������
            }

    document.addEventListener('DOMContentLoaded', function () {
                var modal = document.getElementById("incidentUpdateModal");
    var btn = document.getElementById("openModalButton");
    var span = document.getElementsByClassName("close-modal")[0];

    var updateForm = document.getElementById("incidentUpdateForm");
    var updateDate = document.getElementById("updateDate");
    var updateReporter = document.getElementById("updateReporter");
    var updateRun = document.getElementById("updateRun");
    var updateSolution = document.getElementById("updateSolution");

    btn.onclick = function (event) {
        event.preventDefault();
    updateDate.value = new Date().toISOString().slice(0, 16);
    updateReporter.value = "@reporter";
    updateRun.value = "@Model.Run";
    updateSolution.value = "";
    modal.style.display = "block";
                }

    span.onclick = function () {
        modal.style.display = "none";
                }

    window.onclick = function (event) {
                    if (event.target == modal) {
        modal.style.display = "none";
                    }
                }

    updateForm.addEventListener('submit', function (event) {
        event.preventDefault();

    var formData = {
        ID: "@Model.ID",
    Date: updateDate.value,
    UpdateReporter: updateReporter.value,
    Run: updateRun.value,
    UpdateSolution: updateSolution.value
                    };

    fetch('@Url.Action("AddUpdate", "Incidents")', {
        method: 'POST',
    headers: {
        'Content-Type': 'application/json',
    'RequestVerificationToken': getAntiForgeryToken()
                        },
    body: JSON.stringify(formData)
                    })
                        .then(response => response.json())
                        .then(data => {
                            if (data.success) {
        alert('�������� ������� ��������.');
    modal.style.display = "none";
    location.reload();
                            } else {
        alert('������ ��� ���������� ���������: ' + data.message);
                            }
                        })
                        .catch(error => {
        console.error('������:', error);
    alert('��������� ������ ��� ���������� ���������.');
                        });
                });

    function getAntiForgeryToken() {
                    var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenElement ? tokenElement.value : '';
                }
            });

</script>