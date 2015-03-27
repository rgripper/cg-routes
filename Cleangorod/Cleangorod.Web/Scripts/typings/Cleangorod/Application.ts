import $ = require("jquery");
import ko = require("knockout");
import L = require("leaflet");

ko.bindingHandlers["enter"] = {
    init: function (element: HTMLElement, valueAccessor) {
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
    init: function (element: HTMLElement, valueAccessor) {
        element.addEventListener("dragstart", function (ev) {
            ev.dataTransfer.setData("Text", ko.unwrap(valueAccessor()));
        });
    }
};

ko.bindingHandlers["leaflet"] = {
    init: function (element: HTMLElement, valueAccessor) {
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

ko.extenders["validator"] = function (target, validationFunc: (x) => string) {
    target.errorMessage = ko.observable();
    target.hasError = ko.computed(() => target.errorMessage() ? true : false);

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

function addComponent(localName: string) {
    var fileName = localName.split("-").map(x => x[0].toUpperCase() + x.substring(1)).join("");
    var prefix = "cg";
    var componentsPath = "components";
    ko.components.register(prefix + "-" + localName, {
        viewModel: { require: componentsPath + "\\" + fileName },
        template: { require: "text!" + componentsPath + "\\" + fileName + ".html" }
    });

}

function addComponents(names: string[]) { names.forEach(addComponent); }

addComponents(["auth", "client-address", "client-schedule", "router", "collection-calendar", "collection-schedule"]);

export class Application {
    public context = ko.observable<ApplicationContext>();

    public router: Router;

    constructor(context: ApplicationContext) {
        this.router = new Router("/Home/Index/",(context: ApplicationContext) => {
            if (context.identity.roles.indexOf("Admin") > -1) return "cg-collection-schedule";
            if (context.identity.roles.indexOf("Client") > -1) return "cg-client-schedule";
            return "";
        }, this.context);
        this.context(context);
    }
}

class Router implements IRouter {

    public componentName: KnockoutObservable<string>;

    public params = ko.observable(Router.parseQuery(location.search));

    private componentNamePrefix = "cg";

    constructor(private prefix: string, private defaultNameFactory: (context: ApplicationContext) => string,
        contextObservable: KnockoutObservable<ApplicationContext>) {
        this.componentName = ko.observable("");
        this.componentName.subscribe(x => history.pushState({ mainComponentName: x }, "", this.getRoute(x)));
        contextObservable.subscribe(x => this.componentName(this.getComponentName(x)));
    }

    private getRoute(componentName: string) {
        return this.prefix + componentName.substring(this.componentNamePrefix.length + 1) + location.search;
    }

    private getComponentName(context: ApplicationContext) {
        if (location.pathname && location.pathname !== "/") {
            return this.componentNamePrefix + "-" + location.pathname.substring(this.prefix.length);
        }
        else {
            return this.defaultNameFactory(context);
        }
    }

    private static parseQuery(queryString: string): { get(name: string): string; } {
        var data: { [key: string]: { name: string; value: string; } } = {};

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
        return { get: (name: string) => data[name.toLowerCase()] ? data[name.toLowerCase()].value : null };
    }


}

$.get("/api/Account/GetApplicationContext")
    .done(x => ko.applyBindings(new Application(x)))
    .fail(x => alert("App failed with " + x));