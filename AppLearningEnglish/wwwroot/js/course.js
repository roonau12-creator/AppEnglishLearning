var courseDataTable;

$(document).ready(function () {

    loadCourseDataTable();

    $("#btnFilter").click(function () {

        courseDataTable.ajax.reload();

    });

    $("#searchInput").keypress(function (event) {

        if (event.which === 13) {

            courseDataTable.ajax.reload();

        }

    });

});


function loadCourseDataTable() {

    courseDataTable = $("#courseTable").DataTable({

        ajax: {

            url: "/Admin/Course/GetAll",

            type: "GET",

            data: function (data) {

                data.search =
                    $("#searchInput").val();

                data.level =
                    $("#levelFilter").val();

                var status =
                    $("#statusFilter").val();

                if (status !== "") {

                    data.isPublished =
                        status === "true";

                }

            }

        },

        columns: [

            {
                data: "thumbnailUrl",

                width: "10%",

                render: function (data) {

                    if (!data) {

                        return `
                            <div class="bg-light
                                        rounded
                                        d-flex
                                        align-items-center
                                        justify-content-center"
                                 style="width:70px;height:50px">

                                <i class="bi bi-image
                                          text-muted"></i>

                            </div>
                        `;

                    }

                    return `
                        <img src="${data}"
                             class="rounded"
                             style="
                                width:70px;
                                height:50px;
                                object-fit:cover;
                             "
                             onerror="
                                this.src='/images/no-image.png';
                             " />
                    `;

                }

            },

            {
                data: "name",

                width: "25%",

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

            {
                data: "level",

                width: "10%",

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

            {
                data: "isPublished",

                width: "15%",

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

            {
                data: "createdAt",

                width: "15%",

                render: function (data) {

                    if (!data)
                        return "";

                    var date =
                        new Date(data);

                    return date.toLocaleDateString(
                        "vi-VN"
                    );

                }

            },

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
                                    class="btn btn-sm btn-warning">

                                <i class="bi bi-eye-slash"></i>

                            </button>
                        `;

                    }
                    else {

                        publishButton = `
                            <button type="button"
                                    onclick="Publish(${data})"
                                    class="btn btn-sm btn-success">

                                <i class="bi bi-eye"></i>

                            </button>
                        `;

                    }

                    return `

                        <div class="d-flex
                                    justify-content-end
                                    gap-1">

                            <a href="/Admin/Course/Details/${data}"
                               class="btn btn-sm btn-info text-white"
                               title="Details">

                                <i class="bi bi-eye"></i>

                            </a>

                            <a href="/Admin/Course/Upsert/${data}"
                               class="btn btn-sm btn-primary"
                               title="Edit">

                                <i class="bi bi-pencil-square"></i>

                            </a>

                            <a href="/Admin/Lesson?courseId=${data}"
                               class="btn btn-sm btn-secondary"
                               title="Lessons">

                                <i class="bi bi-book"></i>

                            </a>

                            ${publishButton}

                            <button type="button"
                                    onclick="Delete('/Admin/Course/Delete/${data}')"
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
                "Không có Course nào",

            search:
                "Tìm kiếm:",

            lengthMenu:
                "Hiển thị _MENU_ dòng",

            info:
                "Hiển thị _START_ đến _END_ trong tổng số _TOTAL_ Course",

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

            [4, "desc"]

        ]

    });

}

function Delete(url) {
    Swal.fire({
        title: "Bạn có chắc không?",
        text: "Course sẽ bị xóa và không thể hoàn tác.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Xóa",
        cancelButtonText: "Hủy"
    }).then(function (result) {
        if (!result.isConfirmed) {
            return;
        }
        $.ajax({
            url: url,
            type: "DELETE",
            success: function (data) {
                if (data.success) {
                    Swal.fire({
                        title: "Thành công!",
                        text: data.message,
                        icon: "success",
                        timer: 1500,
                        showConfirmButton: false
                    });
                    courseDataTable.ajax.reload();
                } else {
                    Swal.fire("Lỗi", data.message, "error");
                }
            },
            error: function () {
                Swal.fire("Lỗi", "Không xóa được Course.", "error");
            }
        });
    });
}

function Publish(id) {

    Swal.fire({

        title: "Publish Course?",

        text: "Course sẽ được hiển thị cho người học.",

        icon: "question",

        showCancelButton: true,

        confirmButtonText: "Publish",

        cancelButtonText: "Hủy"

    }).then(function (result) {

        if (result.isConfirmed) {

            $.ajax({

                url: "/Admin/Course/Publish/" + id,

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

                        courseDataTable.ajax.reload();

                    }

                }

            });

        }

    });

}
function Unpublish(id) {

    Swal.fire({

        title: "Unpublish Course?",

        text: "Course sẽ trở về trạng thái Draft.",

        icon: "warning",

        showCancelButton: true,

        confirmButtonText: "Unpublish",

        cancelButtonText: "Hủy"

    }).then(function (result) {

        if (result.isConfirmed) {

            $.ajax({

                url: "/Admin/Course/Unpublish/" + id,

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

                        courseDataTable.ajax.reload();

                    }

                }

            });

        }

    });

}
