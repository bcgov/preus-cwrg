app.controller('EvaluationClaimQuestions', function ($scope, $attrs, $controller, $timeout, Utils) {
  $scope.section = {
    name: 'EvaluationClaimQuestions',
    save: {
      url: function () {
        return '/Int/Admin/EvaluationForm/ClaimQuestions';
      },
      method: 'PUT',
      data: function () {
        return $scope.claimModel;
      },
      backup: true
    },
    onSave: function () {
      loadQuestions();
      $scope.emit('refresh');
    },
    loaded: function () {
      return $scope.section.isLoaded;
    }
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  function loadQuestionTypes() {
    return $scope.load({
      url: '/Int/Admin/EvaluationForm/ClaimQuestionTypes',
      set: 'claimQuestionTypes'
    });
  }

  function loadQuestions() {
    return $scope.load({
      url: '/Int/Admin/EvaluationForm/ClaimQuestions',
      set: 'claimModel'
    });
  }

  function init() {
    return Promise.all([
      loadQuestionTypes(),
      loadQuestions()
    ]).catch(angular.noop);
  }

  init();

  $scope.createQuestion = function () {
    if ($scope.claimModel.Questions == null)
      $scope.claimModel.Questions = [];

    let nextSequence = $scope.claimModel.Questions.length + 1;

    $scope.claimModel.Questions.push({
      Id: 0,
      Text: "",
      ClaimEvaluationFormQuestionType: 1,
      RowSequence: nextSequence
    });
    $scope.renumberQuestions(1);
  }

  $scope.renumberQuestions = function (increment) {
    $scope.claimModel.Questions.sort(function (a, b) { return a.RowSequence - b.RowSequence });
    for (var aIdx = 0; aIdx < $scope.claimModel.Questions.length; aIdx++) {
      $scope.claimModel.Questions[aIdx].RowSequence = (aIdx + 1) * increment;
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
    const index = $scope.claimModel.Questions.indexOf(question);

    if (index > -1)
      $scope.claimModel.Questions.splice(index, 1);

    $scope.renumberQuestions(1);
  }
});
