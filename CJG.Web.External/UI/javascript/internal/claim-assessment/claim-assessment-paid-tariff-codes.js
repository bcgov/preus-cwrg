app.controller('ClaimAssessmentPaidTariffCodes', function ($scope, $attrs, $controller) {
  $scope.section = {
    name: 'ClaimAssessmentPaidTariffCodes',
    save: {
      url: function () {
        return '/Int/Claim/PaymentsAndTariffCodes';
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

  function loadPaymentsAndTariffs() {
    return $scope.load({
      url: '/Int/Claim/PaymentsAndTariffCodes/' + $scope.parent.claimId + '/' + $scope.parent.claimVersion,
      set: 'model'
    });
  }

  function init() {
    return Promise.all([
      loadPaymentsAndTariffs()
    ]).catch(angular.noop);
  }

  init();
});
