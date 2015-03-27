define(["require", "exports", "knockout", "jquery", "Utils"], function (require, exports, ko, $, Utils) {
    var AuthViewModel = (function () {
        function AuthViewModel(params) {
            var _this = this;
            this.email = ko.observable("");
            this.errorMessage = ko.observable("");
            this.actions = Utils.observeExec({
                auth: function () {
                    if (_this.context().identity.isAuthenticated) {
                        return $.post("/api/Account/Logout").done(function () {
                            document.cookie = ".AspNet.ApplicationCookie" + "=;Max-Age=0;domain=;";
                            _this.getApplicationContext();
                        });
                    }
                    else {
                        var email = _this.email();
                        if (!email) {
                            throw new Error("email");
                        }
                        return $.post("/api/Account/RegisterOrLoginWithoutPassword", { email: email }).done(function () {
                            _this.errorMessage("");
                            _this.getApplicationContext();
                        }).fail(function (xhr) {
                            _this.getApplicationContext();
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
                }
            });
            if (params.context === null || params.context === undefined) {
                throw new TypeError("isAuthenticated");
            }
            this.context = params.context;
            var email = this.context().identity.name || "";
            var urlEmail = params.router.params().get("email");
            if (!email && urlEmail) {
                email = urlEmail;
                this.email(email);
                this.actions.auth();
            }
            else {
                this.email(email);
            }
        }
        AuthViewModel.prototype.getApplicationContext = function () {
            var _this = this;
            return $.get("/api/Account/GetApplicationContext").done(function (x) { return _this.context(x); }).fail(function (x) { return _this.errorMessage("Ошибка"); });
        };
        return AuthViewModel;
    })();
    return AuthViewModel;
});
//# sourceMappingURL=Auth.js.map