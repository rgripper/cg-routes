require.config({
    baseUrl: "/Scripts/typings/Cleangorod",

    paths: {
        //main libraries
        jquery: '/Scripts/jquery-2.1.3.min',
 
        //shortcut paths
        knockout: '/Scripts/knockout-3.3.0',

        leaflet: '/Scripts/leaflet-0.7.3',

        bootstrap: '/Scripts/bootstrap',

        text: '/Scripts/text',
        //signalR: '../../jquery.signalR-2.2.0',

        //signalRHubs: '/signalr/hubs?',
    },
    shim: {
        jquery: {
            exports: "$"
        },
        bootstrap: {
            deps: ["jquery"],
        },
        //signalR: {
        //    deps: ["jquery"],
        //    exports: "$.connection"
        //},
        //signalRHubs: {
        //    deps: ["signalR"],
        //},
        application: {
            deps: ["text", "knockout"],
        }
    }
});

require(["application"], function () { });