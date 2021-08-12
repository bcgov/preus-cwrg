app.controller('ApplicationEvaluationPrint', function ($scope, $attrs, $controller, $timeout) {
  $scope.section = {
    name: 'ApplicationEvaluationPrint',
    grantApplicationId: $attrs.ngGrantApplicationId,
    version: $attrs.ngVersion
  };

  angular.extend(this, $controller('ParentSection', { $scope: $scope, $attrs: $attrs }));

  /**
   * Make AJAX request to load grant agreement data.
   * @function loadGrantAgreement
   * @returns {Promise}
   **/
  function loadEvaluationSummary() {
    return $scope.load({
      url: '/Int/Application/Evaluation/Summary/' + $scope.section.grantApplicationId,
      set: 'summary'
    });
  }

  function loadEvaluationQuestions() {
    return $scope.load({
      url: '/Int/Application/Evaluation/QuestionsPrint/' + $scope.section.grantApplicationId,
      set: 'questions'
    });
  }


  /**
   * Fetch all the data for the form.
   * @function init
   * @returns {Promise}
   **/
  function init() {
    $('body').addClass("print-mode");

    return Promise.all([
        loadEvaluationSummary(),
        loadEvaluationQuestions()
      ])
      .then(function() {
        window.print();
      })
      .catch(angular.noop);
  }

  init();
});
