var questionDataTable;

$(document).ready(function () {

    loadQuestionDataTable();


    $("#btnFilter").click(function () {

        questionDataTable.ajax.reload();

    });


    $("#searchInput").keypress(function (event) {

        if (event.which === 13) {

            questionDataTable.ajax.reload();

        }

    });


    // Lấy exerciseId từ URL

    var urlParams =
        new URLSearchParams(
            window.location.search
        );

    var exerciseId =
        urlParams.get("exerciseId");


    if (exerciseId) {

        $("#exerciseFilter")
            .val(exerciseId);

        questionDataTable
            .ajax
            .reload();

    }

});


function loadQuestionDataTable() {

    questionDataTable =
        $("#questionTable").DataTable({

            ajax: {

                url:
                    "/Admin/Question/GetAll",

                type: "GET",

                data: function (data) {

                    data.search =
                        $("#searchInput").val();


                    var exerciseId =
                        $("#exerciseFilter").val();


                    if (exerciseId !== "") {

                        data.exerciseId =
                            parseInt(exerciseId);

                    }

                }

            },


            columns: [

                // EXERCISE

                {
                    data: "exercise",

                    width: "20%",

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

                                <div class="fw-bold">

                                    ${data.type}

                                </div>

                                <small class="text-muted">

                                    Exercise ID:
                                    ${row.exerciseId}

                                </small>

                            </div>

                        `;

                    }

                },


                // QUESTION

                {
                    data: "questionText",

                    width: "35%",

                    render: function (data) {

                        if (!data) {

                            return `
                                <span class="text-muted">
                                    N/A
                                </span>
                            `;

                        }


                        var text = data;


                        if (text.length > 120) {

                            text =
                                text.substring(0, 120)
                                + "...";

                        }


                        return `

                            <div>

                                <i class="bi bi-question-circle
                                          text-primary
                                          me-1"></i>

                                ${text}

                            </div>

                        `;

                    }

                },


                // EXPLANATION

                {
                    data: "explanation",

                    width: "30%",

                    render: function (data) {

                        if (!data) {

                            return `

                                <span class="text-muted">

                                    Chưa có explanation

                                </span>

                            `;

                        }


                        var text = data;


                        if (text.length > 100) {

                            text =
                                text.substring(0, 100)
                                + "...";

                        }


                        return `

                            <div class="text-muted">

                                ${text}

                            </div>

                        `;

                    }

                },


                // ACTION

                {
                    data: "id",

                    width: "15%",

                    orderable: false,

                    className: "text-end",

                    render: function (data) {

                        return `

                            <div class="d-flex
                                        justify-content-end
                                        gap-1">


                                <a href="/Admin/Question/Details/${data}"
                                   class="btn btn-sm btn-info text-white"
                                   title="Details">

                                    <i class="bi bi-eye"></i>

                                </a>


                                <a href="/Admin/Question/Upsert/${data}"
                                   class="btn btn-sm btn-primary"
                                   title="Edit">

                                    <i class="bi bi-pencil-square"></i>

                                </a>


                                <button type="button"
                                        onclick="Delete('/Admin/Question/Delete/${data}')"
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
                    "Không có Question nào",

                search:
                    "Tìm kiếm:",

                lengthMenu:
                    "Hiển thị _MENU_ dòng",

                info:
                    "Hiển thị _START_ đến _END_ trong tổng số _TOTAL_ Question",

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
            "Question sẽ bị xóa và không thể hoàn tác!",

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


                        questionDataTable
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
                        "Không thể xóa Question.",
                        "error"
                    );

                }

            });

        }

    });

}