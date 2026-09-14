var lessonDataTable;

$(document).ready(function () {

    loadLessonDataTable();

    $("#btnFilter").click(function () {

        lessonDataTable.ajax.reload();

    });

    $("#btnReset").click(function () {

        $("#searchInput").val("");

        $("#courseFilter").val("");

        $("#topicFilter").val("");

        $("#statusFilter").val("");

        lessonDataTable.ajax.reload();

    });

    $("#searchInput").keypress(function (event) {

        if (event.which === 13) {

            lessonDataTable.ajax.reload();

        }

    });

});


function loadLessonDataTable() {

    lessonDataTable = $("#lessonTable").DataTable({

        ajax: {

            url: "/Admin/Lesson/GetAll",

            type: "GET",

            data: function (data) {

                data.search =
                    $("#searchInput").val();

                var courseId =
                    $("#courseFilter").val();

                if (courseId !== "") {

                    data.courseId =
                        parseInt(courseId);

                }

                var topicId =
                    $("#topicFilter").val();

                if (topicId !== "") {

                    data.topicId =
                        parseInt(topicId);

                }

                var status =
                    $("#statusFilter").val();

                if (status !== "") {

                    data.isPublished =
                        status === "true";

                }

            }

        },

        columns: [

            // ORDER

            {
                data: "lessonOrder",

                width: "8%",

                className: "text-center",

                render: function (data) {

                    return `
                        <span class="badge bg-primary">
                            ${data}
                        </span>
                    `;

                }

            },


            // TITLE

            {
                data: "title",

                width: "22%",

                render: function (data, type, row) {

                    return `
                        <div>

                            <div class="fw-semibold">
                                ${data}
                            </div>

                            <small class="text-muted">
                                ID: ${row.id}
                            </small>

                        </div>
                    `;

                }

            },


            // COURSE

            {
                data: "course",

                width: "15%",

                render: function (data) {

                    if (!data) {

                        return `
                            <span class="text-muted">
                                N/A
                            </span>
                        `;

                    }

                    return `
                        <span class="badge bg-info text-dark">
                            ${data.name}
                        </span>
                    `;

                }

            },


            // TOPIC

            {
                data: "topic",

                width: "15%",

                render: function (data) {

                    if (!data) {

                        return `
                            <span class="text-muted">
                                N/A
                            </span>
                        `;

                    }

                    return `
                        <span class="badge bg-secondary">
                            ${data.name}
                        </span>
                    `;

                }

            },


            // DURATION

            {
                data: "durationMinutes",

                width: "10%",

                render: function (data) {

                    return `
                        <span>
                            <i class="bi bi-clock me-1"></i>
                            ${data} phút
                        </span>
                    `;

                }

            },


            // STATUS

            {
                data: "isPublished",

                width: "12%",

                render: function (data) {

                    if (data) {

                        return `
                            <span class="badge bg-success">
                                <i class="bi bi-check-circle me-1"></i>
                                Published
                            </span>
                        `;

                    }

                    return `
                        <span class="badge bg-warning text-dark">
                            <i class="bi bi-clock me-1"></i>
                            Draft
                        </span>
                    `;

                }

            },


            // ACTION

            {
                data: "id",

                width: "25%",

                className: "text-end",

                orderable: false,

                render: function (data, type, row) {

                    var publishButton = "";

                    if (row.isPublished) {

                        publishButton = `

                            <button type="button"
                                    onclick="Unpublish(${data})"
                                    class="btn btn-sm btn-warning"
                                    title="Unpublish">

                                <i class="bi bi-eye-slash"></i>

                            </button>

                        `;

                    }
                    else {

                        publishButton = `

                            <button type="button"
                                    onclick="Publish(${data})"
                                    class="btn btn-sm btn-success"
                                    title="Publish">

                                <i class="bi bi-eye"></i>

                            </button>

                        `;

                    }

                    return `

                        <div class="d-flex
                                    justify-content-end
                                    gap-1">

                            <a href="/Admin/Lesson/Details/${data}"
                               class="btn btn-sm btn-info text-white"
                               title="Details">

                                <i class="bi bi-eye"></i>

                            </a>

                            <a href="/Admin/Lesson/Upsert/${data}"
                               class="btn btn-sm btn-primary"
                               title="Edit">

                                <i class="bi bi-pencil-square"></i>

                            </a>

                            ${publishButton}

                            <button type="button"
                                    onclick="Delete('/Admin/Lesson/Delete/${data}')"
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
                "Không có Lesson nào",

            search:
                "Tìm kiếm:",

            lengthMenu:
                "Hiển thị _MENU_ dòng",

            info:
                "Hiển thị _START_ đến _END_ trong tổng số _TOTAL_ Lesson",

            infoEmpty:
                "Không có Lesson",

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


// ==============================
// DELETE
// ==============================

function Delete(url) {

    Swal.fire({

        title: "Bạn có chắc không?",

        text: "Lesson sẽ bị xóa và không thể hoàn tác!",

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

                        lessonDataTable.ajax.reload();

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
                        "Không thể xóa Lesson.",
                        "error"
                    );

                }

            });

        }

    });

}


// ==============================
// PUBLISH
// ==============================

function Publish(id) {

    Swal.fire({

        title: "Publish Lesson?",

        text: "Lesson sẽ được hiển thị cho người học.",

        icon: "question",

        showCancelButton: true,

        confirmButtonText: "Publish",

        cancelButtonText: "Hủy"

    }).then(function (result) {

        if (result.isConfirmed) {

            $.ajax({

                url: "/Admin/Lesson/Publish/" + id,

                type: "POST",

                success: function (data) {

                    if (data.success) {

                        Swal.fire({

                            title: "Published!",

                            text: data.message,

                            icon: "success",

                            timer: 1200,

                            showConfirmButton: false

                        });

                        lessonDataTable.ajax.reload();

                    }

                },

                error: function () {

                    Swal.fire(
                        "Lỗi",
                        "Không thể publish Lesson.",
                        "error"
                    );

                }

            });

        }

    });

}


// ==============================
// UNPUBLISH
// ==============================

function Unpublish(id) {

    Swal.fire({

        title: "Unpublish Lesson?",

        text: "Lesson sẽ trở về trạng thái Draft.",

        icon: "warning",

        showCancelButton: true,

        confirmButtonText: "Unpublish",

        cancelButtonText: "Hủy"

    }).then(function (result) {

        if (result.isConfirmed) {

            $.ajax({

                url: "/Admin/Lesson/Unpublish/" + id,

                type: "POST",

                success: function (data) {

                    if (data.success) {

                        Swal.fire({

                            title: "Unpublished!",

                            text: data.message,

                            icon: "success",

                            timer: 1200,

                            showConfirmButton: false

                        });

                        lessonDataTable.ajax.reload();

                    }

                },

                error: function () {

                    Swal.fire(
                        "Lỗi",
                        "Không thể unpublish Lesson.",
                        "error"
                    );

                }

            });

        }

    });

}
