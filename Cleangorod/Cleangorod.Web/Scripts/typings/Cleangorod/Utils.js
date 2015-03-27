define(["require", "exports", "knockout"], function (require, exports, ko) {
    var ObservableExecutionWrapper = (function () {
        function ObservableExecutionWrapper() {
        }
        ObservableExecutionWrapper.observe = function (value) {
            if (typeof value === "function") {
                return ObservableExecutionWrapper.observeFunction(value);
            }
            else {
                Object.keys(value).filter(function (x) { return typeof value[x] === "function"; }).forEach(function (x) { return value[x] = ObservableExecutionWrapper.observeFunction(value[x]); });
                return value;
            }
        };
        ObservableExecutionWrapper.observeFunction = function (func) {
            var isExecuting = ko.observable(false);
            var wrapper = function () {
                isExecuting(true);
                var result = func.apply(this, arguments);
                if (result && typeof result.always === "function") {
                    return result.always(function () { return isExecuting(false); });
                }
                else {
                    isExecuting(false);
                    return result;
                }
            };
            wrapper["isExecuting"] = isExecuting;
            return wrapper;
        };
        return ObservableExecutionWrapper;
    })();
    exports.observeExec = ObservableExecutionWrapper.observe;
    exports.resettableObservable = function (value, beforeFadeTimeout, fadeTimeout) {
        var observable = ko.observable(value);
        var fadingObservable = observable["fading"] = ko.observable(false);
        var timeoutHandle = null;
        observable.subscribe(function (x) {
            if (x !== value) {
                clearTimeout(timeoutHandle);
                timeoutHandle = setTimeout(function () {
                    fadingObservable(true);
                    timeoutHandle = setTimeout(function () {
                        fadingObservable(false);
                        observable(value);
                    }, fadeTimeout);
                }, beforeFadeTimeout);
            }
        });
        return observable;
    };
    function dateDaysEqual(x, y) {
        return x.getDate() === y.getDate() && x.getMonth() === y.getMonth() && x.getFullYear() === y.getFullYear();
    }
    exports.dateDaysEqual = dateDaysEqual;
});
//export function getDateOnly(x: Date): Date {
//    return new Date(x.getFullYear(), x.getMonth(), x.getDate());
//} 
//# sourceMappingURL=Utils.js.map