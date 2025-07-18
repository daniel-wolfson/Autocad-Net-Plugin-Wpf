L.Control.Sample = L.Control.extend({
  options: {
    // topright, topleft, bottomleft, bottomright
    position: 'topright'
  },
  initialize: function (options) {
    // constructor
  },
  onAdd: function (map) {
    // happens after added to map
  },
  onRemove: function (map) {
    // when removed
  }
});

L.control.sample = function(id, options) {
  return new L.Control.Sample(id, options);
}