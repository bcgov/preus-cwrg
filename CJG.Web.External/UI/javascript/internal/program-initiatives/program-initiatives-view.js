require('./program-initiatives-items');

app.controller('ProgramInitiativesManagement', function ($scope, $attrs, $controller) {
  $scope.section = {
    name: 'ProgramInitiativesManagement'
  };

  angular.extend(this, $controller('ParentSection', { $scope: $scope, $attrs: $attrs }));

  function init() {
    return Promise.all([
      ])
      .then(function() {
        $scope.broadcast('show', { target: 'ProgramInitiativesItems' });
      })
      .catch(angular.noop);
  }

  init();
});
