var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/semester/getall' },
        "columns": [
            { data: 'name', "width": "25%" },
            { data: 'startDate', "width": "25%" },
            { data: 'endDate', "width": "25%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                     <a href="/admin/semester/edit?id=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Edit</a>
                     <button type="button" class="btn btn-danger mx-2" onclick="showDeleteConfirmation(${data})"> <i class="bi bi-trash-fill"></i> Delete</button>
                    </div>`
                },
                "width": "25%"
            }
        ]
    });
}

function showDeleteConfirmation(id) {
    $('#deleteConfirmationModal').modal('show');
    $('#deleteConfirmationButton').off('click').on('click', function () {
        deleteSemester(id);
    });
    $('#hideBtn').off('click').on('click', function () {
        hideModal();
    })
}
function hideModal() {
    $('#deleteConfirmationModal').modal('hide');
}

function deleteSemester(id) {
    $.ajax({
        url: `/admin/semester/DeleteConfirmed/${id}`,
        type: 'DELETE',
        success: function (response) {        
            console.log('Semester deleted successfully.');
            location.reload();
        },
        error: function (xhr, status, error) {
            console.error('Error deleting semester:', error);
        }
    });
}



