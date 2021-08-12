app.controller('ParticipantInvitationView', function ($scope, $attrs, $controller, $timeout, Utils, ngDialog) {

  $scope.section = {
    name: 'ParticipantInvitationView',
    grantApplicationId: $attrs.ngGrantApplicationId,
    showInvitation: false
  };

  angular.extend(this, $controller('Section', { $scope, $attrs }));

  function loadModel() {
    return $scope.load({
      url: '/Ext/Reporting/InvitationInfo/' + $scope.section.grantApplicationId,
      set: 'model'
    });
  }

  function init() {
    return Promise.all([
        loadModel()
    ])
      .catch(angular.noop);
  }
  
  init();
});
