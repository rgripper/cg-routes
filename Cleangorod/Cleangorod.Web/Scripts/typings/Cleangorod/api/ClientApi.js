define(["require", "exports"], function (require, exports) {
    var ClientApi = (function () {
        function ClientApi() {
        }
        ClientApi.postSchedule = function (data) {
            return $.ajax({
                url: "/api/Client/PostSchedule",
                data: JSON.stringify(data),
                type: "POST",
                contentType: "application/json"
            });
        };
        ClientApi.getSchedule = function () {
            return $.get("/api/Client/GetSchedule");
        };
        return ClientApi;
    })();
    return ClientApi;
});
//# sourceMappingURL=ClientApi.js.map