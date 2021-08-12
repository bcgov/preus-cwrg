app.controller('ApplicationEvaluationQuestions', function ($scope, $attrs, $controller) {
  $scope.section = {
    name: 'ApplicationEvaluationQuestions',
    save: {
      url: function () {
        return '/Int/Application/Evaluation/Questions';
      },
      method: 'PUT',
      data: function () {
        return $scope.model;
      },
      backup: true
    },
    onSave: function () {
      window.location = '/Int/Application/Evaluation/View/' + $scope.parent.grantApplicationId;
    },
    loaded: function () {
      return $scope.section.isLoaded;
    }
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  //$scope.$watch('$parent.model', function (newValue, oldValue) {
  //  $scope.model = newValue;
  //});

  function loadQuestionTypes() {
    return $scope.load({
      url: '/Int/Application/Evaluation/QuestionTypes',
      set: 'questionTypes'
    });
  }

  function loadQuestions() {
    return $scope.load({
      url: '/Int/Application/Evaluation/Questions/' + $scope.parent.grantApplicationId,
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
      loadQuestionTypes(),
      loadQuestions()
    ]).catch(angular.noop);
  }

  init();
});
