var controlAngular = L.control.angular({
    position: 'topleft',
    templateUrl: '/MapIt/templates/mapIt_search.html',
    controllerAs: 'leaflet',
    controller: ["$compile", "$rootScope", "$scope", "$filter", "config", searchController]
    //controller: function ($scope, $element, $map, ExampleService) {
    //    var that = this;
    //    this.latlng = null;
    //    this.message = ExampleService.message;
    //    this.zoom = function () {
    //        $map.zoomIn();
    //    };

    //    $map.on('click', function (e) {
    //        that.latlng = e.latlng;
    //        $scope.$apply();
    //    });
    //}
});

map.addControl(controlAngular);