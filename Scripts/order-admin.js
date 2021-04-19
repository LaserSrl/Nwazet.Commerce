$(function() {
    $(".address-view").click(function (e) {
        var view = $(e.target).hide();
        view.siblings(".address-form").show();
    });

    $(".order-tracking input").each(function () {
        var input = $(this),
            container = input.closest(".order-tracking");
        if (input.val() !== "") {
            container.find(".order-tracking-view").show();
            container.find(".order-tracking-edit").hide();
        }
    });
    
    $(".order-tracking button.edit-button").click(function (e) {
        e.preventDefault();
        var button = $(e.target),
            container = button.closest(".order-tracking"),
            input = container.find(".order-tracking-edit-field");
        container.find(".order-tracking-view").hide();
        container.find(".order-tracking-edit").show();
        input.data("previous-value", input.val());
    });

    $(".order-tracking button.cancel-button").click(function (e) {
        e.preventDefault();
        var button = $(e.target),
            container = button.closest(".order-tracking"),
            input = container.find(".order-tracking-edit-field");
        container.find(".order-tracking-view").show();
        container.find(".order-tracking-edit").hide();
        input.val(input.data("previous-value"));
    });

    $(".new-order-event-add").click(function (e) {
        e.preventDefault();
        var htmlTemplate = $('#order-event-log-item-template').html();
        var button = $(e.target),
            container = button.closest(".order-events"),
            url = container.data("add-event-url"),
            orderId = container.data("order-id"),
            category = container.find(".new-order-event-category:checked").val() || "Note",
            descriptionElement = container.find(".new-order-event-description"),
            description = descriptionElement.val(),
            token = $("input[name='__RequestVerificationToken']").val();
        if (description) {
            $.post(url, {
                orderId: orderId,
                category: category,
                description: description,
                __RequestVerificationToken: token
            }, function (data) {
                if (htmlTemplate == "") {
                    container.find("ul.order-event-log")
                        .prepend($("<li></li>")
                            .append($("<h3></h3>").html(data.Date + " (" + data.Category + ")"))
                            .append($("<p></p>").html(data.Description)));
                } else {
                    var htmlPreprend = htmlTemplate
                        .replace("{cssclass}", data.Category.toLowerCase().replace(" ", "-"))
                        .replace("{date}", data.Date)
                        .replace("{category}", data.Category)
                        .replace("{text}", data.Description)
                        .replace("{author}", "");
                    container.find("ul.order-event-log")
                        .prepend(htmlPreprend)
                }
                descriptionElement.val("");
            });
        }
    });
})