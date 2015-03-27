define(["require", "exports", "knockout", "leaflet", "Utils", "api/AdminApi"], function (require, exports, ko, L, Utils, AdminApi) {
    var RouterViewModel = (function () {
        function RouterViewModel() {
            var _this = this;
            this.nodes = ko.observableArray([]);
            this.isLoading = ko.observable(false);
            this.keyWatchers = {
                ctrl: ko.observable(false),
            };
            this.settings = {
                draggableNodes: ko.pureComputed(function () { return false; }),
                connectableNodes: ko.pureComputed(function () { return _this.keyWatchers.ctrl(); }),
            };
            this.imagePath = "/content/images";
            this.icons = {
                selected: new L.Icon(this.createIconOptions(this.imagePath + "/marker-icon-green.png")),
                default: new L.Icon(this.createIconOptions(this.imagePath + "/marker-icon.png"))
            };
            this.selectedNode = ko.observable();
            this.hoveredNode = ko.observable();
            this.mapData = {
                map: ko.observable(),
                layers: {
                    tileLayer: L.tileLayer('http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                        attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
                    }),
                    nodesLayer: new L.FeatureGroup(),
                    connectionsLayer: new L.FeatureGroup(),
                }
            };
            this.actions = Utils.observeExec({
                loadClients: function (date) {
                    return AdminApi.getRouteClientsForDate({ date: date }).done(function (model) {
                        //var dummyPoints = [[37.545905, 55.714605], [37.470608, 55.669183], [37.512191, 55.627905], [37.545905, 55.714605], [37.456639, 55.717802], [37.550217, 55.543262], [37.571399, 55.705559], [37.630571, 55.823029], [37.457124, 55.720408], [37.863720, 55.739184], [37.450629, 55.828855], [37.462936, 55.778046], [37.555777, 55.666746], [37.630418, 55.723354], [37.604547, 55.597464], [37.722801, 55.559725], [37.405600, 55.751363], [37.405067, 55.751609], [37.570644, 55.817699], [37.397323, 55.906375], [37.599741, 55.754735], [37.524360, 55.545003], [37.525691, 55.543757], [37.590713, 55.659698], [37.613997, 55.657271], [37.497611, 55.645456], [37.568012, 55.769185], [37.666926, 55.638343], [37.676879, 55.626660], [37.544530, 55.774568], [37.749634, 55.825007], [37.495581, 55.690258], [37.406298, 55.747900], [37.713728, 55.649550], [37.508535, 55.716215], [37.458409, 55.830513], [37.473280, 55.568252], [37.472926, 55.568288], [37.072924, 55.545752], [37.348195, 55.849908], [37.723259, 55.811902], [37.341870, 55.845865], [37.887741, 55.792250], [37.918023, 55.700299], [37.901377, 55.723095], [37.398204, 55.743831], [37.618022, 55.631768]];
                        var nodes = model.map(function (x) { return _this.createNode(x.latitude, x.longitude); });
                        nodes.forEach(function (x, i) { return x.next(nodes[i + 1]); });
                        nodes.forEach(function (x) { return x.marker.on("click", function (ev) { return _this.selectedNode(x); }); });
                        ko.utils.arrayPushAll(_this.nodes, nodes);
                    });
                }
            });
            this.mapData.map.subscribe(function (newMap) {
                Object.keys(_this.mapData.layers).forEach(function (x) { return newMap.addLayer(_this.mapData.layers[x]); });
                _this.initMarkerProperties(newMap);
                _this.actions.loadClients(new Date());
                setTimeout(function () { return newMap.invalidateSize(null); }, 1000);
            });
            document.addEventListener("keydown", function (ev) {
                if (ev.which === 17) {
                    _this.keyWatchers.ctrl(true);
                }
            });
            document.addEventListener("keyup", function (ev) {
                if (ev.which === 17) {
                    _this.keyWatchers.ctrl(false);
                }
            });
        }
        RouterViewModel.prototype.createIconOptions = function (iconUrl) {
            return {
                iconSize: new L.Point(25, 41),
                iconAnchor: new L.Point(12, 41),
                popupAnchor: new L.Point(1, -34),
                shadowSize: new L.Point(41, 41),
                iconUrl: iconUrl,
            };
        };
        RouterViewModel.prototype.initMarkerProperties = function (map) {
            var _this = this;
            this.nodes.subscribe(function (changes) {
                changes.forEach(function (change) {
                    switch (change.status) {
                        case "added":
                            map.addLayer(change.value.marker);
                            return;
                        case "deleted":
                            map.removeLayer(change.value.marker);
                            return;
                    }
                });
            }, null, "arrayChange");
            this.selectedNode.subscribeChanged(function (newValue, oldValue) {
                if (newValue === oldValue)
                    return;
                if (oldValue)
                    oldValue.marker.setIcon(_this.icons.default);
                newValue.marker.setIcon(_this.icons.selected);
            });
            this.nodes.subscribe(function () {
                var markers = _this.nodes().map(function (x) { return x.marker; });
                if (markers.length < 1)
                    return;
                var bounds = new L.FeatureGroup(markers).getBounds();
                map.fitBounds(bounds);
            });
        };
        RouterViewModel.prototype.allowNodeAcceptDrop = function (node) {
            var viewModel = this;
            var tmpAdd = node.marker.onAdd;
            node.marker.onAdd = function (map) {
                tmpAdd.call(this, map);
                var element = this["_icon"];
                element.addEventListener("dragover", function (ev) {
                    var selected = viewModel.selectedNode();
                    if (selected && node !== selected) {
                        ev.preventDefault();
                    }
                });
                element.addEventListener("drop", function (ev) {
                    ev.preventDefault();
                    var direction = ev.dataTransfer.getData("Text");
                    if (direction === "from") {
                        node.next(viewModel.selectedNode());
                    }
                    else if (direction === "to") {
                        viewModel.selectedNode().next(node);
                    }
                    else {
                        throw new Error("Unknown drop event");
                    }
                });
            };
        };
        RouterViewModel.prototype.createNode = function (latitude, longitude) {
            var _this = this;
            var coords = new L.LatLng(latitude, longitude);
            var node = {
                marker: observableMarker(new L.Marker(coords, { icon: this.icons.default, draggable: false })),
                next: ko.observable(),
                nextLine: ko.observable(),
            };
            this.allowNodeAcceptDrop(node);
            this.settings.draggableNodes.subscribe(function (x) { return node.marker.draggable(x); });
            //node.marker.hovered.subscribe(x => x ? node.marker.setIcon(this.icons.selected) : node.marker.setIcon(this.icons.default));
            var polylineBinder;
            node.next.subscribeChanged(function (newNextNode, oldNextNode) {
                if (oldNextNode === newNextNode)
                    return;
                if (!newNextNode) {
                    _this.mapData.layers.connectionsLayer.removeLayer(node.nextLine());
                    node.nextLine(null);
                    polylineBinder.dispose();
                    return;
                }
                if (!oldNextNode) {
                    var line = new L.Polyline([node.marker.latLng(), newNextNode.marker.latLng()]);
                    line.setStyle({ color: "#00f", opacity: 0.6 });
                    _this.mapData.layers.connectionsLayer.addLayer(line);
                    node.nextLine(line);
                }
                var nextLine = node.nextLine();
                if (polylineBinder)
                    polylineBinder.dispose();
                polylineBinder = ko.computed(function () { return nextLine.setLatLngs([node.marker.latLng(), newNextNode.marker.latLng()]); });
            });
            return node;
        };
        return RouterViewModel;
    })();
    function observableMarker(marker) {
        var obsMarker = marker;
        obsMarker.latLng = ko.observable(obsMarker.getLatLng());
        //obsMarker.latLng.subscribe(x => obsMarker.setLatLng(x));
        obsMarker.on("drag", function () { return obsMarker.latLng(obsMarker.getLatLng()); });
        obsMarker.draggable = ko.observable(obsMarker.dragging ? obsMarker.dragging.enabled() : false);
        obsMarker.draggable.subscribe(function (x) { return x ? obsMarker.dragging.enable() : obsMarker.dragging.disable(); });
        obsMarker.hovered = ko.observable(false);
        obsMarker.on("mouseover", function () { return obsMarker.hovered(true); });
        obsMarker.on("mouseout", function () { return obsMarker.hovered(false); });
        return obsMarker;
    }
    return RouterViewModel;
});
//# sourceMappingURL=Router.js.map