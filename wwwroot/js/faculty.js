var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/faculty/getall' },
        "columns": [
            { data: 'name', "width": "25%" },
            { data: 'description', "width": "50%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                     <a href="/admin/faculty/edit?id=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Edit</a>
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
        deleteFaculty(id);
    });
    $('#hideBtn').off('click').on('click', function () {
        hideModal();
    })
}
function hideModal() {
    $('#deleteConfirmationModal').modal('hide');
}

function deleteFaculty(id) {
    $.ajax({
        url: `/admin/faculty/DeletePOST/${id}`,
        type: 'DELETE',
        success: function (response) {
            console.log('faculty deleted successfully.');
            location.reload();
        },
        error: function (xhr, status, error) {
            console.error('Error deleting faculty:', error);
        }
    });
}


