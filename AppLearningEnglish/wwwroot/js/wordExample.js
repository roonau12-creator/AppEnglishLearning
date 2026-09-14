var wordExampleDataTable;

$(document).ready(function () {

    loadWordExampleDataTable();


    $("#btnFilter").click(function () {

        wordExampleDataTable.ajax.reload();

    });


    $("#searchInput").keypress(function (event) {

        if (event.which === 13) {

            wordExampleDataTable.ajax.reload();

        }

    });


    // Lấy wordId từ URL

    var urlParams =
        new URLSearchParams(
            window.location.search
        );

    var wordId =
        urlParams.get("wordId");


    if (wordId) {

        $("#wordFilter").val(wordId);

        wordExampleDataTable.ajax.reload();

    }

});


function loadWordExampleDataTable() {

    wordExampleDataTable =
        $("#wordExampleTable").DataTable({

            ajax: {

                url:
                    "/Admin/WordExample/GetAll",

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

                }

            },


            columns: [

                // WORD

                {
                    data: "word",

                    width: "18%",

                    render: function (
                        data,
                        type,
                        row
                    ) {

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


                // ENGLISH SENTENCE

                {
                    data: "englishSentence",

                    width: "32%",

                    render: function (data) {

                        if (!data) {

                            return `
                                <span class="text-muted">
                                    N/A
                                </span>
                            `;

                        }

                        return `

                            <div>

                                <i class="bi bi-quote
                                          text-primary
                                          me-1"></i>

                                ${data}

                            </div>

                        `;

                    }

                },


                // VIETNAMESE

                {
                    data: "vietnameseMeaning",

                    width: "30%",

                    render: function (data) {

                        if (!data) {

                            return `
                                <span class="text-muted">
                                    N/A
                                </span>
                            `;

                        }

                        return `

                            <div class="text-muted">

                                ${data}

                            </div>

                        `;

                    }

                },


                // ACTION

                {
                    data: "id",

                    width: "20%",

                    orderable: false,

                    className: "text-end",

                    render: function (data) {

                        return `

                            <div class="d-flex
                                        justify-content-end
                                        gap-1">


                                <a href="/Admin/WordExample/Details/${data}"
                                   class="btn btn-sm btn-info text-white"
                                   title="Details">

                                    <i class="bi bi-eye"></i>

                                </a>


                                <a href="/Admin/WordExample/Upsert/${data}"
                                   class="btn btn-sm btn-primary"
                                   title="Edit">

                                    <i class="bi bi-pencil-square"></i>

                                </a>


                                <button type="button"
                                        onclick="Delete('/Admin/WordExample/Delete/${data}')"
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
                    "Không có câu ví dụ nào",

                search:
                    "Tìm kiếm:",

                lengthMenu:
                    "Hiển thị _MENU_ dòng",

                info:
                    "Hiển thị _START_ đến _END_ trong tổng số _TOTAL_ câu",

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


// ==========================================
// DELETE
// ==========================================

function Delete(url) {

    Swal.fire({

        title:
            "Bạn có chắc không?",

        text:
            "Câu ví dụ sẽ bị xóa và không thể hoàn tác!",

        icon:
            "warning",

        showCancelButton:
            true,

        confirmButtonText:
            "Xóa",

        cancelButtonText:
            "Hủy",

        confirmButtonColor:
            "#dc3545"

    }).then(function (result) {

        if (result.isConfirmed) {

            $.ajax({

                url: url,

                type: "DELETE",

                success: function (data) {

                    if (data.success) {

                        Swal.fire({

                            title:
                                "Đã xóa!",

                            text:
                                data.message,

                            icon:
                                "success",

                            timer:
                                1500,

                            showConfirmButton:
                                false

                        });


                        wordExampleDataTable
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
                        "Không thể xóa câu ví dụ.",
                        "error"
                    );

                }

            });

        }

    });

}