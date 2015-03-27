define(["require", "exports", "jquery", "knockout", "leaflet"], function (require, exports, $, ko, L) {
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
    ko.bindingHandlers["dragData"] = {
        init: function (element, valueAccessor) {
            element.addEventListener("dragstart", function (ev) {
                ev.dataTransfer.setData("Text", ko.unwrap(valueAccessor()));
            });
        }
    };
    ko.bindingHandlers["leaflet"] = {
        init: function (element, valueAccessor) {
            valueAccessor()(new L.Map(element));
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
    ko.extenders["validator"] = function (target, validationFunc) {
        target.errorMessage = ko.observable();
        target.hasError = ko.computed(function () { return target.errorMessage() ? true : false; });
        target.validate = function () {
            var value = target();
            var message = validationFunc(value);
            target.errorMessage(message);
        };
        target.subscribe(function (value) {
            var message = validationFunc(value);
            target.errorMessage(message);
        });
        return target;
    };
    function addComponent(localName) {
        var fileName = localName.split("-").map(function (x) { return x[0].toUpperCase() + x.substring(1); }).join("");
        var prefix = "cg";
        var componentsPath = "components";
        ko.components.register(prefix + "-" + localName, {
            viewModel: { require: componentsPath + "\\" + fileName },
            template: { require: "text!" + componentsPath + "\\" + fileName + ".html" }
        });
    }
    function addComponents(names) {
        names.forEach(addComponent);
    }
    addComponents(["auth", "client-address", "client-schedule", "router", "collection-calendar", "collection-schedule"]);
    var Application = (function () {
        function Application(context) {
            this.context = ko.observable();
            this.router = new Router("/Home/Index/", function (context) {
                if (context.identity.roles.indexOf("Admin") > -1)
                    return "cg-collection-schedule";
                if (context.identity.roles.indexOf("Client") > -1)
                    return "cg-client-schedule";
                return "";
            }, this.context);
            this.context(context);
        }
        return Application;
    })();
    exports.Application = Application;
    var Router = (function () {
        function Router(prefix, defaultNameFactory, contextObservable) {
            var _this = this;
            this.prefix = prefix;
            this.defaultNameFactory = defaultNameFactory;
            this.params = ko.observable(Router.parseQuery(location.search));
            this.componentNamePrefix = "cg";
            this.componentName = ko.observable("");
            this.componentName.subscribe(function (x) { return history.pushState({ mainComponentName: x }, "", _this.getRoute(x)); });
            contextObservable.subscribe(function (x) { return _this.componentName(_this.getComponentName(x)); });
        }
        Router.prototype.getRoute = function (componentName) {
            return this.prefix + componentName.substring(this.componentNamePrefix.length + 1) + location.search;
        };
        Router.prototype.getComponentName = function (context) {
            if (location.pathname && location.pathname !== "/") {
                return this.componentNamePrefix + "-" + location.pathname.substring(this.prefix.length);
            }
            else {
                return this.defaultNameFactory(context);
            }
        };
        Router.parseQuery = function (queryString) {
            var data = {};
            if (queryString && queryString.length > 0) {
                var queryWithoutQuestionMark = queryString.charAt(0) === "?" ? queryString.substring(1) : queryString;
                var pairs = queryWithoutQuestionMark.split("&");
                for (var i = 0; i < pairs.length; i++) {
                    var pair = pairs[i].split("=");
                    data[decodeURIComponent(pair[0]).toLowerCase()] = {
                        name: decodeURIComponent(pair[0]),
                        value: decodeURIComponent(pair[1])
                    };
                }
            }
            return { get: function (name) { return data[name.toLowerCase()] ? data[name.toLowerCase()].value : null; } };
        };
        return Router;
    })();
    $.get("/api/Account/GetApplicationContext").done(function (x) { return ko.applyBindings(new Application(x)); }).fail(function (x) { return alert("App failed with " + x); });
});
//# sourceMappingURL=Application.js.map