define(["require", "exports"], function (require, exports) {
    var AdminApi = (function () {
        function AdminApi() {
        }
        AdminApi.postDateSelection = function (data) {
            return $.ajax({
                url: "/api/Admin/PostDateSelection",
                data: JSON.stringify(data),
                type: "POST",
                contentType: "application/json"
            });
        };
        AdminApi.getSelectedDates = function (data) {
            return $.get("/api/Admin/GetSelectedDates", { startDate: data.startDate.toISOString(), endDate: data.endDate.toISOString() });
        };
        AdminApi.getRouteClientsForDate = function (data) {
            return $.get("/api/Admin/GetRouteClientsForDate", { date: data.date.toISOString() });
        };
        return AdminApi;
    })();
    return AdminApi;
});
//# sourceMappingURL=AdminApi.js.map