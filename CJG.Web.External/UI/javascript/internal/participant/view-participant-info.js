app.controller('ViewParticipantInfo', function ($scope, $attrs, $controller) {
  $scope.section = {
    name: 'ViewParticipantInfo'
  };

  angular.extend(this, $controller('Section', { $scope, $attrs }));

  $scope.showTab = 'contact';

  $scope.toggleEmployerInfo = function (tabName) {
    $scope.showTab = tabName;
  };
});
