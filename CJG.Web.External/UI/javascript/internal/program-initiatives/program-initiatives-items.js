app.controller('ProgramInitiativesItems', function ($scope, $attrs, $controller, $timeout, Utils) {
  $scope.section = {
    name: 'ProgramInitiativesItems',
    save: {
      url: function () {
        return '/Int/Admin/ProgramInitiatives/Initiatives';
      },
      method: 'PUT',
      data: function () {
        return $scope.model;
      },
      backup: true
    },
    onSave: function () {
      $scope.emit('refresh');
    },
    loaded: function () {
      return $scope.section.isLoaded;
    }
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  function loadInitiatives() {
    return $scope.load({
      url: '/Int/Admin/ProgramInitiatives/Initiatives',
      set: 'model'
    });
  }

  /**
 * Initialize the data for the form
 * @function init
 * @returns {void{
 **/
  function init() {
    return Promise.all([
        loadInitiatives()
      ])
      .catch(angular.noop);
  }

  init();

  $scope.createInitiative = function () {
    if ($scope.model.Initiatives == null)
      $scope.model.Initiatives = [];

    let nextSequence = $scope.model.Initiatives.length + 1;

    $scope.model.Initiatives.push({
      Id: 0,
      Name: "",
      Code: "",
      RowSequence: nextSequence
    });
    $scope.renumberInitiatives(1);
  }

  $scope.renumberInitiatives = function (increment) {
    $scope.model.Initiatives.sort(function (a, b) { return a.RowSequence - b.RowSequence });
    for (var aIdx = 0; aIdx < $scope.model.Initiatives.length; aIdx++) {
      $scope.model.Initiatives[aIdx].RowSequence = (aIdx + 1) * increment;
    }
  }

  $scope.moveInitiative = function (initiative, up) {
    // Renumber by 100's. To move up or down, simply add or subtract 101, then renumber.
    $scope.renumberInitiatives(100);
    if (up === 1)
      initiative.RowSequence -= 101;
    else
      initiative.RowSequence += 101;

    $scope.renumberInitiatives(1);
  }

  $scope.removeItem = function (initiative) {
    const index = $scope.model.Initiatives.indexOf(initiative);

    if (index > -1)
      $scope.model.Initiatives.splice(index, 1);

    $scope.renumberInitiatives(1);
  }
});
