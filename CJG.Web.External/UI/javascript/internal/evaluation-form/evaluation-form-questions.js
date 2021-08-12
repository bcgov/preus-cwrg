app.controller('EvaluationFormQuestions', function ($scope, $attrs, $controller, $timeout, Utils) {
  $scope.section = {
    name: 'EvaluationFormQuestions',
    save: {
      url: function () {
        return '/Int/Admin/EvaluationForm/Questions';
      },
      method: 'PUT',
      data: function () {
        return $scope.model;
      },
      backup: true
    },
    onSave: function () {
      $scope.emit('refresh');
    },
    loaded: function () {
      return $scope.section.isLoaded;
    }
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  function loadQuestionTypes() {
    return $scope.load({
      url: '/Int/Admin/EvaluationForm/QuestionTypes',
      set: 'questionTypes'
    });
  }

  function loadQuestions() {
    return $scope.load({
      url: '/Int/Admin/EvaluationForm/Questions',
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

  $scope.createQuestion = function () {
    if ($scope.model.Questions == null)
      $scope.model.Questions = [];

    let nextSequence = $scope.model.Questions.length + 1;

    $scope.model.Questions.push({
      Id: 0,
      Text: "",
      EvaluationFormQuestionType: 1,
      RowSequence: nextSequence
    });
    $scope.renumberQuestions(1);
  }

  $scope.renumberQuestions = function (increment) {
    $scope.model.Questions.sort(function (a, b) { return a.RowSequence - b.RowSequence });
    for (var aIdx = 0; aIdx < $scope.model.Questions.length; aIdx++) {
      $scope.model.Questions[aIdx].RowSequence = (aIdx + 1) * increment;
    }
  }

  $scope.moveQuestion = function (question, up) {
    // Renumber by 100's. To move up or down, simply add or subtract 101, then renumber.
    $scope.renumberQuestions(100);
    if (up === 1)
      question.RowSequence -= 101;
    else
      question.RowSequence += 101;

    $scope.renumberQuestions(1);
  }

  $scope.removeItem = function (question) {
    const index = $scope.model.Questions.indexOf(question);

    if (index > -1)
      $scope.model.Questions.splice(index, 1);

    $scope.renumberQuestions(1);
  }
});
