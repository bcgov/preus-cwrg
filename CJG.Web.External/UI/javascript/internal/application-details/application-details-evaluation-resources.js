app.controller('EvaluationResources', function ($scope, $attrs, $controller, $timeout, Utils, ngDialog) {
  $scope.section = {
    name: 'EvaluationResources',
    displayName: 'Evaluation Resources',
    attachments: []
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  function loadAttachments() {
    return $scope.load({
      url: '/Int/Application/Evaluation/Resources',
      set: 'model'
    });
  }

  $scope.init = function () {
    return Promise.all([
      loadAttachments()
    ]).catch(angular.noop);
  }
});
