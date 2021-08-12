app.controller('AttestationView', function ($scope, $attrs, $controller) {
  $scope.section = {
    name: 'AttestationView',
    displayName: 'Participant Financial Supports Attestation',
    save: {
      url: function () {
        return '/Ext/Application/Attestation/' + $scope.section.grantApplicationId;
      },
      method: 'PUT',
      data: function () {
        return $scope.model;
      },
      backup: true
    },
    loaded: function () {
      return $scope.model && $scope.model.RowVersion && $scope.model.RowVersion === $scope.grantFile.RowVersion;
    },
    onSave: function () {
      window.location = $scope.section.redirectUrl;
    },
    onRefresh: function () {
      return loadAttestation().catch(angular.noop);
    },
    onCancel: function () {
    },
    grantApplicationId: $attrs.ngGrantApplicationId,

    redirectUrl: $attrs.ngRedirectUrl,
    attachments: []
  };

  $scope.grantFile = {
    Id: $attrs.grantApplicationId
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  /**
   * Make AJAX request to fetch Attestation data
   * @function loadAttachments
   * @returns {Promise}
   **/
  function loadAttestation() {
    return $scope.load({
      url: '/Ext/Application/Attestation/' + $scope.section.grantApplicationId,
      set: 'model'
    });
  }

  /**
   * Initialize form data.
   * @function init
   * @returns {Promise}
   **/
  function init() {
    return Promise.all([
      loadAttestation()
    ]).catch(angular.noop);
  }

  $scope.recalculateAttestation = function () {
    let allocatedCosts = $scope.model.AllocatedCosts;
    if (isNaN(allocatedCosts) || allocatedCosts < 0)
      allocatedCosts = 0;

    let newFunds = $scope.model.ClaimedCosts - allocatedCosts;
    if (newFunds < 0)
      newFunds = 0;

    $scope.model.UnusedFunds = newFunds;
  };

  $scope.cancel = function () {
    window.location = $scope.section.redirectUrl;
  }

  init();
});
