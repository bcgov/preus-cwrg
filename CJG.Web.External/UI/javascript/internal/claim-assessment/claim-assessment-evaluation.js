app.controller('ClaimAssessmentEvaluation', function ($scope, $attrs, $controller) {
  $scope.section = {
    name: 'ClaimAssessmentEvaluation',
    save: {
      url: function () {
        return '/Int/Claim/Evaluation/Questions';
      },
      method: 'PUT',
      data: function () {
        return $scope.model;
      },
      backup: true
    },
    onSave: function () {
      window.location = '/Int/Claim/Assessment/View/' + $scope.parent.claimId + '/' + $scope.parent.claimVersion;
    },
    loaded: function () {
      return $scope.section.isLoaded;
    }
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  function loadQuestionTypes() {
    return $scope.load({
      url: '/Int/Claim/Evaluation/QuestionTypes',
      set: 'questionTypes'
    });
  }

  function loadQuestions() {
    return $scope.load({
      url: '/Int/Claim/Evaluation/Questions/' + $scope.parent.claimId + '/' + $scope.parent.claimVersion,
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
