require('./participant-funding-sources-items');

app.controller('ParticipantFundingSourcesManagement', function ($scope, $attrs, $controller) {
  $scope.section = {
    name: 'ParticipantFundingSourcesManagement'
  };

  angular.extend(this, $controller('ParentSection', { $scope: $scope, $attrs: $attrs }));

  function init() {
    return Promise.all([
      ])
      .then(function() {
        $scope.broadcast('show', { target: 'ParticipantFundingSourcesItems' });
      })
      .catch(angular.noop);
  }

  init();
});
