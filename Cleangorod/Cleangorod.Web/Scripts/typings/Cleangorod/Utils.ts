import ko = require("knockout");

class ObservableExecutionWrapper {
    public static observe<T>(value: T): T {
        if (typeof value === "function") {
            return ObservableExecutionWrapper.observeFunction(<any>value);
        }
        else {
            Object
                .keys(value)
                .filter(x => typeof value[x] === "function")
                .forEach(x => value[x] = ObservableExecutionWrapper.observeFunction(value[x]));

             return value;
        }
    }

    private static observeFunction<T extends Function>(func: T): T {
        var isExecuting = ko.observable(false);
        var wrapper = function () {
            isExecuting(true);
            var result = func.apply(this, arguments);
            if (result && typeof result.always === "function") {
                return (<JQueryXHR>result).always(() => isExecuting(false));
            }
            else {
                isExecuting(false);
                return result;
            }
        }
        wrapper["isExecuting"] = isExecuting;
        return <any>wrapper;
    }
}

export var observeExec = ObservableExecutionWrapper.observe;

export var resettableObservable = function <T>(value: T, beforeFadeTimeout, fadeTimeout) {
    var observable = ko.observable<T>(value);
    var fadingObservable = observable["fading"] = ko.observable(false);
    var timeoutHandle = null;
    observable.subscribe(x => {
        if (x !== value) {
            clearTimeout(timeoutHandle);
            timeoutHandle = setTimeout(() => {
                fadingObservable(true); 
                timeoutHandle = setTimeout(() => {
                    fadingObservable(false);
                    observable(value);
                }, fadeTimeout);
            }, beforeFadeTimeout);
        }
    });
    return observable;
};

export function dateDaysEqual(x: Date, y: Date): boolean {
    return x.getDate() === y.getDate()
        && x.getMonth() === y.getMonth()
        && x.getFullYear() === y.getFullYear();
}

//export function getDateOnly(x: Date): Date {
//    return new Date(x.getFullYear(), x.getMonth(), x.getDate());
//}