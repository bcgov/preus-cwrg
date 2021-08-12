app.controller('ApplicationEvaluationSummary', function ($scope, $attrs, $controller, $timeout, Utils, ngDialog) {
  $scope.section = {
    name: 'ApplicationEvaluationSummary',
    displayName: 'Summary',
    save: {
      url: '/Int/Application/Evaluation/Summary',
      method: 'PUT',
      data: function () {
        return $scope.model;
      },
      backup: true
    },
    loaded: function () {
      return $scope.model && $scope.model.RowVersion && $scope.grantFile && $scope.model.RowVersion === $scope.grantFile.RowVersion;
    },
    onSave: function () {
      window.location = '/Int/Application/Evaluation/View/' + $scope.parent.grantApplicationId;
    }
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  /**
   * Make AJAX request for summary data
   * @function loadSummary
   * @returns {void}
   **/
  function loadSummary() {
    return $scope.load({
      url: '/Int/Application/Evaluation/Summary/' + $scope.parent.grantApplicationId,
      set: 'model'
    });
  }

  /**
   * Initialize the data for the form
   * @function init
   * @returns {void{
   **/
  $scope.init = function() {
    return Promise.all([
      loadSummary()
    ]).catch(angular.noop);
  }
  $scope.init();
});
