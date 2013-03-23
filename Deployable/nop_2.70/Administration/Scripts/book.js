$(document).ready(function () {
    $(".push").click(function () {
        var parent = $(this).parent().parent();
        moveValue(parent);
    });
    $(".push-all").click(function () {
        var table = $(this).parents("table");
        $.each(table.find("tr"), function () {
            moveValue($(this));
        });
    });
    updateImg();
    function updateImg() {
        var field1 = $("#PictureUrl");
        var imgFld1 = field1.parents("td").find("img");
        imgFld1.attr("src", field1.val());

        var field2 = $(".picture");
        var imgFld2 = field2.parents("td").find("img");
        imgFld2.attr("src", field2.val());
    }

    function moveValue(parent) {
        var destField = parent.find("td:eq(1) input, td:eq(1) textarea");
        var srcField = parent.find("td:eq(3) input, td:eq(3) textarea");
        if (destField.length > 0 && srcField.length > 0) {
            destField.val(srcField.val());
            updateImg();
        }
    }

    $("#g-btn").click(function () {
        var isbn = $("#ISBN").val();
        if (isbn === undefined || isbn.length == 0) {
            alert("¬ведите ISBN!");
            return;
        }
        $.ajax({
            url: "https://www.googleapis.com/books/v1/volumes?q=" + isbn,
            dataType: 'json',
            success: function (data) {
                if (data === undefined)
                    return;

                clearForm();

                if (data.totalItems > 0) {
                    var item = data.items[0];
                    var imageLink = (item.volumeInfo.imageLinks != undefined
						&& item.volumeInfo.imageLinks.thumbnail != undefined)
							? item.volumeInfo.imageLinks.thumbnail
							: "";

                    fillFields({
                        title: item.volumeInfo.title,
                        authors: item.volumeInfo.authors.join(';'),
                        publisher: item.volumeInfo.publisher,
                        year: item.volumeInfo.publishedDate,
                        isbns: jQuery.map(item.volumeInfo.industryIdentifiers, function (x) {
                            return x.identifier;
                        }).join(';'),
                        description: item.volumeInfo.description,
                        pageCount: item.volumeInfo.pageCount,
                        imageLink: imageLink,
                        language: item.volumeInfo.language
                    });
                    updateImg();
                }
            }
        });

        function fillFields(fields) {
            var serviceFields = $(".service");
            serviceFields.filter('.title').val(fields.title);
            serviceFields.filter('.description').val(fields.description);
            serviceFields.filter('.picture').val(fields.imageLink);
            serviceFields.filter('.Author').val(fields.authors);
            serviceFields.filter('.Year').val(fields.year);
            serviceFields.filter('.ISBN').val(fields.isbns);
            serviceFields.filter('.Language').val(fields.language);
            serviceFields.filter('.Publisher').val(fields.publisher);
            serviceFields.filter('.Pages').val(fields.pageCount);
            serviceFields.filter('.Series').val(fields.series);
            serviceFields.filter('.Format').val(fields.format);
            serviceFields.filter('.Weight').val(fields.weight);
            serviceFields.filter('.Cover').val(fields.cover);
        }

        function clearForm() {
            $(".service").each(function () {
                $(this).val("");
            });
            updateImg();
        }
    });

    $("#ll-btn").click(function () {
        $.get(
	        "http://www.ozon.ru/?context=search&text=%e0%ea%f3%f8%e5%f0+%f5%e0",
	        function (responseText) {
	            alert(responseText);
	        }
	    );
    });
});