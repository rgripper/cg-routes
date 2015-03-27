define(["require", "exports", "knockout", "jquery", "Utils"], function (require, exports, ko, $, Utils) {
    var LoginViewModel = (function () {
        function LoginViewModel(params) {
            var _this = this;
            this.email = ko.observable("");
            this.errorMessage = ko.observable("");
            this.submit = Utils.observeExec(function () {
                if (_this.isAuthenticated()) {
                    return $.post("/api/Account/Logout");
                }
                else {
                    var email = _this.email();
                    if (!email) {
                        throw new Error("email");
                    }
                    return $.post("/api/Account/RegisterOrLoginWithoutPassword", { email: email }).done(function () {
                        _this.errorMessage();
                        _this.isAuthenticated(true);
                    }).fail(function (xhr) {
                        _this.isAuthenticated(false);
                        switch (xhr.status) {
                            case 404:
                                _this.errorMessage("Пользователь не найден");
                                break;
                            default:
                                _this.errorMessage("Ошибка");
                                break;
                        }
                    });
                }
            });
            this.isAuthenticated = params.isAuthenticated;
            if (this.isAuthenticated === null || this.isAuthenticated === undefined) {
                throw new TypeError("isAuthenticated");
            }
        }
        return LoginViewModel;
    })();
    return LoginViewModel;
});
//# sourceMappingURL=Login.js.map