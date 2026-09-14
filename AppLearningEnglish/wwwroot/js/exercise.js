var exerciseDataTable;

$(document).ready(function () {

    loadExerciseDataTable();


    $("#btnFilter").click(function () {

        exerciseDataTable.ajax.reload();

    });


    $("#searchInput").keypress(function (event) {

        if (event.which === 13) {

            exerciseDataTable.ajax.reload();

        }

    });


    // Lấy lessonId từ URL

    var urlParams =
        new URLSearchParams(
            window.location.search
        );

    var lessonId =
        urlParams.get("lessonId");


    if (lessonId) {

        $("#lessonFilter").val(lessonId);

        exerciseDataTable.ajax.reload();

    }

});


function loadExerciseDataTable() {

    exerciseDataTable =
        $("#exerciseTable").DataTable({

            ajax: {

                url:
                    "/Admin/Exercise/GetAll",

                type: "GET",

                data: function (data) {

                    data.search =
                        $("#searchInput").val();


                    var lessonId =
                        $("#lessonFilter").val();


                    if (lessonId !== "") {

                        data.lessonId =
                            parseInt(lessonId);

                    }


                    data.type =
                        $("#typeFilter").val();

                }

            },


            columns: [

                // ORDER

                {
                    data: "exerciseOrder",

                    width: "10%",

                    className: "text-center",

                    render: function (data) {

                        return `

                            <span class="badge bg-dark fs-6">

                                ${data}

                            </span>

                        `;

                    }

                },


                // LESSON

                {
                    data: "lesson",

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

                                    ${data.title}

                                </div>

                                <small class="text-muted">

                                    Lesson ID:
                                    ${row.lessonId}

                                </small>

                            </div>

                        `;

                    }

                },


                // TYPE

                {
                    data: "type",

                    width: "15%",

                    render: function (data) {

                        if (!data) {

                            return `
                                <span class="badge bg-secondary">
                                    N/A
                                </span>
                            `;

                        }

                        var text = data;


                        switch (data) {

                            case "MultipleChoice":

                                text =
                                    "Multiple Choice";

                                break;


                            case "FillBlank":

                                text =
                                    "Fill in the Blank";

                                break;


                            case "TrueFalse":

                                text =
                                    "True / False";

                                break;


                            case "Matching":

                                text =
                                    "Matching";

                                break;


                            case "Listening":

                                text =
                                    "Listening";

                                break;


                            case "Reading":

                                text =
                                    "Reading";

                                break;

                        }


                        return `

                            <span class="badge bg-primary">

                                ${text}

                            </span>

                        `;

                    }

                },


                // QUESTION

                {
                    data: "question",

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


                        if (text.length > 100) {

                            text =
                                text.substring(0, 100)
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


                                <a href="/Admin/Exercise/Details/${data}"
                                   class="btn btn-sm btn-info text-white"
                                   title="Details">

                                    <i class="bi bi-eye"></i>

                                </a>


                                <a href="/Admin/Exercise/Upsert/${data}"
                                   class="btn btn-sm btn-primary"
                                   title="Edit">

                                    <i class="bi bi-pencil-square"></i>

                                </a>


                                <button type="button"
                                        onclick="Delete('/Admin/Exercise/Delete/${data}')"
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
                    "Không có Exercise nào",

                search:
                    "Tìm kiếm:",

                lengthMenu:
                    "Hiển thị _MENU_ dòng",

                info:
                    "Hiển thị _START_ đến _END_ trong tổng số _TOTAL_ Exercise",

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

                [1, "asc"],

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
            "Exercise sẽ bị xóa và không thể hoàn tác!",

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


                        exerciseDataTable
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
                        "Không thể xóa Exercise.",
                        "error"
                    );

                }

            });

        }

    });

}