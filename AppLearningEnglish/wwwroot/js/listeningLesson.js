var listeningDataTable;


$(document).ready(function () {

    loadListeningDataTable();


    // FILTER

    $("#btnFilter").click(function () {

        listeningDataTable.ajax.reload();

    });


    // ENTER SEARCH

    $("#searchInput").keypress(function (event) {

        if (event.which === 13) {

            listeningDataTable.ajax.reload();

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

        $("#lessonFilter")
            .val(lessonId);

        listeningDataTable
            .ajax
            .reload();

    }

});


function loadListeningDataTable() {

    listeningDataTable =
        $("#listeningTable").DataTable({

            ajax: {

                url:
                    "/Admin/ListeningLesson/GetAll",

                type:
                    "GET",

                data:
                    function (data) {

                        data.search =
                            $("#searchInput").val();


                        var lessonId =
                            $("#lessonFilter").val();


                        if (lessonId !== "") {

                            data.lessonId =
                                parseInt(
                                    lessonId
                                );

                        }

                    }

            },


            columns: [

                // =================================
                // LESSON
                // =================================

                {
                    data:
                        "lessonTitle",

                    width:
                        "20%",

                    render:
                        function (
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

                                        ${data}

                                    </div>

                                    <small class="text-muted">

                                        Lesson ID:
                                        ${row.lessonId}

                                    </small>

                                </div>

                            `;

                        }

                },


                // =================================
                // AUDIO
                // =================================

                {
                    data:
                        "audioUrl",

                    width:
                        "30%",

                    orderable:
                        false,

                    render:
                        function (
                            data
                        ) {

                            if (!data) {

                                return `

                                    <span class="text-muted">

                                        Chưa có Audio

                                    </span>

                                `;

                            }


                            return `

                                <audio controls
                                       preload="none"
                                       style="width:100%;">

                                    <source
                                        src="${data}" />

                                    Browser không hỗ trợ audio.

                                </audio>

                            `;

                        }

                },


                // =================================
                // TRANSCRIPT
                // =================================

                {
                    data:
                        "transcript",

                    width:
                        "30%",

                    render:
                        function (
                            data
                        ) {

                            if (!data) {

                                return `

                                    <span class="text-muted">

                                        Chưa có Transcript

                                    </span>

                                `;

                            }


                            var text =
                                data;


                            if (text.length > 120) {

                                text =
                                    text.substring(
                                        0,
                                        120
                                    )
                                    + "...";

                            }


                            return `

                                <div
                                    class="text-muted"
                                    style="white-space:pre-line;">

                                    ${text}

                                </div>

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
                        function (
                            data
                        ) {

                            return `

                                <div class="d-flex
                                            justify-content-end
                                            gap-1">


                                    <a href="/Admin/ListeningLesson/Details/${data}"
                                       class="btn btn-sm btn-info text-white"
                                       title="Details">

                                        <i class="bi bi-eye"></i>

                                    </a>


                                    <a href="/Admin/ListeningLesson/Upsert/${data}"
                                       class="btn btn-sm btn-primary"
                                       title="Edit">

                                        <i class="bi bi-pencil-square"></i>

                                    </a>


                                    <button type="button"
                                            onclick="Delete('/Admin/ListeningLesson/Delete/${data}')"
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
                    "Không có Listening nào",

                search:
                    "Tìm kiếm:",

                lengthMenu:
                    "Hiển thị _MENU_ dòng",

                info:
                    "Hiển thị _START_ đến _END_ trong tổng số _TOTAL_ Listening",

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
            "Listening sẽ bị xóa và không thể hoàn tác!",

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


                            listeningDataTable
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
                            "Không thể xóa Listening.",
                            "error"
                        );

                    }

            });

        }

    });

}