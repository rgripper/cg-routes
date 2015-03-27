define(["require", "exports", "knockout"], function (require, exports, ko) {
    ko.bindingHandlers["enter"] = {
        init: function (element, valueAccessor) {
            element.addEventListener("keydown", function (e) {
                if (e.which === 13) {
                    var event = document.createEvent("Event");
                    event.initEvent("change", true, true);
                    e.target.dispatchEvent(event);
                    valueAccessor();
                    e.preventDefault();
                }
            });
        }
    };
    ko.subscribable.fn["subscribeChanged"] = function (callback) {
        var oldValue;
        this.subscribe(function (_oldValue) {
            oldValue = _oldValue;
        }, this, 'beforeChange');
        var subscription = this.subscribe(function (newValue) {
            callback(newValue, oldValue);
        });
        // always return subscription
        return subscription;
    };
    var Application = (function () {
        function Application() {
        }
        return Application;
    })();
    exports.Application = Application;
});
//# sourceMappingURL=App.js.map