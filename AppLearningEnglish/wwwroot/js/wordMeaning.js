var wordMeaningDataTable;

$(document).ready(function () {

    loadWordMeaningDataTable();

    $("#btnFilter").click(function () {

        wordMeaningDataTable.ajax.reload();

    });

    $("#searchInput").keypress(function (event) {

        if (event.which === 13) {

            wordMeaningDataTable.ajax.reload();

        }

    });

    // Đọc wordId từ URL
    var urlParams =
        new URLSearchParams(
            window.location.search
        );

    var wordId =
        urlParams.get("wordId");

    if (wordId) {

        $("#wordFilter").val(wordId);

        wordMeaningDataTable.ajax.reload();

    }

});


function loadWordMeaningDataTable() {

    wordMeaningDataTable =
        $("#wordMeaningTable").DataTable({

            ajax: {

                url: "/Admin/WordMeaning/GetAll",

                type: "GET",

                data: function (data) {

                    data.search =
                        $("#searchInput").val();

                    var wordId =
                        $("#wordFilter").val();

                    if (wordId !== "") {

                        data.wordId =
                            parseInt(wordId);

                    }

                    data.language =
                        $("#languageFilter").val();

                }

            },


            columns: [

                // WORD

                {
                    data: "word",

                    width: "25%",

                    render: function (data, type, row) {

                        if (!data) {

                            return `
                                <span class="text-muted">
                                    N/A
                                </span>
                            `;

                        }

                        return `

                            <div>

                                <div class="fw-bold fs-5">

                                    ${data.wordText}

                                </div>

                                <small class="text-muted">

                                    Word ID:
                                    ${row.wordId}

                                </small>

                            </div>

                        `;

                    }

                },


                // LANGUAGE

                {
                    data: "language",

                    width: "15%",

                    render: function (data) {

                        if (!data) {

                            return `
                                <span class="badge bg-secondary">
                                    N/A
                                </span>
                            `;

                        }

                        return `

                            <span class="badge bg-primary">

                                ${data}

                            </span>

                        `;

                    }

                },


                // MEANING

                {
                    data: "meaning",

                    width: "40%",

                    render: function (data) {

                        if (!data) {

                            return `
                                <span class="text-muted">
                                    N/A
                                </span>
                            `;

                        }

                        return `

                            <span>

                                ${data}

                            </span>

                        `;

                    }

                },


                // ACTION

                {
                    data: "id",

                    width: "20%",

                    className: "text-end",

                    orderable: false,

                    render: function (data) {

                        return `

                            <div class="d-flex
                                        justify-content-end
                                        gap-1">

                                <a href="/Admin/WordMeaning/Details/${data}"
                                   class="btn btn-sm btn-info text-white"
                                   title="Details">

                                    <i class="bi bi-eye"></i>

                                </a>


                                <a href="/Admin/WordMeaning/Upsert/${data}"
                                   class="btn btn-sm btn-primary"
                                   title="Edit">

                                    <i class="bi bi-pencil-square"></i>

                                </a>


                                <button type="button"
                                        onclick="Delete('/Admin/WordMeaning/Delete/${data}')"
                                        class="btn btn-sm btn-danger"
                                        title="Delete">

                                    <i class="bi bi-trash"></i>

                                </button>

                            </div>

                        `;

                    }

                }

            ],


            language: {

                emptyTable:
                    "Không có Meaning nào",

                search:
                    "Tìm kiếm:",

                lengthMenu:
                    "Hiển thị _MENU_ dòng",

                info:
                    "Hiển thị _START_ đến _END_ trong tổng số _TOTAL_ Meaning",

                infoEmpty:
                    "Không có dữ liệu",

                paginate: {

                    first: "Đầu",

                    last: "Cuối",

                    next: "Sau",

                    previous: "Trước"

                }

            },


            responsive: true,

            pageLength: 10,

            order: [

                [0, "asc"]

            ]

        });

}


// ======================================
// DELETE
// ======================================

function Delete(url) {

    Swal.fire({

        title: "Bạn có chắc không?",

        text:
            "Meaning sẽ bị xóa và không thể hoàn tác!",

        icon: "warning",

        showCancelButton: true,

        confirmButtonText: "Xóa",

        cancelButtonText: "Hủy",

        confirmButtonColor: "#dc3545"

    }).then(function (result) {

        if (result.isConfirmed) {

            $.ajax({

                url: url,

                type: "DELETE",

                success: function (data) {

                    if (data.success) {

                        Swal.fire({

                            title: "Đã xóa!",

                            text: data.message,

                            icon: "success",

                            timer: 1500,

                            showConfirmButton: false

                        });

                        wordMeaningDataTable
                            .ajax
                            .reload();

                    }
                    else {

                        Swal.fire(
                            "Lỗi",
                            data.message,
                            "error"
                        );

                    }

                },

                error: function () {

                    Swal.fire(
                        "Lỗi",
                        "Không thể xóa Meaning.",
                        "error"
                    );

                }

            });

        }

    });

}