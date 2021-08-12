var utils = require('../../shared/utils.js');

require('./application-evaluation-summary');
require('./application-evaluation-questions');

app.controller('ApplicationEvaluation', function ($scope, $attrs, $controller, $timeout, Utils, ngDialog) {
  $scope.section = {
    name: 'ApplicationEvaluation',
    onRefresh: function () {
      return loadApplicationDetails().catch(angular.noop);
    },
    resources: []
  };

  $scope.parent = {
    grantApplicationId: $attrs.grantApplicationId,
    editing: null,
};

  $scope.submitEvaluation = function () {
    return $scope.ajax({
      url: '/Int/Application/Evaluation/Submit/' + $attrs.grantApplicationId,
        method: 'PUT'
      })
      .then(function (response) {
        if (response.data.RedirectURL)
          window.location = response.data.RedirectURL;
      })
      .catch(angular.noop);
  }

  $scope.withdrawEvaluation = function () {
    return $scope.ajax({
      url: '/Int/Application/Evaluation/Withdraw/' + $attrs.grantApplicationId,
        method: 'PUT'
      })
      .then(function (response) {
        if (response.data.RedirectURL)
          window.location = response.data.RedirectURL;
      })
      .catch(angular.noop);
  }

  $scope.returnToEvaluation = function() {
    window.location = '/Int/Application/Details/View/' + $attrs.grantApplicationId;
  }

  $scope.printEvaluation = function() {
    window.location = '/Int/Application/Evaluation/Print/' + $attrs.grantApplicationId;
  }

  angular.extend(this, $controller('ParentSection', { $scope: $scope, $attrs: $attrs }));

  function loadResources() {
    return $scope.load({
      url: '/Int/Application/Evaluation/Resources',
      set: 'resources'
    });
  }

  /**
   * Make AJAX request to load application details data.
   * @function loadApplicationDetails
   * @param {bool} [set=true] - Whether to set a scope level variable.
   * @returns {Promise}
   **/
  function loadApplicationDetails(set) {
    if (typeof (set) === 'undefined')
      set = true;
    return $scope.load({
      url: '/Int/Application/Details/' + $attrs.grantApplicationId,
      set: set ? 'grantFile' : null
    });
  }

  /**
   * Fetch all the data for the form.
   * @function init
   * @returns {Promise}
   **/
  function init() {
    return Promise.all([
        loadResources(),
        loadApplicationDetails()
      ])
      .then(function() {
        $scope.broadcast('show', { target: 'ApplicationEvaluationSummary, ApplicationEvaluationQuestions, ApplicationEvaluationNotes' });
      })
      .catch(angular.noop);
  }

  $scope.wrapLines = function (text) {
    return Utils.replaceAll(text, '\\n', '<br />');
  }

  init();
});
