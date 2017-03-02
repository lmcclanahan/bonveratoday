// Product module
define(["jquery", "pubsub", "bootstrap", "extensions"], function ($) {

    var module = {
        bind: function (context) { 
            var $context = $(context); 

            // Items
            $context.on('click', '[data-role="item.addtoorder"]', function () {
                var item = helpers.formatItem($context.serializeObject());
                item.Type = 0;

                window.trigger($(this).data('role'), item);
            });
            $context.on('click', '[data-role="item.addtoautoorder"]', function () {
                var item = helpers.formatItem($context.serializeObject());
                item.Type = 1;

                window.trigger($(this).data('role'), item);
            });
            $context.on('click', '[data-role="item.customizedynamickit"]', function () {
                var item = helpers.formatItem($context.serializeObject());

                $context.find('.modal').modal('show');
            });

            // Carts
            $context.on('change', '[data-role="cart.updateitemquantity"]', function () {
                var item = helpers.formatItem({
                    ID: $(this).data('id'),
                    Quantity: $(this).val()
                });

                window.trigger($(this).data('role'), item);
            });
            $context.on('click', '[data-role="cart.removeitemfromcart"]', function () {
                window.trigger($(this).data('role'), $(this).data('id'));
            });
        }
    };

    var helpers = {
        formatItem: function (item) {
            // Quantity checking
            item.Quantity = item.Quantity || 0;
            if (isNaN(item.Quantity)) item.Quantity = 0;
            else item.Quantity = parseFloat(item.Quantity);

            return item;
        }
    };

    return module;

});