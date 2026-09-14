var listeningQuestionDataTable;


$(document).ready(function () {

    loadListeningQuestionDataTable();


    // FILTER

    $("#btnFilter").click(function () {

        listeningQuestionDataTable
            .ajax
            .reload();

    });


    // ENTER SEARCH

    $("#searchInput").keypress(function (event) {

        if (event.which === 13) {

            listeningQuestionDataTable
                .ajax
                .reload();

        }

    });


    // Lấy listeningLessonId từ URL

    var urlParams =
        new URLSearchParams(
            window.location.search
        );

    var listeningLessonId =
        urlParams.get(
            "listeningLessonId"
        );


    if (listeningLessonId) {

        $("#listeningLessonFilter")
            .val(listeningLessonId);

        listeningQuestionDataTable
            .ajax
            .reload();

    }

});


function loadListeningQuestionDataTable() {

    listeningQuestionDataTable =
        $("#listeningQuestionTable")
            .DataTable({

                ajax: {

                    url:
                        "/Admin/ListeningQuestion/GetAll",

                    type:
                        "GET",

                    data:
                        function (data) {

                            data.search =
                                $("#searchInput")
                                    .val();


                            var listeningLessonId =
                                $("#listeningLessonFilter")
                                    .val();


                            if (
                                listeningLessonId !== ""
                            ) {

                                data.listeningLessonId =
                                    parseInt(
                                        listeningLessonId
                                    );

                            }

                        }

                },


                columns: [

                    // =================================
                    // LISTENING
                    // =================================

                    {
                        data:
                            "listeningLessonTitle",

                        width:
                            "25%",

                        render:
                            function (
                                data,
                                type,
                                row
                            ) {

                                return `

                                    <div>

                                        <div class="fw-bold">

                                            <i class="bi bi-headphones
                                                      text-primary
                                                      me-1"></i>

                                            ${data}

                                        </div>

                                        <small class="text-muted">

                                            Listening ID:
                                            ${row.listeningLessonId}

                                        </small>

                                    </div>

                                `;

                            }

                    },


                    // =================================
                    // QUESTION
                    // =================================

                    {
                        data:
                            "question",

                        width:
                            "30%",

                        render:
                            function (data) {

                                if (!data) {

                                    return `
                                        <span class="text-muted">
                                            N/A
                                        </span>
                                    `;

                                }


                                return `

                                    <div class="fw-semibold">

                                        ${data}

                                    </div>

                                `;

                            }

                    },


                    // =================================
                    // ANSWER
                    // =================================

                    {
                        data:
                            "answer",

                        width:
                            "25%",

                        render:
                            function (data) {

                                if (!data) {

                                    return `
                                        <span class="text-muted">
                                            N/A
                                        </span>
                                    `;

                                }


                                return `

                                    <span class="badge
                                                 bg-success-subtle
                                                 text-success">

                                        ${data}

                                    </span>

                                `;

                            }

                    },


                    // =================================
                    // ACTION
                    // =================================

                    {
                        data:
                            "id",

                        width:
                            "20%",

                        orderable:
                            false,

                        className:
                            "text-end",

                        render:
                            function (data) {

                                return `

                                    <div class="d-flex
                                                justify-content-end
                                                gap-1">


                                        <a href="/Admin/ListeningQuestion/Details/${data}"
                                           class="btn btn-sm btn-info text-white"
                                           title="Details">

                                            <i class="bi bi-eye"></i>

                                        </a>


                                        <a href="/Admin/ListeningQuestion/Upsert/${data}"
                                           class="btn btn-sm btn-primary"
                                           title="Edit">

                                            <i class="bi bi-pencil-square"></i>

                                        </a>


                                        <button type="button"
                                                onclick="Delete('/Admin/ListeningQuestion/Delete/${data}')"
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
                        "Không có Listening Question nào",

                    search:
                        "Tìm kiếm:",

                    lengthMenu:
                        "Hiển thị _MENU_ dòng",

                    info:
                        "Hiển thị _START_ đến _END_ trong tổng số _TOTAL_ câu hỏi",

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


                responsive:
                    true,

                pageLength:
                    10

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
            "Listening Question sẽ bị xóa!",

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

                url:
                    url,

                type:
                    "DELETE",

                success:
                    function (data) {

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


                            listeningQuestionDataTable
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

                error:
                    function () {

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