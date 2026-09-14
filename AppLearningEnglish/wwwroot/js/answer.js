var answerDataTable;


$(document).ready(function () {

    loadAnswerDataTable();


    // FILTER

    $("#btnFilter").click(function () {

        answerDataTable.ajax.reload();

    });


    // ENTER SEARCH

    $("#searchInput").keypress(function (event) {

        if (event.which === 13) {

            answerDataTable.ajax.reload();

        }

    });


    // Lấy questionId từ URL

    var urlParams =
        new URLSearchParams(
            window.location.search
        );

    var questionId =
        urlParams.get("questionId");


    if (questionId) {

        $("#questionFilter")
            .val(questionId);

        answerDataTable
            .ajax
            .reload();

    }

});


function loadAnswerDataTable() {

    answerDataTable =
        $("#answerTable").DataTable({

            ajax: {

                url:
                    "/Admin/Answer/GetAll",

                type: "GET",

                data: function (data) {

                    data.search =
                        $("#searchInput").val();


                    var questionId =
                        $("#questionFilter").val();


                    if (questionId !== "") {

                        data.questionId =
                            parseInt(questionId);

                    }

                }

            },


            columns: [

                // QUESTION

                {
                    data: "questionText",

                    width: "35%",

                    render: function (data, type, row) {

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

                                <div class="fw-semibold">

                                    <i class="bi bi-question-circle
                                              text-primary
                                              me-1"></i>

                                    ${text}

                                </div>

                                <small class="text-muted">

                                    Question ID:
                                    ${row.questionId}

                                </small>

                            </div>

                        `;

                    }

                },


                // ANSWER

                {
                    data: "answerText",

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

                            <span>

                                ${data}

                            </span>

                        `;

                    }

                },


                // CORRECT

                {
                    data: "isCorrect",

                    width: "15%",

                    className: "text-center",

                    render: function (data) {

                        if (data === true) {

                            return `

                                <span class="badge bg-success">

                                    <i class="bi bi-check-circle me-1"></i>

                                    Correct

                                </span>

                            `;

                        }


                        return `

                            <span class="badge bg-danger">

                                <i class="bi bi-x-circle me-1"></i>

                                Incorrect

                            </span>

                        `;

                    }

                },


                // ACTION

                {
                    data: "id",

                    width: "20%",

                    orderable: false,

                    className: "text-end",

                    render: function (data, type, row) {

                        var correctButton = "";


                        if (row.isCorrect === true) {

                            correctButton = `

                                <button type="button"
                                        onclick="SetIncorrect(${data})"
                                        class="btn btn-sm btn-warning"
                                        title="Set Incorrect">

                                    <i class="bi bi-x-circle"></i>

                                </button>

                            `;

                        }
                        else {

                            correctButton = `

                                <button type="button"
                                        onclick="SetCorrect(${data})"
                                        class="btn btn-sm btn-success"
                                        title="Set Correct">

                                    <i class="bi bi-check-circle"></i>

                                </button>

                            `;

                        }


                        return `

                            <div class="d-flex
                                        justify-content-end
                                        gap-1">


                                ${correctButton}


                                <a href="/Admin/Answer/Details/${data}"
                                   class="btn btn-sm btn-info text-white"
                                   title="Details">

                                    <i class="bi bi-eye"></i>

                                </a>


                                <a href="/Admin/Answer/Upsert/${data}"
                                   class="btn btn-sm btn-primary"
                                   title="Edit">

                                    <i class="bi bi-pencil-square"></i>

                                </a>


                                <button type="button"
                                        onclick="Delete('/Admin/Answer/Delete/${data}')"
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
                    "Không có Answer nào",

                search:
                    "Tìm kiếm:",

                lengthMenu:
                    "Hiển thị _MENU_ dòng",

                info:
                    "Hiển thị _START_ đến _END_ trong tổng số _TOTAL_ Answer",

                infoEmpty:
                    "Không có dữ liệu",

                paginate: {

                    first:
                        "Đầu",

                    last:
                        "Cuối",

                    next:
                        "Sau",

                    previous:
                        "Trước"

                }

            },


            responsive: true,

            pageLength: 10

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
            "Answer sẽ bị xóa và không thể hoàn tác!",

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


                        answerDataTable
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
                        "Không thể xóa Answer.",
                        "error"
                    );

                }

            });

        }

    });

}


// ==========================================
// SET CORRECT
// ==========================================

function SetCorrect(id) {

    $.ajax({

        url:
            "/Admin/Answer/SetCorrect?id="
            + id,

        type:
            "POST",

        success: function (data) {

            if (data.success) {

                Swal.fire({

                    title:
                        "Thành công!",

                    text:
                        data.message,

                    icon:
                        "success",

                    timer:
                        1200,

                    showConfirmButton:
                        false

                });


                answerDataTable
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
                "Không thể cập nhật Answer.",
                "error"
            );

        }

    });

}


// ==========================================
// SET INCORRECT
// ==========================================

function SetIncorrect(id) {

    $.ajax({

        url:
            "/Admin/Answer/SetIncorrect?id="
            + id,

        type:
            "POST",

        success: function (data) {

            if (data.success) {

                Swal.fire({

                    title:
                        "Thành công!",

                    text:
                        data.message,

                    icon:
                        "success",

                    timer:
                        1200,

                    showConfirmButton:
                        false

                });


                answerDataTable
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
                "Không thể cập nhật Answer.",
                "error"
            );

        }

    });

}