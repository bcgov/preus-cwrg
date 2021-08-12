require('./evaluation-form-questions');
require('./evaluation-form-attachments');
require('./evaluation-claim-questions');

app.controller('EvaluationFormManagement', function ($scope, $attrs, $controller) {
  $scope.section = {
    name: 'EvaluationFormManagement'
  };

  angular.extend(this, $controller('ParentSection', { $scope: $scope, $attrs: $attrs }));

  function init() {
    return Promise.all([
    ]).catch(angular.noop);
  }

  init();
});
