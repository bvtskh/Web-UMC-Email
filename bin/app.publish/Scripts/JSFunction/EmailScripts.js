$(document).ready(function () {
    var currentPage = 1;
    var pageSize = 40;

    $('#searchBox').on('input', function () {
        var query = $(this).val();
        currentPage = 1; // Reset page to 1 when performing new search
        loadData(query, currentPage, pageSize);
    });

    function loadData(searchTerm, page, pageSize) {
        $.ajax({
            url: '/Email/Search',
            type: 'GET',
            data: { searchTerm: searchTerm, page: page, pageSize: pageSize },
            success: function (response) {
                $('#dataTable tbody').empty();
                $.each(response.allData, function (index, item) {
                    index++;
                    $('#dataTable tbody').append('<tr class="data-row"><td>' + index + '</td><td translate="no">' + item.DEPARTMENT + '</td><td translate="no">' + item.NAME + '</td><td translate="no" class="data-email">' + '<strong>' + item.EMAIL + '</strong>' +
                        '<input class="form-check-input" type="checkbox" style = "margin :0 15px ;   transform: scale(1.8);  margin-top: 5px;" value="" id="flexCheckDefault">'
                        + '</td></tr>');
                });
                /*Duyệt qua từng phần tử của danh sách dữ liệu để kiểm tra checkbox trong bảng*/
                response.data.forEach(function (item) {
                    $('.data-row').each(function () {
                        var rowText = $(this).find('.data-email').text().trim();

                        var itemEmail = rowText.split(' ')[0];
                        if (itemEmail === item.EMAIL) {
                            $(this).find('.form-check-input').prop('checked', true);
                        }
                    });
                    ActionLoading(response);
                });

                // Update pagination
                $('#pagination ul').empty();
                var totalPages = response.totalPages;
                var maxPagesToShow = 10; // Số lượng trang tối đa được hiển thị
                var startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
                var endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);

                var previous = currentPage - 1;
                if (previous == 1) previous = 1;

                var next = currentPage + 1;
                if (next == 1) next = 1;

                $('#pagination ul').append('<li class="page-item active"><button class="page-link page"  data-page="' + previous + '">Previous</button></li>');
                for (var i = startPage; i <= endPage; i++) {
                    $('#pagination ul').append('<li class="page-item"><button class="page-link page" data-page="' + i + '">' + i + '</button></li>');
                }
                $('#pagination ul').append('<li class="page-item active"><button class="page-link page" data-page="' + next + '">Next</button></li>')
            }
        });
    }

    // Load initial data
    loadData('', currentPage, pageSize);

    // Pagination click event
    $(document).on('click', '.page', function () {
        var page = $(this).data('page');
        currentPage = page;
        var query = $('#searchBox').val();
        loadData(query, currentPage, pageSize);
    });
    // Checkbox change event
    $('#dataTable').on('change', '.form-check-input', function () {

        var isChecked = $(this).is(':checked');
        var row = $(this).closest('tr');
        var rowData = {
            DEPARTMENT: row.find('td:eq(1)').text(), // Lấy dữ liệu từ cột đầu tiên (index 0)
            NAME: row.find('td:eq(2)').text(), // Lấy dữ liệu từ cột thứ hai (index 1)
            EMAIL: row.find('td:eq(3)').text() // Lấy dữ liệu từ cột thứ ba (index 2)
        };
        $.ajax({
            url: '/Email/ProcessCheckbox',
            type: 'POST',
            data: { isChecked: isChecked, dataRow: rowData },
            success: function (response) {
                ActionLoading(response);
            },
            error: function (xhr, status, error) {
                console.log("error!");
            }
        });
    });
    function ActionLoading(response) {

        var count = response.data.length;
        var data = response.data;
        var text = '';
        var getEmail = data.map(item => item.EMAIL).join('; ');
        $('#copyMail').click(function () {
            copyToClipboard(getEmail);
        });


        if (data.length > 3) {
            var temp = count - 3;
            $('#numberCount').html(data[0].NAME + ', ' + data[1].NAME + ', ' + data[2].NAME + ' và ' + "<a href='#' id='otherNamesLink'>" + temp + " người khác</a>"); // Cập nhật số lượng
        }
        else {
            data.forEach(function (item) {
                text += item.NAME + ', ';
            });
            text = text.slice(0, -2);
            $('#numberCount').text(text);
        }
        if (data.length <= 0) {
            $('#numberCount').text('0 Email');
        }

        // Sự kiện click cho link label 'những người khác'
        var otherNames = "";
        for (var i = 3; i < count; i++) {
            otherNames += ", " + data[i].NAME;
        }

        $('#otherNamesLink').click(function () {
            alert("Những người khác: " + otherNames.substring(2)); // Hiển thị hộp thoại với tên những người khác
        });


    }
    $('#deleteAll').click(function () {
        $.ajax({
            type: 'POST',
            url: '/Email/clearSession', // URL đến controller để xóa session
            success: function (res) {
                var currentSearch = $("#searchBox").val();
                loadData(currentSearch, 1, 10);
                $('#numberCount').text('0 Email');
            },
            error: function (error) {
                console.error('Error:', error);
            }
        });
    });
    function copyToClipboard(text) {
        var tempInput = document.createElement("input");
        tempInput.style.position = "absolute";
        tempInput.style.left = "-9999px";
        tempInput.value = text;
        document.body.appendChild(tempInput);
        tempInput.select();
        document.execCommand("copy");
        document.body.removeChild(tempInput);
    }
    // Function để mở modal và truyền dữ liệu vào
    function openModalWithData(data) {
        var modalBody = $('#modalBody');
        modalBody.empty(); // Xóa nội dung hiện tại của modal body

        // Thêm dữ liệu từ controller vào modal body
        data.forEach(function (item) {
            var rowHtml = '<div class="row mb-3"><div class="col">' + item.EMAIL + ' (' + item.NAME + ')' + '   (' + item.DEPARTMENT +')'+ '</div><div class="col-auto"><button type="button" class="btn btn-danger delete-btn" data-id="' + item.EMAIL + '">Remove</button></div></div>'; // Tạo HTML cho mỗi hàng, bao gồm tên và nút Xóa
            modalBody.append(rowHtml); // Thêm hàng vào modal body
        });

        // Mở modal
        $('#myModal').modal('show');
        // Thêm sự kiện click cho nút Xóa
        $('.delete-btn').click(function () {
            var email = $(this).data('id'); // Lấy ID của phần tử cần xóa từ thuộc tính data-id
            // Gọi hàm xóa phần tử (ví dụ: bằng AJAX)
            deleteItem(email);
        });
    }

    // Sự kiện click của nút mở modal
    $('#openModalBtn').click(function () {
        // Gọi AJAX để lấy dữ liệu từ controller
        $.ajax({
            type: 'GET',
            url: '/Email/GetData',
            success: function (response) {
                openModalWithData(response); // Mở modal và truyền dữ liệu vào khi nhận được phản hồi từ controller
            },
            error: function (error) {
                console.error('Error:', error);
            }
        });
        $('#closeModalBtn').click(function () {
            // Đóng modal
            $('#myModal').modal('hide');
            var currentSearch = $("#searchBox").val();
            loadData(currentSearch, 1, 40);
        });
        // Thêm sự kiện click cho nút "X" bằng jQuery
        $('.modal .close').click(function () {
            // Đóng modal khi nút "X" được nhấp vào
            $('#myModal').modal('hide');
            var currentSearch = $("#searchBox").val();
            loadData(currentSearch, 1, 40);
        });
    });


    // Function để xóa phần tử
    function deleteItem(email) {
        $.ajax({
            type: 'POST',
            url: '/Email/DeleteItem',
            data: { email: email }, // Dữ liệu gửi đi (ID của phần tử cần xóa)
            success: function (response) {
                // Xử lý phản hồi từ controller (nếu cần)
                console.log("Phản hồi từ controller:", response);
                refreshList();
            },
            error: function (error) {
                console.error('Lỗi khi gửi yêu cầu AJAX:', error);
            }
        });
    }
    // Function để làm mới danh sách sau khi xóa thành công
    function refreshList() {
        // Gọi AJAX để lấy danh sách mới từ controller
        $.ajax({
            type: 'GET',
            url: '/Email/GetData', // Đường dẫn tới action trong controller để lấy danh sách mới
            success: function (response) {
                // Sau khi nhận được danh sách mới, cập nhật giao diện người dùng
                openModalWithData(response); // Ví dụ: mở modal với danh sách mới
            },
            error: function (error) {
                console.error('Lỗi khi làm mới danh sách:', error);
            }
        });
    }
});