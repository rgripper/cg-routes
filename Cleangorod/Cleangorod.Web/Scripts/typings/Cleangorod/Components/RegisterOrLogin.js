define(["require", "exports"], function (require, exports) {
    var RegisterOrLoginViewModel = (function () {
        function RegisterOrLoginViewModel() {
            this.email = ko.observable("");
        }
        RegisterOrLoginViewModel.prototype.submit = function () {
            $.post("/Account/RegisterOrLoginWithoutPassword", { email: this.email() });
        };
        return RegisterOrLoginViewModel;
    })();
    exports.RegisterOrLoginViewModel = RegisterOrLoginViewModel;
});
//# sourceMappingURL=RegisterOrLogin.js.map