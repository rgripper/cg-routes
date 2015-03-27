import ko = require("knockout");
import $ = require("jquery");
import Utils = require("Utils");

class AuthViewModel {

    public email = ko.observable("");

    public errorMessage = ko.observable("");

    private context: KnockoutObservable<ApplicationContext>;

    constructor(params: { context: KnockoutObservable<ApplicationContext>; router: IRouter; }) {

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

    public actions = Utils.observeExec({
        auth: () => {
            if (this.context().identity.isAuthenticated) {
                return $.post("/api/Account/Logout").done(() => {
                    document.cookie = ".AspNet.ApplicationCookie" + "=;Max-Age=0;domain=;";
                    this.getApplicationContext();
                });
            }
            else {
                var email = this.email();
                if (!email) {
                    throw new Error("email");
                }
                return $.post("/api/Account/RegisterOrLoginWithoutPassword", { email: email })
                    .done(() => {
                    this.errorMessage("");
                    this.getApplicationContext();
                })
                    .fail((xhr: JQueryXHR) => {
                    this.getApplicationContext();
                    switch (xhr.status) {
                        case 404:
                            this.errorMessage("Пользователь не найден");
                            break;
                        default:
                            this.errorMessage("Ошибка");
                            break;
                    }
                });
            }
        }
    });

    public getApplicationContext() {
        return $.get("/api/Account/GetApplicationContext")
            .done(x => this.context(x))
            .fail(x => this.errorMessage("Ошибка"));
    }
}

export = AuthViewModel