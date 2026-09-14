var wordDataTable;

$(document).ready(function () {

    loadWordDataTable();

    $("#btnFilter").click(function () {

        wordDataTable.ajax.reload();

    });

    $("#searchInput").keypress(function (event) {

        if (event.which === 13) {

            wordDataTable.ajax.reload();

        }

    });

});


function loadWordDataTable() {

    wordDataTable = $("#wordTable").DataTable({

        ajax: {

            url: "/Admin/Word/GetAll",

            type: "GET",

            data: function (data) {

                data.search =
                    $("#searchInput").val();

                data.partOfSpeech =
                    $("#partOfSpeechFilter").val();

            }

        },


        columns: [

            // IMAGE

            {
                data: "imageUrl",

                width: "10%",

                orderable: false,

                render: function (data) {

                    if (!data) {

                        return `

                            <div class="bg-light
                                        rounded
                                        d-flex
                                        align-items-center
                                        justify-content-center"
                                 style="
                                    width:70px;
                                    height:60px;
                                 ">

                                <i class="bi bi-image
                                          text-muted
                                          fs-4"></i>

                            </div>

                        `;

                    }

                    return `

                        <img src="${data}"
                             class="rounded"
                             style="
                                width:70px;
                                height:60px;
                                object-fit:cover;
                             "
                             onerror="
                                this.style.display='none';
                             " />

                    `;

                }

            },


            // WORD

            {
                data: "wordText",

                width: "15%",

                render: function (data, type, row) {

                    return `

                        <div>

                            <div class="fw-bold fs-5">
                                ${data}
                            </div>

                            <small class="text-muted">
                                ID: ${row.id}
                            </small>

                        </div>

                    `;

                }

            },


            // PRONUNCIATION

            {
                data: "pronunciation",

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

                        <span class="text-muted">
                            ${data}
                        </span>

                    `;

                }

            },


            // PART OF SPEECH

            {
                data: "partOfSpeech",

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


            // DEFINITION

            {
                data: "definition",

                width: "25%",

                render: function (data) {

                    if (!data) {

                        return `
                            <span class="text-muted">
                                Chưa có định nghĩa
                            </span>
                        `;

                    }

                    var text = data;

                    if (text.length > 80) {

                        text =
                            text.substring(0, 80)
                            + "...";

                    }

                    return text;

                }

            },


            // AUDIO

            {
                data: "audioUrl",

                width: "8%",

                orderable: false,

                className: "text-center",

                render: function (data) {

                    if (!data) {

                        return `
                            <span class="text-muted">
                                -
                            </span>
                        `;

                    }

                    return `

                        <button type="button"
                                class="btn btn-sm btn-outline-primary"
                                onclick="playAudio('${data}')"
                                title="Play Audio">

                            <i class="bi bi-volume-up"></i>

                        </button>

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

                            <a href="/Admin/Word/Details/${data}"
                               class="btn btn-sm btn-info text-white"
                               title="Details">

                                <i class="bi bi-eye"></i>

                            </a>


                            <a href="/Admin/Word/Upsert/${data}"
                               class="btn btn-sm btn-primary"
                               title="Edit">

                                <i class="bi bi-pencil-square"></i>

                            </a>


                            <button type="button"
                                    onclick="Delete('/Admin/Word/Delete/${data}')"
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
                "Không có từ vựng nào",

            search:
                "Tìm kiếm:",

            lengthMenu:
                "Hiển thị _MENU_ dòng",

            info:
                "Hiển thị _START_ đến _END_ trong tổng số _TOTAL_ từ",

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

            [1, "asc"]

        ]

    });

}


// ======================================
// PLAY AUDIO
// ======================================

function playAudio(url) {

    if (!url) {

        return;

    }

    var audio =
        new Audio(url);

    audio.play();

}


// ======================================
// DELETE
// ======================================

function Delete(url) {

    Swal.fire({

        title: "Bạn có chắc không?",

        text:
            "Từ vựng sẽ bị xóa và không thể hoàn tác!",

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

                        wordDataTable.ajax.reload();

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
                        "Không thể xóa từ vựng.",
                        "error"
                    );

                }

            });

        }

    });

}