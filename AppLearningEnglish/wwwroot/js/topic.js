var topicDataTable;

$(document).ready(function () {
  loadTopicDataTable();
});

function loadTopicDataTable() {
  topicDataTable = $("#topicTable").DataTable({
    ajax: {
      url: "/Admin/Topic/GetAll",
      type: "GET",
    },

    columns: [
      {
        data: "name",
        width: "25%",
      },

      {
        data: "description",
        width: "45%",
      },

      {
        data: "id",
        width: "30%",
        className: "text-end",

        render: function (data) {
          return `
                        <div class="d-flex justify-content-end gap-2">

                            <a href="/Admin/Topic/Upsert/${data}"
                               class="btn btn-sm btn-success">

                                <i class="bi bi-pencil-square"></i>
                                Edit

                            </a>

                            <button type="button"
                                    onclick="Delete('/Admin/Topic/Delete/${data}')"
                                    class="btn btn-sm btn-danger">

                                <i class="bi bi-trash"></i>
                                Delete

                            </button>

                        </div>
                    `;
        },
      },
    ],

    language: {
      emptyTable: "Không có Topic nào",

      search: "Tìm kiếm:",

      lengthMenu: "Hiển thị _MENU_ dòng",

      info: "Hiển thị _START_ đến _END_ trong tổng số _TOTAL_ Topic",

      paginate: {
        first: "Đầu",

        last: "Cuối",

        next: "Sau",

        previous: "Trước",
      },
    },

    responsive: true,

    pageLength: 10,
  });
}

function Delete(url) {
  Swal.fire({
    title: "Bạn có chắc không?",

    text: "Topic sẽ bị xóa và không thể hoàn tác!",

    icon: "warning",

    showCancelButton: true,

    confirmButtonText: "Xóa",

    cancelButtonText: "Hủy",
  }).then(function (result) {
    if (result.isConfirmed) {
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

              showConfirmButton: false,
            });

            topicDataTable.ajax.reload();
          } else {
            Swal.fire("Lỗi", data.message, "error");
          }
        },

        error: function () {
          Swal.fire("Lỗi", "Có lỗi xảy ra khi xóa Topic.", "error");
        },
      });
    }
  });
}
