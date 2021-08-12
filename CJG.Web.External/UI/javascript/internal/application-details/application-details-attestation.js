app.controller('Attestation', function ($scope, $attrs, $controller) {
  $scope.section = {
    name: 'Attestation',
    displayName: 'Participant Financial Supports Attestation',
    save: {
      url: '/Int/Application/Attestation/',
      method: 'PUT',
      data: function () {
        return $scope.model;
      },
      backup: true
    },
    onShow: function () {
      return angular.noop;
    },
    loaded: function () {
      return $scope.model && $scope.model.RowVersion && $scope.model.RowVersion === $scope.grantFile.RowVersion;
    },
    onSave: function () {
      $scope.emit('refresh', { target: 'ApplicationNotes', force: true });
    },
    onRefresh: function () {
      return loadAttestationDetails()
        .catch(angular.noop);
    },
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  /**
   * Make AJAX request to fetch attestation data
   * @function loadAttachments
   * @returns {Promise}
   **/
  function loadAttestationDetails() {
    return $scope.load({
      url: '/Int/Application/Attestation/' + $scope.parent.grantApplicationId,
      set: 'model'
    });
  }

  /**
   * Initialize section data.
   * @function init
   * @returns {Promise}
   **/
  $scope.init = function () {
    return Promise.all([
      loadAttestationDetails()
    ]).catch(angular.noop);
  }

  $scope.toggleAttestation = function () {
    return $scope.ajax({
      url: '/Int/Application/Attestation/Toggle/' + $scope.parent.grantApplicationId + '?rowVersion=' + encodeURIComponent($scope.grantFile.RowVersion),
      method: 'PUT'
    }).then(function (response) {
      $scope.emit('update', {
        grantFile: {
          AttestationState: response.data.AttestationState,
          AttestationStateDescription: response.data.AttestationStateDescription,
          RowVersion: response.data.RowVersion
        }
      });
      $scope.emit('refresh', { target: 'ApplicationNotes', force: true });
    }).catch(angular.noop);
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
});
